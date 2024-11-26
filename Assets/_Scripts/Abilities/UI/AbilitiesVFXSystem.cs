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

    private WaitForSeconds _wait = new WaitForSeconds(SorsTimings.effectProjectile);
    public WaitForSeconds Wait() => _wait;

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
        // var sourcePosition = source.gameObject.transform.position;
        var (projectilePrefab, projectileVFX) = effect switch
        {
            Effect.Damage => (damageProjectilePrefab, _damageProjectileVFX),
            Effect.Life => (healProjectilePrefab, _healProjectileVFX),
            Effect.Cash => (moneyProjectilePrefab, _moneyProjectileVFX),

            // TODO: Add more projectile type VFX
            Effect.CardDraw => (moneyProjectilePrefab, _moneyProjectileVFX),
            Effect.PriceReduction => (moneyProjectilePrefab, _moneyProjectileVFX),
            Effect.Curse => (damageProjectilePrefab, _damageProjectileVFX),
            _ => throw new NotImplementedException("Projectile VFX not implemented for effect: " + effect),
        };

        var sourcePosition = source.transform.position;
        var targetPosition = target.transform.position;
        var dir = Quaternion.LookRotation(targetPosition - sourcePosition).eulerAngles;

        projectilePrefab.transform.position = sourcePosition;
        projectilePrefab.transform.localRotation = Quaternion.Euler(dir.x, dir.y - 90f, dir.z);

        projectilePrefab.SetActive(true);
        projectileVFX.Play();

        projectilePrefab.transform
            .DOMove(targetPosition, SorsTimings.effectProjectile)
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
            Effect.Life => (healHitPrefab, _healHitVFX),
            Effect.Cash => (moneyHitPrefab, _moneyHitVFX),

            // TODO: Add more hit type VFX
            Effect.CardDraw => (moneyHitPrefab, _moneyHitVFX),
            Effect.PriceReduction => (moneyHitPrefab, _moneyHitVFX),
            Effect.Curse => (damageHitPrefab, _damageHitVFX),
            _ => throw new NotImplementedException("Hit VFX not implemented for effect: " + effect),
        };
        hitPrefab.transform.position = target.transform.position;

        RunHitVFX(hitVFX, hitPrefab).Forget();
    }

    private async UniTaskVoid RunHitVFX(ParticleSystem vfx, GameObject prefab)
    {
        prefab.SetActive(true);

        await UniTask.Delay(TimeSpan.FromSeconds(SorsTimings.effectHitVFX));

        prefab.SetActive(false);
    }
}