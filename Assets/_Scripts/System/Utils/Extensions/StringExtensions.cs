using UnityEngine;

public static class StringExtensions
{
    public static string AddColor(this string text, Color col) => $"<color={col.ColorHexFromUnityColor()}>{text}</color>";
    public static string AddColorFromHex(this string text, string hex) => $"<color={hex}>{text}</color>";
}
