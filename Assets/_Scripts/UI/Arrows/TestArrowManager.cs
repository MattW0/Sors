using System.Collections.Generic;
using UnityEngine;

public class TestArrowManager : MonoBehaviour
{
    [SerializeField] private Transform parentTransform;
    // [SerializeField] private GameObject targetArrowPrefab;
    // [SerializeField] private GameObject attackerArrowPrefab;
    // [SerializeField] private GameObject blockerArrowPrefab;
    
    private Dictionary<int, ArrowController> _floatingArrows = new();
    [SerializeReference] private Dictionary<int, ArrowController> _combatArrows = new();
    
    private void Awake() {
        TestEntity.OnEntityClicked += SpawnFloatingArrow;
    }

    private void SpawnFloatingArrow(GameObject prefab, Transform origin, int id)
    {
        var arrow = Instantiate(prefab, parentTransform, true).GetComponent<ArrowController>();
        arrow.SetOrigin(origin.position);
        
        _combatArrows.Add(id, arrow);
        if(_floatingArrows.ContainsKey(id)) _floatingArrows[id] = arrow;
        else _floatingArrows.Add(id, arrow);
    }

    private ArrowController SpawnArrowFromOpponent(GameObject prefab, Transform origin, Transform target)
    {
        var arrow = Instantiate(prefab, parentTransform).GetComponent<ArrowController>();
        
        arrow.SetOrigin(origin.position);
        arrow.SetTarget(target.position);

        return arrow;
    }
    
}
