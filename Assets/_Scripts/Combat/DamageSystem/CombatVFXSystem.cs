using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using DG.Tweening;
using Mirror;


public class CombatVFXSystem : NetworkBehaviour
{
    public static CombatVFXSystem Instance { get; private set; }
    public GameObject attackVFXPrefab;
    public GameObject damageVFXPrefab;
    public bool IsDone { get; set; }
    private ParticleSystem _damageVFX;
    private ParticleSystem _attackVFX;

    private void Awake()
    {
        if (!Instance) Instance = this;
        print("Combat vfx system instantiated");
    }

    public void Start()
    {
        // Instantiate and set active false
        attackVFXPrefab.SetActive(false);
        _attackVFX = attackVFXPrefab.GetComponent<ParticleSystem>();
        _damageVFX = damageVFXPrefab.GetComponent<ParticleSystem>();
    }

    [ClientRpc]
    public void RpcPlayDamage(BattleZoneEntity entity, int damage)
    {
        damageVFXPrefab.transform.position = entity.gameObject.transform.position;
        _damageVFX.Play();
        damageVFXPrefab.transform.DORotate(Vector3.zero, SorsTimings.damageTime).OnComplete(() => _damageVFX.Stop());
    }

    [ClientRpc]
    public void RpcPlayAttack(BattleZoneEntity source, BattleZoneEntity target)
    {
        var sourcePosition = source.gameObject.transform.position;
        attackVFXPrefab.transform.position = sourcePosition;

        var tartgetPosition = target.gameObject.transform.position;
        var dir = Quaternion.LookRotation(tartgetPosition - sourcePosition).eulerAngles;
        attackVFXPrefab.transform.localRotation = Quaternion.Euler(dir.x, dir.y - 90f, dir.z);

        attackVFXPrefab.SetActive(true);
        _attackVFX.Play();

        attackVFXPrefab.transform
            .DOMove(tartgetPosition, SorsTimings.attackTime)
            .SetEase(Ease.InCubic)
            .OnComplete(() =>  {
                _attackVFX.Stop();
                attackVFXPrefab.SetActive(false);
            });
    }
}
