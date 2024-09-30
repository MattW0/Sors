using UnityEngine;
using DG.Tweening;
using Mirror;
using Cysharp.Threading.Tasks;
using System;

public class AbilitiesVFXSystem : NetworkBehaviour
{
    public static AbilitiesVFXSystem Instance { get; private set; }

    [Header("Projectiles")]
    public GameObject damageProjectilePrefab;
    public GameObject healProjectilePrefab;
    public GameObject moneyProjectilePrefab;
    private ParticleSystem _damageProjectileVFX;
    private ParticleSystem _healProjectileVFX;
    private ParticleSystem _moneyProjectileVFX;

    [Header("Hits")]
    public GameObject damageHitPrefab;
    public GameObject healHitPrefab;
    public GameObject moneyHitPrefab;
    private ParticleSystem _damageHitVFX;
    private ParticleSystem _healHitVFX;
    private ParticleSystem _moneyHitVFX;


    private void Awake()
    {
        if (!Instance) Instance = this;
    }

    public void Start()
    {
        damageProjectilePrefab.SetActive(false);
        healProjectilePrefab.SetActive(false);
        moneyProjectilePrefab.SetActive(false);
        _damageProjectileVFX = damageProjectilePrefab.GetComponent<ParticleSystem>();
        _healProjectileVFX = healProjectilePrefab.GetComponent<ParticleSystem>();
        _moneyProjectileVFX = moneyProjectilePrefab.GetComponent<ParticleSystem>();

        damageHitPrefab.SetActive(false);
        healHitPrefab.SetActive(false);
        moneyHitPrefab.SetActive(false);
        _damageHitVFX = damageHitPrefab.GetComponent<ParticleSystem>();
        _healHitVFX = healHitPrefab.GetComponent<ParticleSystem>();
        _moneyHitVFX = moneyHitPrefab.GetComponent<ParticleSystem>();
    }

    [ClientRpc]
    public void RpcPlayProjectile(BattleZoneEntity source, BattleZoneEntity target, Effect effect)
    {
        var sourcePosition = source.gameObject.transform.position;
        var (projectilePrefab, projectileVFX) = effect switch
        {
            Effect.Damage => (damageProjectilePrefab, _damageProjectileVFX),
            Effect.LifeGain => (healProjectilePrefab, _healProjectileVFX),
            Effect.MoneyGain => (moneyProjectilePrefab, _moneyProjectileVFX),
            _ => (null, null),
        };
        projectilePrefab.transform.position = sourcePosition;

        var tartgetPosition = target.gameObject.transform.position;
        var dir = Quaternion.LookRotation(tartgetPosition - sourcePosition).eulerAngles;
        projectilePrefab.transform.localRotation = Quaternion.Euler(dir.x, dir.y - 90f, dir.z);

        projectilePrefab.SetActive(true);
        projectileVFX.Play();

        projectilePrefab.transform
            .DOMove(tartgetPosition, SorsTimings.effectProjectile)
            .SetEase(Ease.InCubic)
            .OnComplete(() => {
                projectileVFX.Stop();
                projectilePrefab.SetActive(false);
            });
    }

    [ClientRpc]
    public void RpcPlayHit(BattleZoneEntity target, Effect effect)
    {
        var (hitPrefab, hitVFX) = effect switch
        {
            Effect.Damage => (damageHitPrefab, _damageHitVFX),
            Effect.LifeGain => (healHitPrefab, _healHitVFX),
            Effect.MoneyGain => (moneyHitPrefab, _moneyHitVFX),
            _ => (null, null),
        };
        hitPrefab.transform.position = target.gameObject.transform.position;

        RunHitVFX(hitVFX, hitPrefab).Forget();
    }

    private async UniTaskVoid RunHitVFX(ParticleSystem vfx, GameObject prefab)
    {
        prefab.SetActive(true);

        await UniTask.Delay(TimeSpan.FromSeconds(SorsTimings.effectHitVFX));

        prefab.SetActive(false);
    }
}
