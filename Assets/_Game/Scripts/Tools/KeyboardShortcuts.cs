using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class KeyboardShortcuts : MonoBehaviour
{
    [Serializable]
    public class KeyboardMapping
    {
        public KeyCode[] Keys = null;
        public bool OnDown = true;
        public UnityEvent Event = new UnityEvent();
    }

    public KeyboardMapping[] Mappings = null;

    private void Update()
    {
        foreach (KeyboardMapping mapping in Mappings)
        {
            if (mapping == null || mapping.Keys == null)
            {
                continue;
            }

            if (mapping.OnDown && mapping.Keys.All(x => Input.GetKeyDown(x)))
            {
                mapping.Event.Invoke();
            }
            else if (!mapping.OnDown && mapping.Keys.All(x => Input.GetKeyUp(x)))
            {
                mapping.Event.Invoke();
            }
        }
    }
}
