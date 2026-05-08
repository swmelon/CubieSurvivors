using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Accessory;

[CreateAssetMenu(fileName = "AccessoryStatsDatabase", menuName = "ScriptableObjects/Game Data/Accessory Database")]
public class AccessoryStatsDatabase : ScriptableObject
{
    [Header("Common Accessories")]
    public List<AccStats> CommonAccessories;

    [Header("Rare Accessories")]
    public List<AccStats> RareAccessories;

    [Header("Epic Accessories")]
    public List<AccStats> EpicAccessories;

    [Header("Legendary Accessories")]
    public List<AccStats> LegendaryAccessories;

    [SerializeField]
    private GameAccessoryManager gameAccessoryManager;

    private bool isInitialized = false;

    private void OnEnable()
    {
        if (true)
        {
            InitializeDatabase();
            isInitialized = true;
        }
    }

    /// <summary>
    /// Initializes the accessory database by generating all possible AccessoryStats for each rank.
    /// </summary>
    private void InitializeDatabase()
    {
        CommonAccessories = AccessoryGenerator.GenerateAccessoryStats(AccessoryRank.Common);
        RareAccessories = AccessoryGenerator.GenerateAccessoryStats(AccessoryRank.Rare);
        EpicAccessories = AccessoryGenerator.GenerateAccessoryStats(AccessoryRank.Epic);
        LegendaryAccessories = AccessoryGenerator.GenerateAccessoryStats(AccessoryRank.Legendary);

        Debug.Log("Accessory Database Initialized.");
    }

    /// <summary>
    /// Retrieves n random AccessoryStats of the specified rank, excluding the currentStats.
    /// </summary>
    /// <param name="rank">The rank of the accessory.</param>
    /// <param name="currentStats">List of currently owned AccessoryStats to exclude.</param>
    /// <param name="n">Number of random accessories to retrieve.</param>
    /// <returns>List of random AccessoryStats.</returns>
    public List<AccStats> GetRandomAccessoryStats(AccessoryRank rank, HashSet<AccStats> currentStats, int n)
    {
        List<AccStats> allStats = GetAllStats(rank);

        // ������ ���� ���͸�
        List<AccStats> availableStats = allStats.Where(stat => !currentStats.Contains(stat)).ToList();

        if (availableStats.Count < n)
        {
            Debug.LogWarning($"Not enough available accessory stats to return {n} unique combinations for rank {rank}.");
            return new List<AccStats>();
        }

        List<AccStats> selectedStats = new List<AccStats>();
        System.Random rand = new System.Random();

        for (int i = 0; i < n; i++)
        {
            int index = rand.Next(availableStats.Count);
            selectedStats.Add(availableStats[index]);
            availableStats.RemoveAt(index); // �ߺ� ������ ���� ����
        }

        return selectedStats;
    }

    public List<AccStats> GetRandomAccessoryStats(Accessory accessory, AccessoryRank rank, int n)
    {
        HashSet<AccStats> existingStats = new HashSet<AccStats>();

        foreach (var pair in gameAccessoryManager.AccessoriesOnShelf)
        {
            Accessory acc = pair.Item1;

            if (ReferenceEquals(acc, null) || acc.ID != accessory.ID)
            {
                continue;
            }

            List<AccData> datas = pair.Item2;

            for (int i = 0; i < datas.Count; i++)
            {
                // �� ������ �̹� ���� �Ǿ�����. �����ϴ� ��
                existingStats.Add(datas[i].accessoryStats);
            }
        }

        foreach (var accEquippedList in gameAccessoryManager.AccessoriesEquipped.Values)
        {
            foreach (var accData in accEquippedList)
            {
                if (accData.accID == accessory.ID)
                {
                    // �� ������ �̹� ���� �Ǿ�����. �����ϴ� ��
                    existingStats.Add(accData.accessoryStats);
                }
            }
        }

        return GetRandomAccessoryStats(rank, existingStats, n);
    }

    /// <summary>
    /// Retrieves all AccessoryStats of the specified rank.
    /// </summary>
    /// <param name="rank">The rank of the accessory.</param>
    /// <returns>List of AccessoryStats.</returns>
    public List<AccStats> GetAllStats(AccessoryRank rank)
    {
        switch (rank)
        {
            case AccessoryRank.Common:
                return new List<AccStats>(CommonAccessories);
            case AccessoryRank.Rare:
                return new List<AccStats>(RareAccessories);
            case AccessoryRank.Epic:
                return new List<AccStats>(EpicAccessories);
            case AccessoryRank.Legendary:
                return new List<AccStats>(LegendaryAccessories);
            default:
                return new List<AccStats>();
        }
    }
}
