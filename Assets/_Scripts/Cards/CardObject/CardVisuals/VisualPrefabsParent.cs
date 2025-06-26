using UnityEngine;

public class VisualPrefabsParent : MonoBehaviour
{

    public static VisualPrefabsParent instance;

    private void Awake()
    {
        instance = this;
    }
}