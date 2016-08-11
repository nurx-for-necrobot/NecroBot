using System;
using System.Collections.Generic;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Event;
using POGOProtos.Inventory.Item;
using PoGo.NecroBot.Logic.Logging;
using SuperSocket.WebSocket;

namespace PoGo.NecroBot.CLI.Nurx.SenderResponders
{
    public class NurxInventoryData
    {
        public NurxInventoryData(ItemData data)
        {
            Base = data;
        }

        public ItemData Base { get; set; }
    }

    class InventoryListSenderResponder : INurxMessageSender, INurxMessageResponder
    {
        // Private vars.
        private Session _pogoSession;
        private NurxService _service;
        private List<NurxInventoryData> _currentList = new List<NurxInventoryData>();
        public Object _lck = new Object();


        // Public properties.
        public string Command { get { return "inventorylist"; } }


        /// <summary>
        /// Register the sender with Nurx.
        /// </summary>
        /// <param name="pogoSession">NecroBot session instnace.</param>
        /// <param name="service">Nurx service instance.</param>
        public void RegisterSender(Session pogoSession, NurxService service)
        {
            _pogoSession = pogoSession;
            _service = service;


            _service.RegisterHook(typeof(InventoryListEvent), EventDispatcher_EventReceived);
        }


        /// <summary>
        /// Register the responder to the nurx service.
        /// </summary>
        /// <param name="pogoSession">NecroBot session instnace.</param>
        /// <param name="service">Nurx service instance.</param>
        public void RegisterResponder(Session pogoSession, NurxService service)
        {
            _pogoSession = pogoSession;
            _service = service;
            _service.RegisterResponder(Command, this);
        }


        /// <summary>
        /// Handle received messages, send item list to requester.
        /// </summary>
        /// <param name="command">Nurx command instance.</param>
        /// <param name="wsSession">Requesting websockets session.</param>
        public async void MessageReceived(NurxCommand command, WebSocketSession wsSession)
        {

            if (_currentList.Count == 0)
            {
                var items = await _pogoSession.Inventory.GetItems();

                lock (_lck)
                {
                    _currentList = new List<NurxInventoryData>();
                    foreach (var i in items) { _currentList.Add(new NurxInventoryData(i.Clone())); }
                }
            }

            _service.Send(wsSession, "inventorylist", _currentList);
        }


        /// <summary>
        /// Handle updates in item list.
        /// </summary>
        /// <param name="evt">Event instance.</param>
        private void EventDispatcher_EventReceived(IEvent e)
        {
            InventoryListEvent evt = (InventoryListEvent)e;
            
            Logger.Write("Sending item list data to websockets clients.", LogLevel.Info);
            InventoryListEvent pEvt = (InventoryListEvent)evt;

            lock (_lck)
            {
                _currentList = new List<NurxInventoryData>();
                pEvt.Items.ForEach(o => _currentList.Add(new NurxInventoryData(o.Clone())));
            }

            _service.Broadcast("inventorylist", _currentList);
        }
    }
}
