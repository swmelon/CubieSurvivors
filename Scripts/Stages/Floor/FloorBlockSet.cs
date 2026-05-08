using UnityEngine;


[CreateAssetMenu(fileName = "FloorBlockSet", menuName = "ScriptableObjects/FloorBlockSet", order = 1)]
public class FloorBlockSet : ScriptableObject
{
    public GameObject CubePrefab;
    public LEDNode HalfCornerCubePrefab, OneSidedCornerCubePrefab, TwoSidedCornerCubePrefab, ThreeSidedCornerCubePrefab;
    public GameObject ExplosivePrefab;
    public CornerLEDCubePool CornerLEDCubePool;
}