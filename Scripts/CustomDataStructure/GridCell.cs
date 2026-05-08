using Local.Scripts.Extensions;
using System.Collections.Generic;
using UnityEngine;

public class GridCell
{
    public HashSet<Transform> Enemies { get; private set; }
    public int X { get; set; } // Grid X coordinate
    public int Y { get; set; } // Grid Y coordinate

    public Vector3 CenterWorldPos { get; set; }

    public GridCell(int x, int y)
    {
        Enemies = new HashSet<Transform>();
        X = x;
        Y = y;
    }

    public void AddEnemy(Transform enemy)
    {
        Enemies.Add(enemy);
    }

    public void RemoveEnemy(Transform enemy)
    {
        Enemies.Remove(enemy);
    }
    public void RemoveAllEnemies()
    {
        Enemies.Clear();
    }

    public bool TryGetRandomEnemy(out Transform enemy)
    {
        if (Enemies.Count == 0)
        {
            enemy = null;
            return false;
        }
            
        enemy = Enemies.PickRandom();
        return true;
    }

    public bool TryGetRandomEnemyExcept(Transform enemy, out Transform neighbor)
    {
        if (!Enemies.Remove(enemy))
        {
            Debug.LogWarning("enemy is not in this cell");
            neighbor = null;
            return false;
        }

        if (Enemies.Count == 0)
        {
            neighbor = null;
            return false;
        }

        neighbor = Enemies.PickRandom();
        Enemies.Add(enemy);
        return true;
    }
}
