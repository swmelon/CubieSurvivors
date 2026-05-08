
using Local.Scripts.Extensions;
using UnityEngine;


[CreateAssetMenu(fileName = "WorldDirectionChannel", menuName = "ScriptableObjects/Channels/WorldDirectionChannel")]
public class WorldDirectionChannelSO : ScriptableObject, IDependentInitialization
{
    private Quaternion[] worldRotation = new []
    {
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 90, 0),
        Quaternion.Euler(0, 180, 0),
        Quaternion.Euler(0, 270, 0)
    };
    
    private WorldDirection worldDirection = WorldDirection.North;
    
    public WorldDirection WorldDirection
    {
        get => worldDirection;
        set
        {
            worldDirection = value;
        }
    }
    
    public Quaternion WorldRotation
    {
        get => worldRotation[(int) worldDirection];
    }

    public Quaternion Rotation(WorldDirection direction)
    {
        return worldRotation[(int) direction];
    }

    public Quaternion RandomRotation()
    {
        int randomIndex = RandomExtenstion.GetIntInRange(0, 3);
        return worldRotation[randomIndex];
    }

    public WorldDirection RotationToDirection(Quaternion rotation)
    {
        for (int i = 0; i < worldRotation.Length; i++)
        {
            if (worldRotation[i] == rotation)
            {
                return (WorldDirection) i;
            }
        }

        return WorldDirection.North;
    }

    public void Initialize()
    {
        worldDirection = WorldDirection.North;
    }
}
