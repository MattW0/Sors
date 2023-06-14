using System.Collections.Generic;
using UnityEngine;

namespace CardDecoder {
    [System.Serializable]
    public class Deck {
        public Card[] cards;
    }

    [System.Serializable]
    public class Card{

        public string hash;
        public CardType type;
        public int cost;
        public string title;
        public int attack;
        public int health;
        public int points;
        public List<string> keyword_abilities;
        public List<string> relations;
    }

    #if UNITY_EDITOR
        public class CardDecoder : MonoBehaviour
        {
            public static CardDecoder instance;
            public bool reloadJson = false;
            
            private string path_json = "cards";

            private void Awake()
            {
                if (reloadJson)
                {
                    CreateScriptableObjectCards(path_json);
                    reloadJson = false;
                }
            }
            private void CreateScriptableObjectCards(string path)
            {
                TextAsset textAsset = Resources.Load<TextAsset>(path);
                Debug.Log(textAsset.text);

                Deck deck = JsonUtility.FromJson<Deck>(textAsset.text);
                Debug.Log("Creating " + deck.cards.Length + " cards");

                foreach(var card in deck.cards) {
                    CreateScriptableObjectCard(card);
                }
            }
            
            private static void CreateScriptableObjectCard(Card card)
            {
                ScriptableCard scriptableCard = ScriptableObject.CreateInstance<ScriptableCard>();

                scriptableCard.hash = card.hash;
                scriptableCard.type = CardType.Creature;

                scriptableCard.title = card.title;
                scriptableCard.cost = card.cost;
                scriptableCard.attack = card.attack;
                scriptableCard.health = card.health;
                scriptableCard.points = card.points;

                var keywords = new List<Keywords>();
                foreach(var kw in card.keyword_abilities){
                    var keyword = (Keywords)System.Enum.Parse(typeof(Keywords), kw);
                    keywords.Add(keyword);
                }
                scriptableCard.keywordAbilities = keywords;

                var relationsTexts = new List<string>();
                foreach (var relation in card.relations)
                {
                    var text = createRelationText(card.title, relation);
                    relationsTexts.Add(text);
                }
                scriptableCard.relationsTexts = relationsTexts;

                UnityEditor.AssetDatabase.CreateAsset(scriptableCard, $"Assets/Resources/CreatureCards/{card.title}.asset");
                UnityEditor.AssetDatabase.SaveAssets();
            }

            private static string createRelationText(string name, string relation){
                
                string txt = "";

                string[] triggerEffect = relation.Split('-');
                string trigger = triggerEffect[0];
                string effect = triggerEffect[1];

                // Trigger
                string[] timeAndOccurence = trigger.Split('_', 2);
                string time = timeAndOccurence[0];
                string occurence = timeAndOccurence[1];
                if (time == "Beginning"){
                    string phase = occurence.Split('_')[^1];
                    if (phase == "initiative") txt += "When you gain the initiative, ";
                    else txt += $"At the beginning of {phase}, ";
                }
                else if (time == "When" || time == "Whenever"){
                    occurence = occurence.Replace("_", " ");
                    txt += $"{time} {name} {occurence}, ";
                }

                // Special Ability
                effect = effect.Replace("_", " ");
                txt += effect;

                return txt;
            }
        }
    #endif
}


