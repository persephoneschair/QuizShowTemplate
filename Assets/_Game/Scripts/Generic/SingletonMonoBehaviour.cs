using UnityEngine;

public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    #region Init

    public static T Get { get; private set; }
    public virtual void Awake()
    {
        if (Get != null && Get != this)
        {
            Destroy(this);
            DebugLog.Print($"Destroyed GameObject that attempted to duplicate a singleton: {this.gameObject.name}", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Red);
        }
        else
        {
            Get = gameObject.GetComponent<T>();
            if (Get == this)
                DebugLog.Print($"Successfully registered the getter for {typeof(T).Name}!");
            else
                DebugLog.Print($"Unable to register the getter for {typeof(T).Name}");
        }
    }

    #endregion
}