using UnityEngine;

public struct StageBlueprint
{
    public StageType StageType;
    public int Padding;
    public int Size;
    public float Threshold;
    public int[,] HeightMap;
    public WaterType LiquidType;
    public Color PillarColor; 
    public Color FogColor; // 여기 있어야하나?
}