using Newtonsoft.Json;
using PoGo.NecroBot.CLI;
using PoGo.NecroBot.CLI.Nurx.SenderResponders;
using PoGo.NecroBot.CLI.Nurx.Senders;
using PoGo.NecroBot.Logic;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.Logging;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Utils;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PoGo.NecroBot.CLI.Nurx
{
    /// <summary>
    /// Main entry point for the Nurx websockets server.
    /// </summary>
    class NurxService
    {
        public delegate void HandleEvent(IEvent evt);

        // Private vars.
        private Session _pogoSession;
        private ConsoleLogger _logger;
        private GlobalSettings _settings;
        private WebSocketServer _websocket;
        private List<WebSocketSession> _authSessions;
        private Dictionary<string, INurxMessageResponder> _responders;
        private Dictionary<Type, List<HandleEvent>> _eventHooks;

    
        // Public properties.
        public ConsoleLogger Logger { get { return _logger; } }


        /// <summary>
        /// Create the Nurx Service and kick off the websockets server.
        /// </summary>
        /// <param name="startInfo">Nurx service start information object.</param>
        public NurxService(NurxInitializerInfo startInfo)
        {
            // Make sure we have all the params we need.
            if (startInfo.Session == null) throw new ArgumentException("NecroBot Session not given.");
            if (startInfo.Settings == null) throw new ArgumentException("NecroBot Settings not given.");
            if (startInfo.Statistics == null) throw new ArgumentException("NecroBot Statistics object not given.");
            if (startInfo.Logger == null) throw new ArgumentException("NecroBot Logger object not given.");

            _pogoSession = startInfo.Session;
            _logger = startInfo.Logger;
            _settings = startInfo.Settings;         
            _responders = new Dictionary<string, INurxMessageResponder>();
            _eventHooks = new Dictionary<Type, List<HandleEvent>>();
            
            // TODO: Use reflection to discover and register all senders and responders.
            INurxMessageSender Log = new LogSender();
            Log.RegisterSender(_pogoSession, this);

            LocationSenderResponder Location = new LocationSenderResponder();
            Location.RegisterSender(_pogoSession, this);
            Location.RegisterResponder(_pogoSession, this);

            PokeStopSenderResponder PokeStop = new PokeStopSenderResponder();
            PokeStop.RegisterSender(_pogoSession, this);
            PokeStop.RegisterResponder(_pogoSession, this);

            PokemonListSenderResponder PokemonList = new PokemonListSenderResponder();
            PokemonList.RegisterSender(_pogoSession, this);
            PokemonList.RegisterResponder(_pogoSession, this);

            ProfileSenderResponder Profile = new ProfileSenderResponder();
            Profile.RegisterSender(_pogoSession, this);
            Profile.RegisterResponder(_pogoSession, this);
			
			InventoryListSenderResponder InventoryList = new InventoryListSenderResponder();
            InventoryList.RegisterSender(_pogoSession, this);
            InventoryList.RegisterResponder(_pogoSession, this);

            FortUsedSender UsedPokestop = new FortUsedSender();
            UsedPokestop.RegisterSender(_pogoSession, this);

            RecycleSender Recycle = new RecycleSender();
            Recycle.RegisterSender(_pogoSession, this);

            EncounterSender Encounter = new EncounterSender();
            Encounter.RegisterSender(_pogoSession, this);

            // Setup the websocket and check for success.
            if (SetupWebSocket())
            {
                // Broadcast statistics.
                startInfo.Statistics.DirtyEvent += () =>
                {
                    var currentStats = startInfo.Statistics.GetCurrentInfo(_pogoSession.Inventory);
                    Broadcast("stats", currentStats);
                };
            }
        }


        /// <summary>
        /// Add a responder to the responders dictionary.
        /// </summary>
        /// <param name="command">Command identifier string.</param>
        /// <param name="responder">Responder instance to link to the command.</param>
        public void RegisterResponder(string command, INurxMessageResponder responder)
        {
            if (!_responders.ContainsKey(command))
                _responders.Add(command, responder);
        }


        /// <summary>
        /// Create the websocket server.
        /// </summary>
        private bool SetupWebSocket()
        {
            _authSessions = new List<WebSocketSession>();
            _websocket = new WebSocketServer();

            // Create websockets server setup.
            var config = new ServerConfig
            {
                Name = "NurxWebSocket",
                Mode = SocketMode.Tcp,
                Certificate = new CertificateConfig
                {
                    FilePath = @"cert.pfx",
                    Password = "necro"
                }
            };
            config.Listeners = new List<ListenerConfig>
            {
                new ListenerConfig()
                {
                    Ip = "Any", Port = _settings.WebSocketPort, Security = "tls",                   
                },
                new ListenerConfig()
                {
                    Ip = "Any", Port = _settings.WebSocketPort + 1, Security = "none"
                }
            };

            // Setup the appServer
            if (!_websocket.Setup(config))
            {
                Logger.Write("Failed to setup Nurx Websockets server.", PoGo.NecroBot.Logic.Logging.LogLevel.Error);                
                return false;
            }

            // Set hooks.
            _websocket.NewMessageReceived += new SessionHandler<WebSocketSession, string>(WebSocket_NewMessageReceived);
            _websocket.NewSessionConnected += new SessionHandler<WebSocketSession>(WebSocket_NewSessionConnected);
            _websocket.SessionClosed += new SessionHandler<WebSocketSession, CloseReason>(WebSocket_SessionClosed);


            // Try to start the appServer
            if (!_websocket.Start())
            {
                Logger.Write("Failed to start Nurx Websockets server.", PoGo.NecroBot.Logic.Logging.LogLevel.Error);                
                return false;
            }

            return true;
        }
        

        /// <summary>
        /// Handle websockets session close.
        /// </summary>
        /// <param name="session">Websockets session instance.</param>
        /// <param name="reason">Reason the websockets session closed.</param>
        private void WebSocket_SessionClosed(WebSocketSession session, CloseReason reason)
        {
            // Sometimes this doesn't work. ¯\_(ツ)_/¯
            try
            {
                if (_authSessions.Contains(session))
                    _authSessions.Remove(session);
            }
            catch { }
        }


        /// <summary>
        /// Handle websockets new session.
        /// </summary>
        /// <param name="session">Websockets session instance.</param>
        private void WebSocket_NewSessionConnected(WebSocketSession session)
        {
            // TODO: Remove this and wait for actual auth.
            _authSessions.Add(session);
        }


        /// <summary>
        /// Handle websockets command received.
        /// </summary>
        /// <param name="session">Websockets session instance.</param>
        /// <param name="message">Message received from websockets client.</param>
        private void WebSocket_NewMessageReceived(WebSocketSession session, string message)
        {
            // Make sure the session is authenticated.
            if (_authSessions.Contains(session))
            {
                // Don't try to interact with NecroBot if the profile hasn't
                // even loaded yet.
                if (_pogoSession.Profile == null)
                    return;

                try
                {
                    NurxCommand cmd = JsonConvert.DeserializeObject<NurxCommand>(message);
                    // Find the appropriate responder and pass it the message.
                    if (_responders.ContainsKey(cmd.Command))
                            _responders[cmd.Command].MessageReceived(cmd, session);

                }
                catch (Exception ex)
                {
                    Logger.Write("Error processing nurx websockets command: " + ex.Message, LogLevel.Debug);
                    Logger.Write(ex.StackTrace, LogLevel.Debug);
                }
            }
        }


        /// <summary>
        /// Broadcast a message to all sessions.
        /// </summary>
        /// <param name="messageType">Messate type identifier.</param>
        /// <param name="data">Data to serialize and send to websockets clients.</param>
        public void Broadcast(string messageType, object data)
        {
            var response = new
            {
                MessageType = messageType,
                Data = data
            };

            _websocket.Broadcast(_authSessions.ToArray(), JsonConvert.SerializeObject(response), (s, b) => { });
        }


        /// <summary>
        /// Send a message to a single session.
        /// </summary>
        /// <param name="wsSession">Websockets client session instance.</param>
        /// <param name="messageType">Messate type identifier.</param>
        /// <param name="data">Data to serialize and send to websockets clients.</param>
        public void Send(WebSocketSession wsSession, string messageType, Object data)
        {
            var response = new
            {
                MessageType = messageType,
                Data = data
            };

            wsSession.Send(JsonConvert.SerializeObject(response));
        }


        /// <summary>
        /// Get any event from NecroBot.
        /// </summary>
        /// <param name="evt">Event instance.</param>
        public void ReceiveEvent(IEvent evt)
        {
            Type eventType = evt.GetType();

            if (!_eventHooks.ContainsKey(eventType))
                return;

            foreach (HandleEvent handler in _eventHooks[eventType])
                handler(evt);
        }


        /// <summary>
        /// Register a new handler for an event type.
        /// </summary>
        /// <param name="eventType">Event type instance.</param>
        /// <param name="action">Callback delegate.</param>
        public void RegisterHook(Type eventType, HandleEvent action)
        {
            if (!_eventHooks.ContainsKey(eventType))
                _eventHooks.Add(eventType, new List<HandleEvent>());

            if(!_eventHooks[eventType].Contains(action))
                _eventHooks[eventType].Add(action);
        }
    }
}
