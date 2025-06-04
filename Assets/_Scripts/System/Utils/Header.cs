using TMPro;
using UnityEngine;

public class Header : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    internal void Init(string v) => text.text = v;
}
