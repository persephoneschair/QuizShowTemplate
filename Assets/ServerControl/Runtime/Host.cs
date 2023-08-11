using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Control
{
    public class Host : MonoBehaviour
    {
        #region Enums
        public enum DebugLevel
        {
            Off,
            Minimal,
            Full
        }
        #endregion

        #region Events
        [Tooltip("Called when this host connects to a room.")]
        public RoomCodeEvent OnRoomConnected = new RoomCodeEvent();
        [Tooltip("Called when this host disconnects from a room.")]
        public RoomCodeEvent OnRoomDisconnected = new RoomCodeEvent();
        [Tooltip("Called when this host attempts to reconnect to a room.")]
        public RoomCodeEvent OnRoomReconnecting = new RoomCodeEvent();
        [Tooltip("Called when this host fails an attempt to reconnect to a room.")]
        public RoomCodeEvent OnRoomReconnectFailed = new RoomCodeEvent();
        [Tooltip("Called when a member joins the room.")]
        public PlayerEvent OnPlayerJoined = new PlayerEvent();
        [Tooltip("Called when a member leaves the room.")]
        public PlayerEvent OnPlayerLeft = new PlayerEvent();
        [Tooltip("Called when a member sends an event in the room.")]
        public EventMessageEvent OnEvent = new EventMessageEvent();
        [Tooltip("Called when a ping/pong event occurs for this host.")]
        public UnityEvent OnPingPong = new UnityEvent();
        #endregion

        #region Public Fields
        [Tooltip("URL of server to connect to. Unless you know what you are doing, leave this as is.")]
        public string URL = "ws://localhost:3000/";
        [Tooltip("A specific host name for this host instance.")]
        public string HostName = null;
        [Tooltip("If true, then it will reload the previous host setup.")]
        public bool ReloadHost = false;
        [Tooltip("The level of logging that will be shown.")]
        public DebugLevel Debugging = DebugLevel.Minimal;
        [Tooltip("The number of seconds between each re-sync of player data from the server.")]
        public float ResyncInterval = 5.0f;
        #endregion

        #region Public Properties
        public bool Connected => _socket?.Connected ?? false;
        public bool Disconnected => _socket?.Disconnected ?? true;

        public string RoomCode
        {
            get;
            private set;
        }

        public string UserID
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        } = "HOST";

        public TimeSpan Latency
        {
            get;
            private set;
        } = TimeSpan.Zero;

        public Player[] AllPlayers => Players.Values.ToArray();
        public bool HasPlayers => Players.Any();
        #endregion

        #region Private Properties
        private string TemporaryFilePath => Path.Combine(Application.temporaryCachePath, $"LastControlHostRoom-{(string.IsNullOrEmpty(HostName) ? name : HostName)}.json");
        private string PlayerPrefsPrefix => $"LastControlHostRoom-{(string.IsNullOrEmpty(HostName) ? name : HostName)}-";
        #endregion

        #region Private Fields
        private readonly ConcurrentDictionary<string, Player> Players = new ConcurrentDictionary<string, Player>();
        private readonly ConcurrentQueue<Action> ThreadSafeActions = new ConcurrentQueue<Action>();
        private ISocketIO _socket = null;
        private float _lastPingTime = 0.0f;
        private Coroutine _resync = null;
        #endregion

        #region Unity Events
        protected virtual void Awake()
        {
            UserID = Guid.NewGuid().ToString();
        }

        protected virtual void Start()
        {
            Connect();
        }

        protected virtual void Update()
        {
            while (!ThreadSafeActions.IsEmpty)
            {
                if (ThreadSafeActions.TryDequeue(out Action action))
                {
                    action?.Invoke();
                }
            }
        }

        protected virtual void OnDestroy()
        {
            Disconnect();
        }
        #endregion

        #region Public Methods
        public void Connect(bool forceNewRoom = false)
        {
            if (forceNewRoom)
            {
                RoomCode = null;
            }
            else if (ReloadHost)
            {
                LoadRoomData();
            }

            SaveRoomData();
            _ = RestartSocket();
        }

        public void Disconnect()
        {
            _ = EndSocket();
        }

        public Player GetMemberByID(string userID)
        {
            return Players.Values.FirstOrDefault(x => x.UserID.Equals(userID));
        }

        public void UpdatePlayerData(Player player, JObject data)
        {
            UpdatePlayerData(player, null, data);
        }

        public void UpdatePlayerData(Player player, string key, JToken data)
        {
            if (!string.IsNullOrEmpty(key))
            {
                player.Data[key] = data;
            }
            else
            {
                player.Data = (JObject)data;
            }
            SendPlayerUpdate(new JArray(new[] { player.UserID }), key, data);
        }

        public void UpdatePlayerData(IEnumerable<Player> players, JObject data)
        {
            UpdatePlayerData(players, null, data);
        }

        public void UpdatePlayerData(IEnumerable<Player> players, string key, JToken data)
        {
            foreach (Player player in players)
            {
                if (!string.IsNullOrEmpty(key))
                {
                    player.Data[key] = data;
                }
                else
                {
                    player.Data = (JObject)data;
                }
            }

            SendPlayerUpdate(new JArray(players.Select(x => x.UserID).ToArray()), key, data);
        }

        public void UpdateAllPlayerData(JObject data)
        {
            UpdateAllPlayerData(null, data);
        }

        public void UpdateAllPlayerData(string key, JToken data)
        {
            foreach (Player player in AllPlayers)
            {
                if (!string.IsNullOrEmpty(key))
                {
                    player.Data[key] = data;
                }
                else
                {
                    player.Data = (JObject)data;
                }
            }

            SendPlayerUpdate(new JArray(AllPlayers.Select(x => x.UserID).ToArray()), key, data);
        }
        #endregion

        #region Private Methods
        private void DoUnityAction(Action action)
        {
            ThreadSafeActions.Enqueue(action);
        }

        private void VerboseLog(string message)
        {
            if (Debugging == DebugLevel.Full)
            {
                DoUnityAction(() => Debug.Log(message));
            }
        }

        private void Log(string message)
        {
            if (Debugging >= DebugLevel.Minimal)
            {
                DoUnityAction(() => Debug.Log(message));
            }
        }

        private void LogWarn(string message)
        {
            if (Debugging >= DebugLevel.Minimal)
            {
                DoUnityAction(() => Debug.LogWarning(message));
            }
        }

        private void LogError(string message)
        {
            DoUnityAction(() => Debug.LogError(message));
        }

        private void LogException(Exception exception)
        {
            DoUnityAction(() => Debug.LogException(exception));
        }

        private void LoadRoomData()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            try
            {
                if (!File.Exists(TemporaryFilePath))
                {
                    return;
                }

                string data = File.ReadAllText(TemporaryFilePath);
                JObject jData = JObject.Parse(data);
                RoomCode = (string)jData["roomCode"];
                UserID = (string)jData["userID"];
                Name = (string)jData["name"];
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
#else
            string prefsPrefix = PlayerPrefsPrefix;
            RoomCode = PlayerPrefs.GetString($"{prefsPrefix}RoomCode", RoomCode);
            UserID = PlayerPrefs.GetString($"{prefsPrefix}UserID", UserID);
            Name = PlayerPrefs.GetString($"{prefsPrefix}Name", Name);
#endif
        }

        private void SaveRoomData()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            JObject jData = JObject.FromObject(new
            {
                roomCode = RoomCode,
                userID = UserID,
                name = Name
            });

            File.WriteAllText(TemporaryFilePath, jData.ToString());
#else
            string prefsPrefix = PlayerPrefsPrefix;
            PlayerPrefs.SetString($"{prefsPrefix}RoomCode", RoomCode);
            PlayerPrefs.SetString($"{prefsPrefix}UserID", UserID);
            PlayerPrefs.SetString($"{prefsPrefix}Name", Name);
#endif
        }

        private async Task StartSocket()
        {
            Log($"Attempting socket connection to {URL} for <b>{RoomCode}</b> with host <i>{Name}|{UserID}</i>...");

            Dictionary<string, string> queryParameters = new Dictionary<string, string>()
            {
                ["userID"] = UserID,
                ["name"] = Name,
                ["hosting"] = "true"
            };

            if (!string.IsNullOrEmpty(RoomCode))
            {
                queryParameters["roomCode"] = RoomCode;
            }

#if UNITY_EDITOR || UNITY_STANDALONE
            _socket = new StandaloneSocketIO(URL, 4, queryParameters);
#elif UNITY_WEBGL
            _socket = new WebGLSocketIO(URL, 4, queryParameters);
#endif
            _socket.OnConnected += OnConnected;
            _socket.OnError += OnError;
            _socket.OnDisconnected += OnDisconnected;
            _socket.OnReconnectAttempt += OnReconnectAttempt;
            _socket.OnReconnected += OnReconnected;
            _socket.OnReconnectFailed += OnReconnectFailed;
            _socket.OnPing += OnPing;
            _socket.OnPong += OnPong;
            _socket.On("myPong", OnMyPong);
            _socket.On("join", OnRoomJoin);
            _socket.On("hostData", OnHostDataUpdate);
            _socket.On("event", OnPlayerEvent);

            await _socket.Connect();
        }

        private void OnConnected()
        {
            Log($"Connected to {URL} with host <i>{UserID}</i>.");
        }

        private void OnError(string error)
        {
            LogError($"Failed connection to {URL}: {error}");
        }

        private void OnDisconnected(string error)
        {
            LogWarn($"Disconnected from {URL}: {error}");
            DoUnityAction(() =>
            {
                OnRoomDisconnected.Invoke(RoomCode);
            });
        }

        private void OnReconnectAttempt(int attemptCount)
        {
            Log($"Reconnecting to {URL} <b>{RoomCode}</b> (attempt {attemptCount})...");
            DoUnityAction(() =>
            {
                OnRoomReconnecting.Invoke(RoomCode);
            });
        }

        private void OnReconnected(int attemptCount)
        {
            Log($"Reconnected to {URL} <b>{RoomCode}</b> with host <i>{UserID}</i> (attempt {attemptCount}).");

            DoUnityAction(() =>
            {
                OnRoomConnected.Invoke(RoomCode);
                SaveRoomData();

                foreach (Player player in Players.Values)
                {
                    if (player.Data != null)
                    {
                        UpdatePlayerData(player, null, player.Data);
                    }
                }
            });
        }

        private void OnReconnectFailed()
        {
            LogError($"Reconnect to {URL} <b>{RoomCode}</b> failed.");
            DoUnityAction(() =>
            {
                OnRoomReconnectFailed.Invoke(RoomCode);
            });
        }

        private void OnPing()
        {
            VerboseLog($"Ping...");
            _ = _socket.Emit("myPing", new JObject());
            DoUnityAction(() => _lastPingTime = Time.time);            
        }

        private void OnPong(TimeSpan e)
        {
            VerboseLog($"...Pong.");

            DoUnityAction(() =>
            {
                OnPingPong.Invoke();
            });
        }

        private void OnMyPong(JObject data)
        {
            DoUnityAction(() =>
            {
                Latency = TimeSpan.FromSeconds(Time.time - _lastPingTime);
            });

            JObject obj = new JObject(
                new JProperty("duration", Latency.TotalSeconds)
            );

            _ = _socket.Emit("latency", obj);
        }

        private async Task EndSocket()
        {
            Log($"Closing socket connection to {URL}...");

            if (_socket != null)
            {
                await _socket.Disconnect();
                _socket = null;
            }

            DoUnityAction(() =>
            {
                if (_resync != null)
                {
                    StopCoroutine(_resync);
                    _resync = null;
                }
            });

            Log("Closed socket connection.");
        }

        private async Task RestartSocket()
        {
            if (_socket != null)
            {
                await EndSocket();
            }

            await StartSocket();
        }

        private void OnRoomJoin(JObject data)
        {
            RoomCode = (string)data["roomCode"];
            Log($"Connected to room <b>{RoomCode}</b> with host <i>{UserID}</i>.");

            DoUnityAction(() =>
            {
                OnRoomConnected.Invoke(RoomCode);
                SaveRoomData();

                if (_resync != null)
                {
                    StopCoroutine(_resync);
                }
                _resync = StartCoroutine(ResyncLoop());
            });
        }

        private void SendPlayerUpdate(JArray players, string key, JToken data)
        {
            if (_socket == null)
            {
                LogError("Cannot send a member update - socket has not been initialised!");
                return;
            }

            JObject obj = new JObject(
                new JProperty("players", players),
                new JProperty("data", data)
            );

            if (!string.IsNullOrEmpty(key))
            {
                obj["key"] = key;
            }

            _ = _socket.Emit("data", obj);

            VerboseLog($"Emitting...\n{obj.ToString(Formatting.None)}");
        }

        private void OnHostDataUpdate(JObject data)
        {
            VerboseLog($"Receiving host data...\n{data.ToString(Formatting.None)}");

            HashSet<Player> oldPlayers = new HashSet<Player>(Players.Values);

            JObject playersObject = (JObject)data["players"];
            foreach (JProperty playerProperty in playersObject.Properties())
            {
                JObject playerData = (JObject)playerProperty.Value;
                string userID = playerProperty.Name;
                if (Players.TryGetValue(userID, out Player player))
                {
                    player.Data = playerData;
                    oldPlayers.Remove(player);
                }
                else
                {
                    Player newPlayer = new Player(userID, playerData);
                    Players[newPlayer.UserID] = newPlayer;

                    Log($"Player <b>{newPlayer.UserID}</b> connected to room <b>{RoomCode}</b>.");
                    DoUnityAction(() => OnPlayerJoined.Invoke(newPlayer));
                }
            }

            foreach (Player oldPlayer in oldPlayers)
            {
                Log($"<b>{oldPlayer.UserID}</b> has left room <b>{RoomCode}</b>.");
                if (Players.TryRemove(oldPlayer.UserID, out _))
                {
                    DoUnityAction(() => OnPlayerLeft.Invoke(oldPlayer));
                }
            }
        }

        private void OnPlayerEvent(JObject msgObject)
        {
            VerboseLog($"Receiving player event...\n{msgObject.ToString(Formatting.None)}");

            string playerID = (string)msgObject["player"];
            if (!Players.TryGetValue(playerID, out Player fromPlayer))
            {
                LogError($"Received a message from player <i>{playerID}</i> but there is no known associated Player object!");
                return;
            }

            JObject data = (JObject)msgObject["data"];
            EventMessage message = new EventMessage(fromPlayer, (string)data["eventName"], (JObject)data["data"]);

            Log($"<i>{fromPlayer.UserID}</i> {message}");
            DoUnityAction(() =>
            {
                OnEvent.Invoke(message);
                fromPlayer.OnMessage.Invoke(message);
            });
        }

        private IEnumerator ResyncLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(ResyncInterval);
                _ = _socket.Emit("resync", new JObject());
            }
        }
        #endregion
    }
}
