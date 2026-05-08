using Local.Scripts.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AccRankProbContainer", menuName = "ScriptableObjects/AccRankProbContainer")]
public class AccRankProbContainer : ScriptableObject
{
    [Serializable]
    private struct AccRankProb
    {
        public float commonProb;
        public float rareProb;
        public float epicProb;
        public float legendaryProb;
    }

    [SerializeField]
    private SerializableDictionary<DifficultyLevel, AccRankProb> probs;

    private Dictionary<DifficultyLevel, AccRankProb> probsDict;
    private bool isInitialized = false;
    private void Initialize()
    {
        probsDict = probs.ToDictionary();
    }

    public AccessoryRank GetRandomRank(DifficultyLevel difficultyLevel)
    {
        Initialize();

        var prob = probsDict[difficultyLevel];
        var rand = RandomExtenstion.GetFloatInRange(0f, 1f);
        if (rand < prob.commonProb)
        {
            return AccessoryRank.Common;
        }
        else if (rand < prob.commonProb + prob.rareProb)
        {
            return AccessoryRank.Rare;
        }
        else if (rand < prob.commonProb + prob.rareProb + prob.epicProb)
        {
            return AccessoryRank.Epic;
        }
        else
        {
            return AccessoryRank.Legendary;
        }
    }
}