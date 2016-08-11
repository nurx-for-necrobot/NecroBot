using PoGo.NecroBot.Logic.State;
using SuperSocket.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoGo.NecroBot.CLI.Nurx
{
    /// <summary>
    /// Interface for handline command responding.
    /// </summary>
    interface INurxMessageResponder
    {
        /// <summary>
        /// Register the reponder with the Nurx service.
        /// </summary>
        /// <param name="pogoSession">Session state instance.</param>
        /// <param name="service">Nurx service instance.</param>
        void RegisterResponder(Session pogoSession, NurxService service);

        /// <summary>
        /// Handle message received from websockets clients
        /// </summary>
        /// <param name="command">Command instance.</param>
        /// <param name="wsSession">Websockets session isntance.</param>
        void MessageReceived(NurxCommand command, WebSocketSession wsSession);

        /// <summary>
        /// Command name.
        /// </summary>
        string Command { get; }
    }
}
