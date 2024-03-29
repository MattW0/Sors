using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using DG.Tweening;


public class CombatVFXSystem : MonoBehaviour
{
    public static CombatVFXSystem Instance { get; private set; }
    public GameObject attackVFXPrefab;
    public GameObject damageVFXPrefab;
    public float damageTime = 10f;
    public float attackTime = 3f;
    public bool IsDone { get; set; }

    private void Awake()
    {
        if (!Instance) Instance = this;
        print("Combat vfx system instantiated");
    }

    public void Start()
    {
        // Instantiate and set active false
        attackVFXPrefab.SetActive(false);
        damageVFXPrefab.SetActive(false);
    }
    public IEnumerator PlayDamage(Vector3 position, int damage)
    {
        damageVFXPrefab.transform.position = position;
        damageVFXPrefab.SetActive(true);

        yield return new WaitForSeconds(damageTime);

        damageVFXPrefab.SetActive(false);
    }

    public void PlayAttack(Transform start, Transform end)
    {
        attackVFXPrefab.transform.position = start.position;

        var rotation = Quaternion.LookRotation(end.position - start.position);
        attackVFXPrefab.transform.rotation = rotation;

        attackVFXPrefab.SetActive(true);

        attackVFXPrefab.transform
            .DOMove(end.position, attackTime)
            .SetEase(Ease.InOutCubic)
            .OnComplete(() => attackVFXPrefab.SetActive(false));

        // yield return new WaitForSeconds(attackTime);

    }
}
