using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Mirror;


public class CombatVFXSystem : NetworkBehaviour
{
    public static CombatVFXSystem Instance { get; private set; }
    public GameObject attackProjectilePrefab;
    private ParticleSystem _attackProjectileVFX;
    public GameObject attackHitPrefab;
    private ParticleSystem _attackHitVFX;

    private void Awake()
    {
        if (!Instance) Instance = this;
    }

    public void Start()
    {
        attackProjectilePrefab.SetActive(false);
        _attackProjectileVFX = attackProjectilePrefab.GetComponent<ParticleSystem>();
        _attackHitVFX = attackHitPrefab.GetComponent<ParticleSystem>();
    }

    [ClientRpc]
    public void RpcPlayDamage(BattleZoneEntity entity, int damage)
    {
        attackHitPrefab.transform.position = entity.gameObject.transform.position;
        _attackHitVFX.Play();
        attackHitPrefab.transform.DORotate(Vector3.zero, SorsTimings.damageTime).OnComplete(() => _attackHitVFX.Stop());
    }

    [ClientRpc]
    public void RpcPlayAttack(BattleZoneEntity source, BattleZoneEntity target)
    {
        var sourcePosition = source.gameObject.transform.position;
        attackProjectilePrefab.transform.position = sourcePosition;

        var tartgetPosition = target.gameObject.transform.position;
        var dir = Quaternion.LookRotation(tartgetPosition - sourcePosition).eulerAngles;
        attackProjectilePrefab.transform.localRotation = Quaternion.Euler(dir.x, dir.y - 90f, dir.z);

        attackProjectilePrefab.SetActive(true);
        _attackProjectileVFX.Play();

        attackProjectilePrefab.transform
            .DOMove(tartgetPosition, SorsTimings.attackTime)
            .SetEase(Ease.InCubic)
            .OnComplete(() =>  {
                _attackProjectileVFX.Stop();
                attackProjectilePrefab.SetActive(false);
            });
    }
}
