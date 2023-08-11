using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Control
{
    public class Client : MonoBehaviour
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
        [Tooltip("Called when this client connects to a room.")]
        public RoomCodeEvent OnRoomConnected = new RoomCodeEvent();
        [Tooltip("Called when this client disconnects from a room.")]
        public RoomCodeEvent OnRoomDisconnected = new RoomCodeEvent();
        [Tooltip("Called when this client attempts to reconnect to a room.")]
        public RoomCodeEvent OnRoomReconnecting = new RoomCodeEvent();
        [Tooltip("Called when this client fails an attempt to reconnect to a room.")]
        public RoomCodeEvent OnRoomReconnectFailed = new RoomCodeEvent();
        [Tooltip("Called when a host sends part data to this player.")]
        public DataMessageEvent OnPartData = new DataMessageEvent();
        [Tooltip("Called when a host sends all data to this player.")]
        public DataMessageEvent OnAllData = new DataMessageEvent();
        [Tooltip("Called when a host sends an event in the room.")]
        public EventMessageEvent OnEvent = new EventMessageEvent();
        [Tooltip("Called when a ping/pong event occurs for this client.")]
        public UnityEvent OnPingPong = new UnityEvent();
        #endregion

        #region Public Fields
        [Tooltip("URL of server to connect to. Unless you know what you are doing, leave this as is.")]
        public string URL = "ws://localhost:3000/";
        [Tooltip("A specific client name for this client instance.")]
        public string ClientName = null;
        [Tooltip("If true, then it will reload the previous client setup.")]
        public bool ReloadClient = false;
        [Tooltip("The level of logging that will be shown.")]
        public DebugLevel Debugging = DebugLevel.Minimal;
        #endregion

        #region Public Properties
        public bool Connected => _socket?.Connected ?? false;
        public bool Disconnected => _socket?.Disconnected ?? true;

        public Player Player
        {
            get;
            private set;
        }

        public string RoomCode
        {
            get;
            set;
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
        }

        public TimeSpan Latency
        {
            get;
            private set;
        } = TimeSpan.Zero;
        #endregion

        #region Private Properties
        private string TemporaryFilePath => Path.Combine(Application.temporaryCachePath, $"LastControlClientRoom-{(string.IsNullOrEmpty(ClientName) ? name : ClientName)}.json");
        private string PlayerPrefsPrefix => $"LastControlClientRoom-{(string.IsNullOrEmpty(ClientName) ? name : ClientName)}-";
        #endregion

        #region Private Fields
        private readonly ConcurrentQueue<Action> ThreadSafeActions = new ConcurrentQueue<Action>();
        private ISocketIO _socket = null;
        private float _lastPingTime = 0.0f;
        #endregion

        #region Unity Events
        protected virtual void Awake()
        {
            if (string.IsNullOrEmpty(UserID))
            {
                UserID = Guid.NewGuid().ToString();
            }
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
        public void Connect()
        {
            if (ReloadClient)
            {
                LoadRoomData();
            }

            SaveRoomData();
            _ = RestartSocket();
        }

        public void Connect(string name, string roomCode)
        {
            Name = name;
            RoomCode = roomCode;
            SaveRoomData();
            _ = RestartSocket();
        }

        public void Disconnect()
        {
            _ = EndSocket();
        }

        public void SendEvent(string eventName, JObject data)
        {
            if (_socket == null)
            {
                LogError("Cannot send a member update - socket has not been initialised!");
                return;
            }

            JObject obj = new JObject(
                new JProperty("eventName", eventName),
                new JProperty("data", data)
            );

            _ = _socket.Emit("event", obj);

            VerboseLog($"Emitting...\n{obj.ToString(Formatting.None)}");
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
            Log($"Attempting socket connection to {URL} for <b>{RoomCode}</b> with client <i>{UserID}</i>...");

            if (string.IsNullOrEmpty(UserID))
            {
                UserID = Guid.NewGuid().ToString();
            }

            Dictionary<string, string> queryParameters = new Dictionary<string, string>()
            {
                ["userID"] = UserID,
                ["name"] = Name,
                ["hosting"] = "false",
                ["roomCode"] = RoomCode
            };

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
            _socket.On("dataPart", OnDataPartUpdate);
            _socket.On("dataAll", OnDataAllUpdate);
            _socket.On("data", OnDataUpdate);
            _socket.On("event", OnHostEvent);

            Player = new Player(UserID, new JObject());

            await _socket.Connect();
        }

        private void OnConnected()
        {
            Log($"Connected to {URL} <b>{RoomCode}</b> with client <i>{UserID}</i>.");
            DoUnityAction(() =>
            {
                OnRoomConnected.Invoke(RoomCode);
            });
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
            Log($"Reconnected to {URL} <b>{RoomCode}</b> with client <i>{UserID}</i> (attempt {attemptCount}).");

            DoUnityAction(() =>
            {
                OnRoomConnected.Invoke(RoomCode);
                SaveRoomData();
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
            Log($"Connected to room <b>{RoomCode}</b> with client <i>{UserID}</i>.");

            DoUnityAction(() =>
            {
                OnRoomConnected.Invoke(RoomCode);
                SaveRoomData();
            });
        }

        private void OnDataPartUpdate(JObject data)
        {
            VerboseLog($"Receiving part data...\n{data.ToString(Formatting.None)}");

            DataMessage message = new DataMessage(null, (string)data["key"], data["data"]);

            DoUnityAction(() =>
            {
                OnPartData.Invoke(message);
            });
        }

        private void OnDataAllUpdate(JObject data)
        {
            VerboseLog($"Receiving all data...\n{data.ToString(Formatting.None)}");

            DataMessage message = new DataMessage(null, null, data);

            DoUnityAction(() =>
            {
                OnAllData.Invoke(message);
            });
        }

        private void OnDataUpdate(JObject data)
        {
            VerboseLog($"Receiving data...\n{data.ToString(Formatting.None)}");

            Player.Data = data;
        }

        private void OnHostEvent(JObject msgObject)
        {
            VerboseLog($"Receiving...\n{msgObject.ToString(Formatting.None)}");

            EventMessage message = new EventMessage(null, (string)msgObject["eventName"], (JObject)msgObject["data"]);

            Log($"<i>HOST</i> {message}");
            DoUnityAction(() =>
            {
                OnEvent.Invoke(message);
            });
        }
        #endregion
    }
}
