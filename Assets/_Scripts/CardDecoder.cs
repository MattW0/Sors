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
        public string title;
        public bool isCreature;
        public int cost;
        public int attack;
        public int health;
        public string triggers;
        public string keyword_abilities;
        public string special_abilities;
    }

    #if UNITY_EDITOR
        public class CardDecoder : MonoBehaviour
        {
            public static CardDecoder instance;
            public bool reloadJson = false;
            
            private string path_json = "cards_from_100";

            private void Awake()
            {
                if (reloadJson)
                {
                    createDeck(path_json);
                    reloadJson = false;
                }
            }
            public void createDeck(string path)
            {
                TextAsset textAsset = Resources.Load<TextAsset>(path);
                Debug.Log(textAsset.text);

                Deck deck = JsonUtility.FromJson<Deck>(textAsset.text);
                Debug.Log("Creating " + deck.cards.Length + " cards");

                foreach(var card in deck.cards) {
                    CreateSOCard(card);
                }
            }
            
            public static void CreateSOCard(Card card)
                {
                    ScriptableCard cardInfo = ScriptableObject.CreateInstance<ScriptableCard>();

                    cardInfo.hash = card.hash;
                    cardInfo.title = card.title;
                    cardInfo.isCreature = true;
                    cardInfo.cost = card.cost;
                    cardInfo.attack = card.attack;
                    cardInfo.health = card.health;

                    UnityEditor.AssetDatabase.CreateAsset(cardInfo, $"Assets/Resources/CreatureCards/{card.title}.asset");
                    UnityEditor.AssetDatabase.SaveAssets();
                }
        }
    #endif
}


