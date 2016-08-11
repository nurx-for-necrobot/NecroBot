using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Event;

namespace PoGo.NecroBot.CLI.Nurx.SenderResponders
{
    class EncounterSender : INurxMessageSender
    {
        // Private vars.
        private Session _pogoSession;
        private NurxService _service;


        /// <summary>
        /// Hook the events for encounters to pipe them out to listeners.
        /// </summary>
        /// <param name="pogoSession">Necro pogo session.</param>
        /// <param name="service">Nurx service instance.</param>
        public void RegisterSender(Session pogoSession, NurxService service)
        {
            _pogoSession = pogoSession;
            _service = service;

            // Register hooks for encoutner events.
            _service.RegisterHook(typeof(EncounterLureEvent), LureEncounter);
            _service.RegisterHook(typeof(EncounterIncenseEvent), IncenseEncounter);
            _service.RegisterHook(typeof(EncounterNearbyEvent), NearbyEncounter);
        }


        /// <summary>
        /// Callback for lure encounter.
        /// </summary>
        /// <param name="evt">Event instance</param>
        private void LureEncounter(IEvent evt)
        {
            EncounterLureEvent e = (EncounterLureEvent) evt;
            _service.Broadcast("encounter_lure", e.Encounter.PokemonData);
        }


        /// <summary>
        /// Callback for incense encounter.
        /// </summary>
        /// <param name="evt">Event instance</param>
        private void IncenseEncounter(IEvent evt)
        {
            EncounterIncenseEvent e = (EncounterIncenseEvent)evt;
            _service.Broadcast("encounter_incense", e.Encounter.PokemonData);
        }

        /// <summary>
        /// Callback for nearby encounter.
        /// </summary>
        /// <param name="evt">Event instance</param>
        private void NearbyEncounter(IEvent evt)
        {
            EncounterNearbyEvent e = (EncounterNearbyEvent)evt;
            _service.Broadcast("encounter_nearby", e.Encounter.WildPokemon);
        }
    }
}
