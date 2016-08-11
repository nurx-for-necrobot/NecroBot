using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Event;

namespace PoGo.NecroBot.CLI.Nurx.SenderResponders
{
    class RecycleSender : INurxMessageSender
    {
        // Private vars.
        private Session _pogoSession;
        private NurxService _service;


        /// <summary>
        /// Hook the events for recycling to pipe them out to listeners.
        /// </summary>
        /// <param name="pogoSession">Necro pogo session.</param>
        /// <param name="service">Nurx service instance.</param>
        public void RegisterSender(Session pogoSession, NurxService service)
        {
            _pogoSession = pogoSession;
            _service = service;

            // Register hooks for recycle events.
            _service.RegisterHook(typeof(ItemRecycledEvent), ItemRecycled);

        }

        /// <summary>
        /// Callback for recycle encounter.
        /// </summary>
        /// <param name="evt">Event instance</param>
        private void ItemRecycled(IEvent evt)
        {
            ItemRecycledEvent e = (ItemRecycledEvent)evt;
            _service.Broadcast("recycled_item", e);
        }
    }
}
