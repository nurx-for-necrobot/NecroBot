using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Event;

namespace PoGo.NecroBot.CLI.Nurx.SenderResponders
{
    class FortUsedSender : INurxMessageSender
    {
        // Private vars.
        private Session _pogoSession;
        private NurxService _service;


        /// <summary>
        /// Hook the events for spun pokestop to pipe them out to listeners.
        /// </summary>
        /// <param name="pogoSession">Necro pogo session.</param>
        /// <param name="service">Nurx service instance.</param>
        public void RegisterSender(Session pogoSession, NurxService service)
        {
            _pogoSession = pogoSession;
            _service = service;

            // Register hooks for FortUsedEvent events.
            _service.RegisterHook(typeof(FortUsedEvent), FortUsed);

        }

        /// <summary>
        /// Callback for spun pokestop.
        /// </summary>
        /// <param name="evt">Event instance</param>
        private void FortUsed(IEvent evt)
        {
            FortUsedEvent e = (FortUsedEvent)evt;
            _service.Broadcast("fortused", e);
        }
    }
}
