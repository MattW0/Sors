using UnityEngine;
using System.Collections.Generic;
using Mirror;
using Cysharp.Threading.Tasks;
using UnityUtils;

[RequireComponent(typeof(CanvasGroup))]
public class AbilityQueueUI : NetworkBehaviour, IModalWindow
{
    [SerializeField] private GameObject _abilityItemPrefab;
    [SerializeField] private CardGrid _grid;
    [SerializeField] private Transform _spawnParentTransform;
    [SerializeField] private Queue<AbilityItemUI> _queue = new();
    private CanvasGroup _canvasGroup;
    private bool _isOpen = false;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        WindowOut();
    }

    [ClientRpc]
    public void RpcAddAbility(CardInfo cardInfo, Ability ability)
    {
        if (! _isOpen) WindowIn();

        InstantiateAbility(cardInfo, ability);
    }

    [ClientRpc]
    internal void RpcStartNextAbility()
    {
        if (_queue.Count == 0) return;

        var abilityUI = _queue.Peek();
        abilityUI.SetActive();
    }

    [ClientRpc]
    internal void RpcRemoveAbility()
    {
        if (_queue.Count == 0) return;

        var abilityUI = _queue.Dequeue();
        abilityUI.SetInactive();
    }

    [ClientRpc] internal void RpcWindowOut() => WindowOut();

    private void InstantiateAbility(CardInfo cardInfo, Ability ability)
    {
        // print("Ability Queue: Instantiate ability");
        var abilityItemUI = Instantiate(_abilityItemPrefab).GetComponent<AbilityItemUI>();
        abilityItemUI.SetUI(cardInfo, ability);
        
        _queue.Enqueue(abilityItemUI);
        _grid.AddAbility(abilityItemUI.transform);
        _grid.Open(_queue.Count);
    }

    public void WindowIn()
    {
        // print("Ability Queue window in");
        _isOpen = true;
        FadeWindow(0, 1, SorsTimings.waitShort).Forget();
    }

    public void WindowOut()
    {
        _queue.Clear();
        _spawnParentTransform.DestroyChildren();

        _isOpen = false;
        FadeWindow(1, 0, SorsTimings.waitShort).Forget();
    }

    private async UniTaskVoid FadeWindow(float start, float end, float durationMiliseconds)
    {
        float time = 0;
        while (time < durationMiliseconds)
        {
            time += Time.deltaTime * 1000; // Time.deltaTime is in seconds
            _canvasGroup.alpha = Mathf.Lerp(start, end, time / durationMiliseconds);
            await UniTask.Yield();
        }

        _canvasGroup.alpha = end;
        _canvasGroup.blocksRaycasts = end == 1;
        _canvasGroup.interactable = end == 1;
    }
}
