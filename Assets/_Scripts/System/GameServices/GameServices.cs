using System;
using System.Collections.Generic;
using UnityEngine;


public static class GameServices
{
    private static readonly Dictionary<Type, object> services = new();

    public static void Register<T>(T service) where T : class
    {
        var type = typeof(T);
        if (services.ContainsKey(type))
            Debug.LogWarning($"Service {type.Name} already registered. Overwriting.");

        services[type] = service;
    }

    public static T Get<T>() where T : class
    {
        var type = typeof(T);
        if (services.TryGetValue(type, out var service))
            return service as T;

        Debug.LogError($"Service {type.Name} not registered.");
        return null;
    }

    public static void Clear() => services.Clear();
}
