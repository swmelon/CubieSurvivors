using System;
using System.Collections.Generic;
using System.Linq;
using static Accessory;

public static class AccessoryGenerator
{
    /// <summary>
    /// Generates all possible AccessoryStats combinations for a given rank.
    /// </summary>
    /// <param name="rank">The rank of the accessory.</param>
    /// <returns>A list of all possible AccessoryStats combinations for the rank.</returns>
    public static List<AccStats> GenerateAccessoryStats(AccessoryRank rank)
    {
        var result = new List<AccStats>();
        int n = (int)rank; // 랭크에 따른 기본 추가 포인트
        int[] baseStats = { 5, 5, 5, 5 }; // Attack, Defense, Agility, Luck
        int numStats = 4;

        // 가능한 감소 수 (d): 0부터 n까지
        for (int d = 0; d <= n && d <= numStats; d++)
        {
            // 가능한 d개의 스탯을 선택하여 1점씩 감소
            var decreaseCombinations = GetCombinations(Enumerable.Range(0, numStats).ToArray(), d);

            foreach (var decreaseIndices in decreaseCombinations)
            {
                // 각 조합에 대해, 선택된 스탯을 1점 감소
                int[] currentStats = (int[])baseStats.Clone();
                bool isValidDecrease = true;

                foreach (var index in decreaseIndices)
                {
                    currentStats[index] -= 1;
                    if (currentStats[index] < 1)
                    {
                        isValidDecrease = false;
                        break; // 스탯이 1 미만으로 감소하면 무효
                    }
                }

                if (!isValidDecrease)
                    continue;

                // 총 추가 포인트: n + d
                int totalPoints = n + d;

                // 감소되지 않은 스탯의 인덱스
                var addableIndices = Enumerable.Range(0, numStats).Where(i => !decreaseIndices.Contains(i)).ToArray();
                int addableCount = addableIndices.Length;

                if (addableCount == 0)
                {
                    // 모든 스탯이 감소된 경우, 추가 포인트를 분배할 스탯이 없음
                    // 단, totalPoints이 0인 경우에만 유효
                    if (totalPoints == 0)
                    {
                        var accessory = new AccStats(
                            currentStats[0],
                            currentStats[1],
                            currentStats[2],
                            currentStats[3]
                        );
                        result.Add(accessory);
                    }
                    continue;
                }

                // 추가 포인트를 분배할 스탯에 최대 추가 가능한 포인트 계산
                int[] maxIncreases = new int[addableCount];
                for (int i = 0; i < addableCount; i++)
                {
                    maxIncreases[i] = 9 - currentStats[addableIndices[i]];
                }

                // 모든 가능한 포인트 분배 생성
                var distributions = GenerateDistributions(totalPoints, addableCount, maxIncreases);

                foreach (var distribution in distributions)
                {
                    int[] finalStats = (int[])currentStats.Clone();
                    bool isValid = true;

                    for (int i = 0; i < addableCount; i++)
                    {
                        finalStats[addableIndices[i]] += distribution[i];
                        if (finalStats[addableIndices[i]] > 9)
                        {
                            isValid = false;
                            break;
                        }
                    }

                    if (isValid)
                    {
                        var accessory = new AccStats(
                            finalStats[0],
                            finalStats[1],
                            finalStats[2],
                            finalStats[3]
                        );
                        result.Add(accessory);
                    }
                }
            }
        }

        // 중복 제거
        return result.Distinct().ToList();
    }

    /// <summary>
    /// Helper method to get all combinations of 'k' elements from 'array'.
    /// </summary>
    private static List<int[]> GetCombinations(int[] array, int k)
    {
        var result = new List<int[]>();
        GetCombinationsRecursive(array, k, 0, new List<int>(), result);
        return result;
    }

    private static void GetCombinationsRecursive(int[] array, int k, int start, List<int> current, List<int[]> result)
    {
        if (current.Count == k)
        {
            result.Add(current.ToArray());
            return;
        }

        for (int i = start; i < array.Length; i++)
        {
            current.Add(array[i]);
            GetCombinationsRecursive(array, k, i + 1, current, result);
            current.RemoveAt(current.Count - 1);
        }
    }

    /// <summary>
    /// Helper method to generate all distributions of 'points' among 'slots' with maximum limits.
    /// </summary>
    private static List<int[]> GenerateDistributions(int points, int slots, int[] maxLimits)
    {
        var result = new List<int[]>();
        if (slots == 0)
            return result;

        int[] distribution = new int[slots];
        GenerateDistributionsRecursive(points, slots, maxLimits, distribution, 0, result);
        return result;
    }

    private static void GenerateDistributionsRecursive(int points, int slots, int[] maxLimits, int[] distribution, int index, List<int[]> result)
    {
        if (index == slots - 1)
        {
            if (points <= maxLimits[index])
            {
                distribution[index] = points;
                result.Add((int[])distribution.Clone());
            }
            return;
        }

        for (int i = 0; i <= Math.Min(points, maxLimits[index]); i++)
        {
            distribution[index] = i;
            GenerateDistributionsRecursive(points - i, slots, maxLimits, distribution, index + 1, result);
        }
    }
}
