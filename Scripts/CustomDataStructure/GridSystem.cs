using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
public class GridSystem
{
    private GridCell[,] grid;
    private float cellSize;
    private int gridSize;
    private Dictionary<int, HashSet<GridCell>> crowdedRank;
    private Dictionary<Transform, GridCell> enemyCellMap;
    private int maxBucket;

    public GridSystem(int gridSize, float cellSize, int maxBucket)
    {
        this.gridSize = gridSize;
        this.cellSize = cellSize;
        this.maxBucket = maxBucket;
        grid = new GridCell[gridSize, gridSize];
        crowdedRank = new Dictionary<int, HashSet<GridCell>>();
        enemyCellMap = new Dictionary<Transform, GridCell>();

        for (int i = 0; i <= maxBucket; i++)
        {
            crowdedRank[i] = new HashSet<GridCell>();
        }

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                grid[x, y] = new GridCell(x, y);
                grid[x, y].CenterWorldPos = GridToWorldPosition(grid[x, y]);
                crowdedRank[0].Add(grid[x, y]);
            }
        }
    }

    public GridCell UpdateEnemyPosition(Transform enemy)
    {
        Vector2Int gridPos = WorldToGridPosition(enemy.position);
        GridCell newCell = grid[gridPos.x, gridPos.y];

        if (enemyCellMap.TryGetValue(enemy, out GridCell currentCell))
        {
            if (currentCell != newCell)
            {
                currentCell.RemoveEnemy(enemy);
                UpdateBuckets(currentCell, -1);
            }
        }

        newCell.AddEnemy(enemy);
        UpdateBuckets(newCell, 1);
        enemyCellMap[enemy] = newCell; // Update the enemy's current cell

        if (newCell.Enemies.Count >= maxBucket)
        {
            DrawRayFromCell(newCell);
        }

        return newCell;
    }

    public void RemoveEnemy(Transform enemy)
    {
        if (enemyCellMap.TryGetValue(enemy, out GridCell currentCell))
        {
            currentCell.RemoveEnemy(enemy);
            UpdateBuckets(currentCell, -1);
            enemyCellMap.Remove(enemy);
        }
    }

    public void RemoveAllEnemies()
    {
        foreach (var cell in grid)
        {
            cell.RemoveAllEnemies();
        }

        foreach (var bucket in crowdedRank)
        {
            bucket.Value.Clear();
        }

        // Re-add all cells to the 0th bucket
        foreach (var cell in grid)
        { 
            crowdedRank[0].Add(cell);
        }

        enemyCellMap.Clear();
    }

    private Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt((worldPosition.x + (gridSize * cellSize) / 2) / cellSize);
        int y = Mathf.FloorToInt((worldPosition.z + (gridSize * cellSize) / 2) / cellSize);
        x = Mathf.Clamp(x, 0, gridSize - 1);
        y = Mathf.Clamp(y, 0, gridSize - 1);
        return new Vector2Int(x, y);
    }

    private Vector3 GridToWorldPosition(GridCell cell)
    {
        float worldX = (cell.X * cellSize) - (gridSize * cellSize) / 2 + cellSize / 2;
        float worldZ = (cell.Y * cellSize) - (gridSize * cellSize) / 2 + cellSize / 2;
        return new Vector3(worldX, 0, worldZ); 
    }

    private void UpdateBuckets(GridCell cell, int change)
    {
        int oldCount = cell.Enemies.Count - change; // 이미 적이 추가되었기 때문에 이전 상태를 알려면 change를 빼줘야함
        int newCount = Mathf.Clamp(oldCount + change, 0, maxBucket); // maxBucket을 넘어가지 않도록 제한(최대 적 수)

        // Remove the cell from its old bucket
        if (oldCount <= maxBucket && crowdedRank.ContainsKey(oldCount))
        {
            crowdedRank[oldCount].Remove(cell);
        }

        // Add the cell to its new bucket
        if (newCount <= maxBucket && crowdedRank.ContainsKey(newCount))
        {
            crowdedRank[newCount].Add(cell);
        }

        // Optionally, handle the case where newCount > maxBucket
    }

    /// <summary>
    /// </summary>
    /// <param name="maxCells">The maximum number of crowded positions to return</param>
    /// <param name="positions"></param>
    /// <returns></returns>
    public int GetCrowdedPositions(int maxCells, Vector3[] positions)
    {
        int count = 0;
        int length = positions.Length;
        maxCells = Mathf.Min(maxCells, length);

        for (int i = maxBucket; i > 0; i--)
        {
            foreach (var cell in crowdedRank[i])
            {
                positions[count] = GridToWorldPosition(cell);
                count++;
                if (count >= maxCells) break;
            }
            if (count >= maxCells) break;
        }

        return count;
    }
    /// <summary>
    /// maxCells: 최대로 뽑을 셀의 수 (=적의 수) 중복되는 구역은 없음
    /// enemies: 적을 담을 배열
    /// </summary>
    /// <param name="maxCells"></param>
    /// <param name="enemies"></param>
    /// <returns></returns>

    public int GetEnemiesFromCrowdedPosition(int maxCells, Transform[] enemies)
    {
        int count = 0;
        int length = enemies.Length;
        maxCells = Mathf.Min(maxCells, length);

        for (int i = maxBucket; i > 0; i--)
        {
            foreach(var cell in crowdedRank[i])
            { 
                // buckets[i] 안에 있는 적의 수는 동일(max이면 더 많을수도)
                // 이 버킷들에서 하나씩 적을 뽑는다
                if (!cell.TryGetRandomEnemy(out Transform enemy)) break;
                                
                enemies[count] = enemy;
                count++;

                if (count >= maxCells) break;
            }

            if (count >= maxCells) break;
        }

        return count;
    }

    public bool TryGetEnemyNearby(Transform enemy, out Transform neighbor)
    {
        Vector3 position = enemy.position;
        GridCell cell = UpdateEnemyPosition(enemy);

        if (cell.TryGetRandomEnemyExcept(enemy, out neighbor))
        {
            return true;
        }

        // compare position and CenterPos of the cell and find closest cell


        if (TryGetClosestCell(position, cell, out GridCell closestCell))
        {
            // 검증 완료
            if (closestCell.TryGetRandomEnemy(out neighbor))
            {
                return true;
            }
        }

        return false;
    }

    public bool TryGetEnemyNearby(Vector3 position, out Transform neighbor)
    {
        Vector2Int gridPos = WorldToGridPosition(position);
        GridCell cell = grid[gridPos.x, gridPos.y];

        Debug.DrawRay(cell.CenterWorldPos, Vector3.up * 10, Color.red, 5f); // Draws a ray upwards for 5 seconds

        if (cell.TryGetRandomEnemy(out neighbor))
        {
            return true;
        }

        // compare position and CenterPos of the cell and find closest cell


        if (TryGetClosestCell(position, cell, out GridCell closestCell))
        {
            Debug.DrawRay(closestCell.CenterWorldPos, Vector3.up * 10, Color.green, 5f); // Draws a ray upwards for 5 seconds

            if (closestCell.TryGetRandomEnemy(out neighbor))
            {
                return true;
            }
        }

        return false;
    }

    private bool TryGetClosestCell(Vector3 position, GridCell currentCell, out GridCell closestCell)
    {
        float dx = position.x - currentCell.CenterWorldPos.x;
        float dz = position.z - currentCell.CenterWorldPos.z;
        float absDx = Mathf.Abs(dx);
        float absDz = Mathf.Abs(dz);

        if (absDx > absDz)
        {
            if (dx > 0)
            {
                if (currentCell.X < gridSize - 1)
                {
                    closestCell = grid[currentCell.X + 1, currentCell.Y];
                    return true;
                }
            }
            else
            {
                if (currentCell.X> 0)
                {
                    closestCell = grid[currentCell.X - 1, currentCell.Y];
                    return true;
                }
            }
        }
        else
        {
            if (dz > 0)
            {
                if (currentCell.Y < gridSize - 1)
                {
                    closestCell = grid[currentCell.X, currentCell.Y + 1];
                    return true;
                }
            }
            else
            {
                if (currentCell.Y > 0)
                {
                    closestCell = grid[currentCell.X, currentCell.Y - 1];
                    return true;
                }
            }
        }

        closestCell = null;
        return false;
        }

    private void DrawRayFromCell(GridCell cell)
    {
        Vector3 cellCenter = GridToWorldPosition(cell);
        Debug.DrawRay(cellCenter, Vector3.up * 10, Color.yellow, 5f); // Draws a ray upwards for 5 seconds
    }
}