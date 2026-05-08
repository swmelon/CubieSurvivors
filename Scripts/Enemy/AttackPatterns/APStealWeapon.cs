using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Local.Scripts.Extensions;

public class APStealWeapon : AttackPattern<WeaponUsableEnemy>
{
    [SerializeField]
    private ItemPackerSO itemPacker;

    private EnemyWeaponManager myWeaponManager;
    private bool stealWeapon = false;
    private float scaleUpTimeCount;
    private const float scaleUpTime = 0.5f;

    protected override void Awake()
    {
        base.Awake();
        myWeaponManager = GetComponent<EnemyWeaponManager>();
    }

    private void OnEnable()
    {
        stealWeapon = false;
    }

    protected override IEnumerator StartBehaviourRoutine()
    {
        // ���� �뽬 �ϸ鼭 Ʈ���ſ� �÷��̾ �ɸ��� ���⸦ �ϳ� ����߸��� ���� �����Ѵ�.
        // ������ ���⸦ ����߷����Ѵ�. (������ ���� ���� ��� ������ �ٸ���)


        enemyAnimationController.JumpForwardAndDive();
        yield return unitDelay;

        if (stealWeapon)
        {
            transform.localScale = Vector3.one;
        }

        OnFinishedBehaviour();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!stealWeapon && other.CompareTag("Player") && other.TryGetComponent(out WeaponManager weaponManager))
        {
            Weapon stolenWeapon = weaponManager.UnmountRandom();
            if (ReferenceEquals(stolenWeapon, null))
            {
                return;
            }
            myWeaponManager.Mount(stolenWeapon);
            stealWeapon = true;
        }
    }

    private IEnumerator ScaleUp()
    {
        scaleUpTimeCount = 0f;
        while (scaleUpTimeCount < scaleUpTime)
        {
            scaleUpTimeCount += Time.deltaTime;
            transform.localScale = Mathf.Lerp(transform.localScale.x, 1f, scaleUpTimeCount / scaleUpTime) * Vector3.one;
            yield return null;
        }
    }
}
