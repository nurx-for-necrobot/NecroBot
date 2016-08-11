using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoGo.NecroBot.CLI.Nurx
{
    /// <summary>
    /// Websockets client command object.
    /// </summary>
    class NurxCommand
    {
        public string Command { get; set; }
        public dynamic Data { get; set; }
    }
}
