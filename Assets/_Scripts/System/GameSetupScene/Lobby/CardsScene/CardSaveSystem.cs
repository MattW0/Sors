using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class CardSaveSystem
{
    private static string PathFor(CardType type, string resourceName) {
        var pathPrefix = type switch {
            CardType.Money => "Cards/MoneyCards",
            CardType.Creature => "Cards/CreatureCards",
            CardType.Technology => "Cards/TechnologyCards",
            _ => "Cards/Unknown"
        };
        var directoryPath = Path.Combine(
            Application.persistentDataPath,
            pathPrefix
        );

        // Ensure directory exists
        Directory.CreateDirectory(directoryPath);
        var filePath = Path.Combine(
            directoryPath,
            resourceName + ".json"
        );

        return filePath;
    }
        

    public static void Save(CardInfo data)
    {
        var json = JsonUtility.ToJson(data, true);
        var path = PathFor(data.type, data.resourceName);

        File.WriteAllText(path, json);

#if UNITY_EDITOR
        Debug.Log($"[CardSaveSystem] Saved runtime card to:\n{path}");
#endif
    }
}
