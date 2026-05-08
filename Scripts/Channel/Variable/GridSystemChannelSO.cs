using UnityEngine;

[CreateAssetMenu(fileName = "GridSystemChannel", menuName = "ScriptableObjects/Channels/GridSystemChannel", order = SOAssetMenuIndex.Channel)]

public class GridSystemChannelSO : ScriptableObject
{
    private GridSystem gridSystem;
    public void SetUpChannel(GridSystem gridSystem)
    {
        this.gridSystem = gridSystem;
    }

    public int GetCrowdedPositions(int maxCells, Vector3[] positions)
    {
        return gridSystem.GetCrowdedPositions(maxCells, positions);
    }

    public int GetEnemiesFromCrowdedPosition(int maxCells, Transform[] enemies)
    {
        return gridSystem.GetEnemiesFromCrowdedPosition(maxCells, enemies);
    }

    public bool TryGetEnemyNearby(Transform enemy, out Transform neighbor)
    {
        return gridSystem.TryGetEnemyNearby(enemy, out neighbor);
    }

    public bool TryGetEnemyNearby(Vector3 position, out Transform neighbor)
    {
        return gridSystem.TryGetEnemyNearby(position, out neighbor);
    }


}