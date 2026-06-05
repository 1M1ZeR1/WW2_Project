using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventBus
{
    private readonly Dictionary<Type, Delegate> events = new();

    public void Subscribe<T1>(Action<T1> callback)
    {
        var key = typeof(Action<T1>);
        if (!events.ContainsKey(key)) events[key] = null;
        events[key] = (Action<T1>)events[key] + callback;
    }

    public void Publish<T1>(T1 arg1)
    {
        var key = typeof(Action<T1>);
        if (events.ContainsKey(key))
            (events[key] as Action<T1>)?.Invoke(arg1);
    }

    public void Subscribe<T1, T2>(Action<T1, T2> callback)
    {
        var key = typeof(Action<T1, T2>);
        if (!events.ContainsKey(key)) events[key] = null;
        events[key] = (Action<T1, T2>)events[key] + callback;
    }

    public void Publish<T1, T2>(T1 arg1, T2 arg2)
    {
        var key = typeof(Action<T1, T2>);
        if (events.ContainsKey(key))
        (events[key] as Action<T1, T2>)?.Invoke(arg1, arg2);
    }
    public void Subscribe<T1, T2, T3>(Action<T1, T2, T3> callback)
    {
        var key = typeof(Action<T1, T2, T3>);
        if (!events.ContainsKey(key)) events[key] = null;
        events[key] = (Action<T1, T2, T3>)events[key] + callback;
    }

    public void Publish<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3)
    {
        var key = typeof(Action<T1, T2, T3>);
        if (events.ContainsKey(key))
            (events[key] as Action<T1, T2, T3>)?.Invoke(arg1, arg2, arg3);
    }
    public void Subscribe<T1, T2, T3, T4>(Action<T1, T2, T3, T4> callback)
    {
        var key = typeof(Action<T1, T2, T3, T4>);
        if (!events.ContainsKey(key)) events[key] = null;
        events[key] = (Action<T1, T2, T3, T4>)events[key] + callback;
    }

    public void Publish<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        var key = typeof(Action<T1, T2, T3, T4>);
        if (events.ContainsKey(key))
            (events[key] as Action<T1, T2, T3, T4>)?.Invoke(arg1, arg2, arg3, arg4);
    }
}
