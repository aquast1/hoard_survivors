using Sandbox;

public sealed class GameManager : GameObjectSystem<GameManager>, ISceneStartup
{
  public string TestVar = "test";

  public GameManager( Scene scene ) : base( scene )
  {
  }

  void ISceneStartup.OnHostInitialize()
  {
    TestVar = "test2";
  }

  // [Property] public EnemyManager EnemyManager { get; set; }

  // [Property] public float RoundBreakTime = 5f;

  // [Sync] public int CurrentRound { get; set; }

  // [Sync] public TimeUntil NextRound { get; set; }

  // private bool _roundBreak;
  // [Sync]
  // public bool RoundBreak
  // {
  //   get
  //   {
  //     return _roundBreak;
  //   }
  //   set
  //   {
  //     _roundBreak = value;
  //     if ( _roundBreak )
  //     {
  //       NextRound = RoundBreakTime;
  //     }
  //   }
  // }

  // protected override void OnStart()
  // {
  //   CurrentRound = 0;
  //   // StartRound();
  //   RoundBreak = true;
  // }

  // protected override void OnUpdate()
  // {
  //   if ( NextRound <= 0 && RoundBreak )
  //   {
  //     StartRound();
  //   }

  //   if ( EnemyManager.EnemiesKilled >= EnemyManager.EnemiesPerRound && !RoundBreak )
  //   {
  //     RoundBreak = true;
  //   }
  // }

  // private void StartRound()
  // {
  //   RoundBreak = false;
  //   EnemyManager.EnemiesKilled = 0;
  //   EnemyManager.EnemiesSpawned = 0;
  //   CurrentRound++;
  // }
}
