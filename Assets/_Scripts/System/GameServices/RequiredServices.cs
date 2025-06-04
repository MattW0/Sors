using System;

public static class RequiredServices
{
    public static readonly Type[] CoreServices = new Type[]
    {
        typeof(INetworkObjectSpawner),
        typeof(CardMover),
        // TODO: Add furhter services here (instead of singletons)
    };
}
