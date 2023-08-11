using System;
using UnityEngine.Events;

namespace Control
{
    [Serializable]
    public class RoomCodeEvent : UnityEvent<string>
    {
        public RoomCodeEvent()
        {
        }
    }

    [Serializable]
    public class PlayerEvent : UnityEvent<Player>
    {
        public PlayerEvent()
        {
        }
    }

    [Serializable]
    public class DataMessageEvent : UnityEvent<DataMessage>
    {
        public DataMessageEvent()
        {
        }
    }

    [Serializable]
    public class EventMessageEvent : UnityEvent<EventMessage>
    {
        public EventMessageEvent()
        {
        }
    }
}
