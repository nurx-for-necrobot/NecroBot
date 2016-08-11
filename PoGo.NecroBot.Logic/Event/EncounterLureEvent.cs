using POGOProtos.Networking.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoGo.NecroBot.Logic.Event
{
    public class EncounterLureEvent : IEvent
    {
        public DiskEncounterResponse Encounter { get; set; }
    }
}
