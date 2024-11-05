using UnityEngine;

public static class StringExtensions
{
    public static string AddColor(this string text, Color col) => $"<color={col.ColorHexFromUnityColor()}>{text}</color>";
    public static string AddColorByType(this string text, LogType type)
    {
        var color = type switch{
            LogType.EffectTrigger => SorsColors.effectTrigger,
            LogType.TurnChange => SorsColors.neutralLight,
            LogType.Phase => SorsColors.neutralLight,
            LogType.Buy => SorsColors.buy,
            LogType.Play => SorsColors.play,
            LogType.Combat => SorsColors.warn,
            LogType.CombatAttacker => SorsColors.warn,
            LogType.CombatBlocker => SorsColors.warn,
            LogType.CombatClash => SorsColors.combatClash,
            LogType.Standard => SorsColors.text,
            _ => SorsColors.text
        };

        return $"<color={color.ColorHexFromUnityColor()}>{text}</color>";
    }
}
