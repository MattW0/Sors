using UnityEngine;
using DG.Tweening;
using Mirror;

public class CombatVFXSystem : NetworkBehaviour
{
    public GameObject attackProjectilePrefab;
    public GameObject attackHitPrefab;
    private ParticleSystem _attackProjectileVFX;
    private ParticleSystem _attackHitVFX;

    private void Awake()
    {
        CombatClash.OnPlayDamage += RpcPlayDamage;
        CombatClash.OnPlayAttack += RpcPlayAttack;
    }

    private void Start()
    {
        attackProjectilePrefab.SetActive(false);
        _attackProjectileVFX = attackProjectilePrefab.GetComponent<ParticleSystem>();
        _attackHitVFX = attackHitPrefab.GetComponent<ParticleSystem>();
    }

    [ClientRpc]
    public void RpcPlayDamage(Transform entity)
    {
        attackHitPrefab.transform.position = entity.position;
        _attackHitVFX.Play();
        attackHitPrefab.transform.DORotate(Vector3.zero, SorsTimings.damageTime).OnComplete(() => _attackHitVFX.Stop());
    }

    [ClientRpc]
    public void RpcPlayAttack(Transform source, Transform target)
    {
        attackProjectilePrefab.transform.position = source.position;

        var dir = Quaternion.LookRotation(target.position - source.position).eulerAngles;
        attackProjectilePrefab.transform.localRotation = Quaternion.Euler(dir.x, dir.y - 90f, dir.z);

        attackProjectilePrefab.SetActive(true);
        _attackProjectileVFX.Play();

        attackProjectilePrefab.transform
            .DOMove(target.position, SorsTimings.attackTime)
            .SetEase(Ease.InCubic)
            .OnComplete(() =>  {
                _attackProjectileVFX.Stop();
                attackProjectilePrefab.SetActive(false);
            });
    }
}
