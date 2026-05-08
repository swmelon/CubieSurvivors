using UnityEngine;

[CreateAssetMenu(fileName = "ItemBoxSpawner", menuName = "ScriptableObjects/Spawner/ItemBoxSpawner", order = SOAssetMenuIndex.Spawner)]
public class ItemBoxSpawner : Spawner<ItemBox, Enemy>
{
    public override ItemBox Spawn()
    {
        ItemBox itemBox = base.Spawn();
        itemBox.RaiseSpawnEvent();
        
        return itemBox;
    }
}
