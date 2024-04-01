using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class DamageSystem : MonoBehaviour
{
    [SerializeField] private CombatManager _combatManager;
    [SerializeField] private PlayerInterfaceManager _playerInterfaceManager;
    public List<CombatClash> _clashes = new();
    public GameObject screenText;

    public void ExecuteClashes(List<CombatClash> clashes)
    {
        print("Number clashes : " + clashes.Count);
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
            // Debug.Log(Time.time);
            
            var log = "";
            if (clash.IsClash) log = $"-- Combat Clash --";
            else log = $"-- Direct Damage --";
            _playerInterfaceManager.RpcLog(log, LogType.Standard);

            _playerInterfaceManager.RpcLog(clash.ToString(), LogType.CombatClash);
            
            // tasks[i] = clash.ExecuteCR();
            yield return StartCoroutine(clash.Execute());
            while(!clash.IsDone) yield return new WaitForSeconds(0.01f);

            i++;
        }
        
        _combatManager.UpdateCombatState(CombatState.CleanUp);
    }



}
 