using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TestEntity : MonoBehaviour, IPointerClickHandler
{
    public static event Action<GameObject, Transform, int> OnEntityClicked;
    public GameObject arrowPrefab;
    public void OnPointerClick(PointerEventData eventData)
    {
        OnEntityClicked?.Invoke(arrowPrefab, transform, -1);
    }
}
