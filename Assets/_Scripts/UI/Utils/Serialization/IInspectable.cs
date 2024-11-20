using System;
using UnityEngine.EventSystems;

public interface IInspectable<T> : IPointerClickHandler
{
    public static event Action<T> OnInspect;
}
