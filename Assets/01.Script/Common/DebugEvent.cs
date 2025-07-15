using System.Collections.Generic;
using System;
using UnityEngine;

public class DebugEvent<T>
{
    private readonly List<Action<T>> listeners = new List<Action<T>>();

    public void AddListener(Action<T> listener)
    {
        listeners.Add(listener);
        UnityEngine.Debug.Log($"[DebugEvent] Listener added: {listener.Method.Name}");
    }

    public void RemoveListener(Action<T> listener)
    {
        listeners.Remove(listener);
        UnityEngine.Debug.Log($"[DebugEvent] Listener removed: {listener.Method.Name}");
    }

    public void Invoke(T param)
    {
        foreach (var listener in listeners)
        {
            listener.Invoke(param);
        }
    }
}
public class DebugEvent
{
    private readonly List<Action> listeners = new List<Action>();

    public void AddListener(Action listener)
    {
        if (!listeners.Contains(listener))
        {
            listeners.Add(listener);
            Debug.Log($"[DebugEvent] Listener added: {listener.Method.DeclaringType}.{listener.Method.Name}");
        }
    }

    public void RemoveListener(Action listener)
    {
        if (listeners.Contains(listener))
        {
            listeners.Remove(listener);
            Debug.Log($"[DebugEvent] Listener removed: {listener.Method.DeclaringType}.{listener.Method.Name}");
        }
    }

    public void Invoke()
    {
        Debug.Log($"[DebugEvent] Invoked (no parameters)");
        foreach (var listener in listeners)
        {
            listener.Invoke();
        }
    }

    public override string ToString()
    {
        string result = "[DebugEvent] Current listeners:\n";
        foreach (var listener in listeners)
        {
            result += $"- {listener.Method.DeclaringType}.{listener.Method.Name}\n";
        }
        return result;
    }
}