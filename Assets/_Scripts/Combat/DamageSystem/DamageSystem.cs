using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class DamageSystem : MonoBehaviour
{
    [SerializeField] private CombatManager _combatManager;
    public List<CombatClash> _clashes = new();
    public GameObject screenText;

    public void SetClashes(List<CombatClash> clashes)
    {
        print("Setting clashes : " + clashes.Count);
        _clashes = clashes;
        StartCoroutine(ExecuteClashesCR());
    }

    public IEnumerator ExecuteClashesCR() 
    {
        // Jänu was here : #@thisIsAComment ##xoxo
        screenText.SetActive(false); 

        IEnumerator[] tasks = new IEnumerator[_clashes.Count];
        var i = 0;
        foreach (var clash in _clashes)
        {
            Debug.Log(Time.time);

            print("Executing clash : " + clash.ToString());
            // tasks[i] = clash.ExecuteCR();
            yield return StartCoroutine(clash.Execute());
            while(!clash.IsDone) yield return new WaitForSeconds(0.01f);

            i++;
        }
        
        _combatManager.UpdateCombatState(CombatState.CleanUp);
    }



}
 