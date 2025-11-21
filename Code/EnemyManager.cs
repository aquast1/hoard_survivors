using Sandbox;

public sealed class EnemyManager : Component, HealthComponent.IEvents
{
  [Property] public GameManager GameManager;
  [Property] public GameObject ZombiePrefab { get; set; }
  [Property] public int MaxSpawnedEnemies = 1;
  [Property] public int EnemiesPerRound = 1;

  [Sync] public NetList<GameObject> Enemies { get; set; } = new();

  [Sync] public int EnemiesSpawned { get; set; }

  [Sync] public int EnemiesKilled { get; set; }

  protected override void OnUpdate()
  {
    if ( GameManager.RoundBreak ) return;

    if ( Enemies.Count >= MaxSpawnedEnemies || EnemiesSpawned >= EnemiesPerRound ) return;

    SpawnEnemy();
  }

  public void SpawnEnemy()
  {
    var zombie = ZombiePrefab.Clone( Vector3.Zero );
    zombie.NetworkSpawn();
    Enemies.Add( zombie );
    EnemiesSpawned++;
  }

  void HealthComponent.IEvents.OnKilled( GameObject gameObject )
  {
    Enemies.Remove( gameObject );
    gameObject.Destroy();
    EnemiesKilled++;

    if ( EnemiesKilled >= EnemiesPerRound ) GameManager.StartRoundBreak();
  }
}
