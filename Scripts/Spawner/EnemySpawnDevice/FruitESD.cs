public class FruitESD : OnFloorEnemySpawnDevice
{
    public override Enemy SpawnEnemy()
    {
        Enemy enemy = base.SpawnEnemy();

        if(!ReferenceEquals(enemy, null) && enemy.TryGetComponent(out EnemyAnimationController animController))
        {
            animController.Reconstruct();
        }

        Destroy(gameObject);
        
        return enemy;
    }
}