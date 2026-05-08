using Local.Scripts.Extensions;
using System.Collections.Generic;
using UnityEngine;

public class BossBuilder : MonoBehaviour
{
    [SerializeField]
    private float weaponsPPRCost;

    [SerializeField]
    [Range(1f, 1.5f)]
    private float upgradePPRConst;

    [SerializeField]
    private float initialBossMaxHealth = 1000f;

    [SerializeField]
    [Range(1f, 3f)]
    private float bossMoveSpeedAdvantage;

    [SerializeField]
    private List<WeaponType> excludedWeapons;

    [SerializeField]
    private BossSpawnDevice bossSpawnDevice, finalBossSpawnDevice;

    [SerializeField]
    private GameWeaponManagerSO gameWeaponManager;

    [SerializeField]
    private PlayerChannelSO currentPlayerChannel;

    [SerializeField]
    private bool debugWeapon;

    private FloorLEDBuilder floorLEDBuilder;
    private Player player;
    private Boss boss, prevBoss;
    private List<int> weaponsNumberOfUpgrades = new List<int>();

    private void Awake()
    {
        floorLEDBuilder = transform.root.GetComponentInChildren<FloorLEDBuilder>();
        currentPlayerChannel.Subscribe(SetPlayer);
    }
    
    public void BuildBoss(int ppr, float bossPowerMultiplier)
    {
        BuildBoss(bossSpawnDevice, ppr, bossPowerMultiplier);
    }

    public void BuildFinalBoss(int ppr, float bossPowerMultiplier)
    {
        BuildBoss(finalBossSpawnDevice, ppr, bossPowerMultiplier);
    }

    public void BuildFinalBoss(int ppr, float bossPowerMultiplier, Weapon weapon)
    {
        BuildBoss(finalBossSpawnDevice, ppr, bossPowerMultiplier, weapon);
    }

    private void BuildBoss(BossSpawnDevice devicePrefab, int ppr, float bossPowerMultiplier, Weapon weapon= null)
    {
        BossSpawnDevice device = Instantiate(devicePrefab);

        floorLEDBuilder.TryGetOnFloorObjectPosition(out Vector3 position);
        device.transform.SetParent(floorLEDBuilder.transform);
        device.transform.position = position;

        EnemyData enemyData = device.EnemyData;
        enemyData.MaxHealth = (int)(initialBossMaxHealth * bossPowerMultiplier);
        enemyData.Color = ColorExtension.GenerateRandomVividColor();
        enemyData.MoveSpeed = player.MoveSpeed + bossMoveSpeedAdvantage;


        if (boss != null)
        {
            prevBoss = boss;
        }

        // Boss�� �θ� Stage�� �����Ͽ� �Ʒ������� �ö���� �ϸ�, �ٴ��� �մ°��� ���´�.
        boss = device.SpawnBoss(floorLEDBuilder.transform, enemyData);

        if (weapon == null)
        {
            EquipRandomWeapons(boss, ppr);
        }
        else
        {
            EquipWeapon(boss, ppr, weapon);
        }
    }


    private void EquipRandomWeapons(Boss boss, int ppr)
    {
        WeaponSet weaponSet = gameWeaponManager.WeaponSet;
        List<Weapon> weapons = new List<Weapon>();

        if (!debugWeapon && !weaponSet.TryGetRandomTypeOfWeaponInstances(out weapons, boss.WeaponManager, excludedWeapons))
        {
            Debug.LogError("No weapon is available.");
        }
        if (debugWeapon)
        {

            if (!weaponSet.TryGetSpecificWeaponInstance(out Weapon WeaponForTest))
            {
                Debug.LogError("No weapon is available.");
            }

            weapons.Add(WeaponForTest);
        }


        int maxNumWeapons = Mathf.RoundToInt(ppr / weaponsPPRCost);
        maxNumWeapons = maxNumWeapons > weapons.Count ? weapons.Count : maxNumWeapons;

        int numWeapons = RandomExtenstion.GetIntInRange(1, maxNumWeapons);

        if (numWeapons == 0)
        {
            Debug.LogError("No boss weapon is selected.");
            return;
        }

        List<Weapon> selectedWeapons = weapons.GetRange(0, numWeapons);

        // Return Weapons that are not selected.
        foreach (var weapon in weapons)
        {
            if (!selectedWeapons.Contains(weapon))
            {
                weaponSet.ReturnWeaponInstance(weapon);
            }
        }

        SetEachWeaponUpgrades(numWeapons, ppr);
        UpgradedWeaponInstances(ref selectedWeapons, weaponsNumberOfUpgrades);

        foreach (var weapon in selectedWeapons)
        {
            if (!boss.MountWeapon(weapon))
            {
                weaponSet.ReturnWeaponInstance(weapon);
            }
        }
    }

    private void SetEachWeaponUpgrades(int numWeapons, int ppr)
    {
        weaponsNumberOfUpgrades.Clear();

        for (int i = 0; i < numWeapons; i++)
        {
            weaponsNumberOfUpgrades.Add(0);
        }

        while (CalculateCombinedPPR() < ppr)
        {
            int index = RandomExtenstion.GetIntInRange(0, weaponsNumberOfUpgrades.Count - 1);
            weaponsNumberOfUpgrades[index] += 1;
        }
    }

    private void EquipWeapon(Boss boss, int ppr, Weapon weapon)
    {
        WeaponSet weaponSet = gameWeaponManager.WeaponSet;

        if (!weaponSet.TryGetLockedWeaponInstance(weapon, out Weapon weaponInstance))
        {
            EquipRandomWeapons(boss, ppr);
            return;
        }

        List<Weapon> weapons = new List<Weapon>();
        weapons.Add(weaponInstance);

        SetEachWeaponUpgrades(1, ppr);
        UpgradedWeaponInstances(ref weapons, weaponsNumberOfUpgrades);

        if (!boss.MountWeapon(weaponInstance))
        {
            // �Ͼ �� ���� ��
            return;
        }
    }

    private void SetPlayer(Player newPlayer)
    {
        player = newPlayer;
    }

    private float CalculateCombinedPPR()
    {
        float combinedPPR = 0;

        foreach (var numUpgrades in weaponsNumberOfUpgrades)
        {
            combinedPPR += weaponsPPRCost * Mathf.Pow(upgradePPRConst, numUpgrades);
        }

        return combinedPPR;
    }

    private void UpgradedWeaponInstances(ref List<Weapon> weapons, List<int> numUpgrades)
    {
        for (int i = 0; i < weapons.Count; i++)
        {
            weapons[i].UpgradeRandom(numUpgrades[i]);
        }
    }

    public void DestroyBoss()
    {
        if (prevBoss != null)
        {
            Destroy(prevBoss.gameObject);
            prevBoss = null;
            return;
        }

        if (boss != null && boss.IsDead)
        {
            Destroy(boss.gameObject);
            boss = null;
        }
    }
}