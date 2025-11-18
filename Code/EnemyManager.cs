using Sandbox;

public sealed class EnemyManager : Component, HealthComponent.IEvents
{
  [Property] public GameObject ZombiePrefab { get; set; }

  [Property] public int MaxSpawnedEnemies = 1;

  [Property] public int EnemiesPerRound = 1;

  [Sync] public List<GameObject> Enemies { get; set; }

  [Sync] public int EnemiesSpawned { get; set; }

  [Sync] public int EnemiesKilled { get; set; }

  protected override void OnStart()
  {
    Enemies = new();
    EnemiesSpawned = EnemiesPerRound;
    EnemiesKilled = EnemiesPerRound;
  }

  protected override void OnUpdate()
  {
    if (IsProxy) return;

    if (Enemies.Count >= MaxSpawnedEnemies || EnemiesSpawned >= EnemiesPerRound) return;

    SpawnEnemy();
  }

  public void SpawnEnemy()
  {
    var zombie = ZombiePrefab.Clone(Vector3.Zero);
    zombie.NetworkSpawn();
    Enemies.Add(zombie);
    EnemiesSpawned++;
  }

  void HealthComponent.IEvents.OnKilled(GameObject gameObject)
  {
    if (IsProxy) return;
    Enemies.Remove(gameObject);
    gameObject.Destroy();
    EnemiesKilled++;
  }
}
