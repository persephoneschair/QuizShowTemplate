using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TTSManager
{
    public static void Speak(string message, float delay = 0f)
    {
        WindowsVoice.speak(message, delay);
    }
}
