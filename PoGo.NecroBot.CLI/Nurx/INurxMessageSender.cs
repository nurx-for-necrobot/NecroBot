using PoGo.NecroBot.Logic.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoGo.NecroBot.CLI.Nurx
{
    /// <summary>
    /// Interface for providing command-less sends to websockets clients.
    /// </summary>
    interface INurxMessageSender
    {
        /// <summary>
        /// Register the sender with Nurx.
        /// </summary>
        /// <param name="pogoSession">Session state instnace.</param>
        /// <param name="service">Nurx service instance.</param>
        void RegisterSender(Session pogoSession, NurxService service);
    }
}
