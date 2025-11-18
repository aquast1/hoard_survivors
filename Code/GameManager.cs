using Sandbox;

public sealed class GameManager : Component, Component.INetworkListener
{
  [Property]
  [ReadOnly]
  [Sync]
  public NetList<PlayerComponent> Players { get; set; } = new();

  [Property] public EnemyManager EnemyManager { get; set; }

  public int RoundBreakTime = 5;

  [Sync] public int CurrentRound { get; set; }

  [Sync] public TimeUntil NextRound { get; set; }

  private bool _roundBreak;
  [Sync]
  public bool RoundBreak
  {
    get
    {
      return _roundBreak;
    }
    set
    {
      _roundBreak = value;
      if ( _roundBreak )
      {
        NextRound = RoundBreakTime;
      }
    }
  }

  protected override void OnStart()
  {
    CurrentRound = 0;
    RoundBreak = true;
  }

  protected override void OnUpdate()
  {
    if ( NextRound <= 0 && RoundBreak )
    {
      StartRound();
    }

    if ( EnemyManager.EnemiesKilled >= EnemyManager.EnemiesPerRound && !RoundBreak )
    {
      RoundBreak = true;
    }
  }

  private void StartRound()
  {
    RoundBreak = false;
    EnemyManager.EnemiesKilled = 0;
    EnemyManager.EnemiesSpawned = 0;
    CurrentRound++;
  }

  public void OnActive( Connection connection )
  {
    UpdatePlayers();
  }

  public void OnDisconnected( Connection connection )
  {
    UpdatePlayers();
  }

  public void UpdatePlayers()
  {
    Players.Clear();
    foreach ( var player in Scene.GetAllComponents<PlayerComponent>() )
    {
      Log.Info( $"{player.PlayerId}" );
    }
  }
}
