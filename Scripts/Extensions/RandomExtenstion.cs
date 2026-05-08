using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace Local.Scripts.Extensions
{
    public static class RandomExtenstion
    {
        private static Random rnd;
        private const float minRandomDamageRate = 0.9f;
        private const float maxRandomDamageRate = 1.1f;
        static RandomExtenstion()
        {
            int tickCount = Environment.TickCount;

            // Use the current time to generate a seed value
            int seed = tickCount ^ Guid.NewGuid().GetHashCode();
            rnd = new Random(seed);
        }

        public static T PickRandom<T>(this IList<T> source)
        {
            int randIndex = rnd.Next(source.Count);
            return source[randIndex];
        }

        public static bool TryPickRandom<T>(this IList<T> source, out T result)
        {
            if (source.Count == 0)
            {
                result = default;
                return false;
            }

            int randIndex = rnd.Next(source.Count);
            result = source[randIndex];
            return true;
        }

        public static T[] PickRandom<T>(this IList<T> source, int num)
        {
            if (source.Count < num)
            {
                Debug.LogWarning("Pickable count is less than num.");
                num = source.Count;
            }

            source.FisherShuffle();
            return source.ToList().GetRange(0, num).ToArray();
        }

        public static bool TryPickRandom<T>(this IList<T> source, int num, out T[] result)
        {
            if (source.Count < num)
            {
                result = null;
                return false;
            }

            source.FisherShuffle();
            result = source.ToList().GetRange(0, num).ToArray();
            return true;
        }

        public static T PickRandom<T>(this IEnumerable<T> source)
        {
            int randIndex = rnd.Next(source.Count());
            return source.ElementAt(randIndex);
        }

        public static T PickRandom<T>(this Dictionary<Enum, T[]> source)
        {
            int sumLength = 0;
            foreach (var array in source.Values)
            {
                sumLength += array.Length;
            }

            int randIndex = rnd.Next(sumLength);

            foreach (var array in source.Values)
            {
                if (randIndex >= array.Length)
                {
                    randIndex -= array.Length;
                }
                else
                {
                    return array[randIndex];
                }
            }

            Debug.LogError("flawed algorithm");
            return source.Values.GetEnumerator().Current[0];
        }

        public static int GetIntInRange(int min, int max)
        {
            return rnd.Next(min, max + 1);
        }

        public static float GetFloatInRange(float min, float max)
        {
            return (float)rnd.NextDouble() * (max - min) + min;
        }

        public static int RandomizeDamage(int damage)
        {
            return Mathf.RoundToInt(damage * GetFloatInRange(minRandomDamageRate, maxRandomDamageRate));
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.OrderBy(_ => rnd.Next());
        }

        public static void FisherShuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rnd.Next(n + 1);
                (list[n], list[k]) = (list[k], list[n]);
            }
        }

        public static bool FiftyFifty()
        {
            return rnd.Next(2) == 1; // 0 또는 1 반환
        }

        public static float GetRandomProbability()
        {
            return (float)rnd.NextDouble();
        }

        public static bool IsHappen(float probability)
        {
            return GetRandomProbability() <= probability;
        }

        public static double SampleNormal()
        {
            // The method requires sampling from a uniform random of (0,1]
            // but Random.NextDouble() returns a sample of [0,1).
            double x1 = 1 - rnd.NextDouble();
            double x2 = 1 - rnd.NextDouble();

            double y1 = Math.Sqrt(-2.0 * Math.Log(x1)) * Math.Cos(2.0 * Math.PI * x2);
            return y1;
        }

        public static Vector3 GetRandomXZVector3(float min, float max)
        {
            return new Vector3(GetFloatInRange(min, max), 0f, GetFloatInRange(min, max));
        }

        public static AccessoryRank GetRandomRank(float rareProp = 0.2f)
        {
            if (IsHappen(rareProp))
            {
                return AccessoryRank.Rare;
            }

            return AccessoryRank.Common;
        }
    }
}