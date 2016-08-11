using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic;
using PoGo.NecroBot.Logic.Utils;

namespace PoGo.NecroBot.CLI.Nurx
{
    /// <summary>
    /// Service initalization info object.
    /// </summary>
    class NurxInitializerInfo
    {
        public Session Session { get; set; }
        public ConsoleLogger Logger { get; set; }
        public GlobalSettings Settings { get; set; }
        public Statistics Statistics { get; set; }
    }
}
