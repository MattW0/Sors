using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardGrid : MonoBehaviour
{
    public bool updateGrid;
    [SerializeField] private CardGridType _cardGridType = CardGridType.All;
    [SerializeField] private float _cardStartX = 450;
    [SerializeField] private float _cardIncrementX = 70;
    [SerializeField] private float _cardEndX = -450;
    private float _cardY = 20f;
    private float _incrementDirection = 0f; // -1 = left, 1 = right

    private void Awake(){
        if (_cardGridType == CardGridType.Chosen){
            _incrementDirection = 1f;
        } else if (_cardGridType == CardGridType.All){
            _incrementDirection = -1f;
        }
    }

    private void Update(){
        if (!updateGrid) return;
        
        UpdateGrid();
        updateGrid = false;
    }

    // update when children change
    private void OnTransformChildrenChanged()
    {
        UpdateGrid();
    }

    private void UpdateGrid(){
        var children = new List<Transform>();
        foreach (Transform child in transform) children.Add(child);
        
        // reverse because last element is rendered on top
        if (_cardGridType == CardGridType.All) children.Reverse();

        if (children.Count < 1) return;


        int i = 0;
        foreach (var child in children) {
            // var x = Mathf.Lerp(_cardStartX, _cardEndX, i / (float)(children.Count - 1));
            var x = _cardStartX + _cardIncrementX * _incrementDirection * i;
            child.localPosition = new Vector3(x, _cardY, 0);
            i++;
        }


        // // sort children by x position
        // children.Sort((a, b) => a.position.x.CompareTo(b.position.x));

        // // set positions
        // for (int i = 0; i < children.Count; i++)
        // {
        //     var child = children[i];
        //     // calculate x position
        //     var x = Mathf.Lerp(_cardStartX, _cardEndX, i / (float)(children.Count - 1));
        //     child.localPosition = new Vector3(x, 0, 0);
        // }
    }
}

public enum CardGridType{
    All,
    Money,
    Creature,
    Technology,
    Chosen,
}
