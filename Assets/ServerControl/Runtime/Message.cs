using Newtonsoft.Json.Linq;

namespace Control
{
    public sealed class DataMessage
    {
        internal DataMessage(Player player, string key, JToken data)
        {
            Player = player;
            Key = key;
            Data = data;
        }

        public readonly Player Player;
        public readonly string Key;
        public readonly JToken Data;

        public override string ToString()
        {
            return $"[{Key}]: {Data}";
        }
    }

    public sealed class EventMessage
    {
        internal EventMessage(Player player, string eventName, JObject data)
        {
            Player = player;
            EventName = eventName;
            Data = data;
        }

        public readonly Player Player;
        public readonly string EventName;
        public readonly JObject Data;        

        public override string ToString()
        {
            return $"[{EventName}]: {Data}";
        }
    }
}
