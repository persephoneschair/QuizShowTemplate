using Newtonsoft.Json.Linq;

namespace Control
{
    public class Player
    {
        internal Player(string userID, JObject data)
        {
            UserID = userID;
            Data = data;
        }

        public DataMessageEvent OnData = new DataMessageEvent();
        public EventMessageEvent OnMessage = new EventMessageEvent();

        public string UserID
        {
            get;
            private set;
        }

        public JObject Data
        {
            get;
            internal set;
        }

        public string Name
        {
            get
            {
                if (TryGetDataPart("name", out JToken nameToken))
                {
                    return (string)nameToken;
                }

                return null;
            }
        }
        public JToken GetDataPart(string key)
        {
            return Data[key];
        }

        public bool TryGetDataPart(string key, out JToken value)
        {
            return Data.TryGetValue(key, out value);
        }

        public override int GetHashCode()
        {
            return UserID.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is Player otherPlayer)
            {
                return UserID == otherPlayer.UserID;
            }

            return false;
        }
    }
}
