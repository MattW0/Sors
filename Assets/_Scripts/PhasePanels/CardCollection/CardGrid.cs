using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardGrid : MonoBehaviour
{
    public bool updateGrid;
    [SerializeField] private float _cardStartX = 450;
    [SerializeField] private float _cardIncrementX = 70;
    [SerializeField] private float _cardEndX = -450;
    [SerializeField] private bool _flipDirection = false;
    private float _cardY = 20f;
    private float _incrementDirection = 1f; // -1 = left, 1 = right

    private void Awake(){
        if (_flipDirection){
            _incrementDirection = -1f;
        }
    }

    private void Update(){
        if (!updateGrid) return;
        
        UpdateGrid();
        updateGrid = false;
    }

    // // update when children change
    // private void OnTransformChildrenChanged()
    // {
    //     UpdateGrid();
    // }

    public void AddCard(Transform t){
        t.SetParent(transform, false);
        updateGrid = true;
    }

    private void UpdateGrid(){
        // var children = new List<Transform>();
        // foreach (Transform child in transform) children.Add(child);
        
        // // reverse because last element is rendered on top
        // if (!_flipDirection) children.Reverse();

        int i = 0;
        foreach (Transform child in transform) {
            // var x = Mathf.Lerp(_cardStartX, _cardEndX, i / (float)(children.Count - 1));
            var x = _cardStartX + _cardIncrementX * _incrementDirection * i;
            child.localPosition = new Vector3(x, _cardY, 0);
            child.localEulerAngles = new Vector3(0, 0, -90 * i);
            i++;
        }
    }

    public void ClearGrid(){
        foreach (Transform child in transform) Destroy(child.gameObject);
    }
}