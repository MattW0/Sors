using System.IO;
using UnityEngine;

public static class ScriptableCardFactory
{
    private static readonly string RuntimeCardPath =
        Path.Combine(Application.persistentDataPath, "Cards/Runtime");

    public static ScriptableCard Load(string resourceName, CardType type)
    {
        // 1️⃣ Try built-in Resources
        var pathPrefix = type switch {
            CardType.Money => "Cards/MoneyCards/",
            CardType.Creature => "Cards/CreatureCards/",
            CardType.Technology => "Cards/TechnologyCards/",
            _ => ""
        };
        var card = Resources.Load<ScriptableCard>(pathPrefix + resourceName);
        if (card != null) return card;

        // 2️⃣ Try runtime JSON
        var filePath = Path.Combine(Application.persistentDataPath, pathPrefix, resourceName + ".json");
        if (!File.Exists(filePath))
            return null;

        var json = File.ReadAllText(filePath);
        var data = JsonUtility.FromJson<CardInfo>(json);

        return CreateRuntimeCard(data);
    }

    private static ScriptableCard CreateRuntimeCard(CardInfo data)
    {
        var card = ScriptableObject.CreateInstance<ScriptableCard>();

        card.isStartCard = data.isStartCard;
        card.hash = data.hash;
        card.resourceName = data.resourceName;

        card.type = data.type;
        card.title = data.title;
        card.cost = data.cost;
        card.health = data.health;
        card.attack = data.attack;
        card.points = data.points;
        card.moneyValue = data.moneyValue;

        card.abilities = data.abilities;
        card.traits = data.traits;

        card.flavourText = data.flavourText;
        card.description = data.description;

        return card;
    }
}
