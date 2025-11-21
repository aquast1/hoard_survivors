using Sandbox;
using System;

public sealed class NetworkPlayer : Component, GameManager.IEvents
{
  public interface IEvents
  {
    void OnUpgrade( NetworkPlayer networkPlayer, Upgrade upgrade ) { }
  }

  public GameManager GameManager;

  private Guid _connectionId;
  [Sync]
  public Guid ConnectionId
  {
    get => _connectionId;
    set
    {
      _connectionId = value;
      // If the connection is the current player, assign this to player client
      if ( _connectionId == Connection.Local.Id )
        Scene.GetComponentInChildren<PlayerClient>().NetworkPlayer = this;
    }
  }

  /// <summary>
  /// Pool of upgrades to randomly select from
  /// </summary>
  [Property]
  [Sync]
  public NetList<Upgrade> UpgradePool { get; set; }

  /// <summary>
  /// Upgrades this player has chosen
  /// </summary>
  [Property]
  [ReadOnly]
  [Sync]
  public NetList<Upgrade> ActiveUpgrades { get; set; } = new();

  /// <summary>
  /// Upgrades currently available for this upgrade round
  /// </summary>
  [Property]
  [ReadOnly]
  [Sync]
  public NetList<Upgrade> UpgradeOptions { get; set; } = new();

  public bool Ready = false;

  public int UpgradeRound = 0;

  protected override void OnStart()
  {
    GameManager = Scene.GetComponentInChildren<GameManager>();

    SetUpgradeOptions();
  }

  [Rpc.Broadcast( NetFlags.HostOnly )]
  public void SetUpgradeOptions()
  {
    if ( UpgradeOptions.Count() > 0 ) return;

    // Select 3 random upgrades from the pool
    List<Upgrade> upgrades = UpgradePool
      .OrderBy( x => Guid.NewGuid() )
      .Take( 3 )
      .ToList();

    UpgradeOptions.Clear();
    foreach ( Upgrade upgrade in upgrades )
    {
      UpgradeOptions.Add( upgrade );
    }

    UpgradeRound++;
    Ready = false;
  }

  [Rpc.Broadcast]
  public void SelectUpgrade( Upgrade upgrade )
  {
    // If this upgrade isn't in the options this player is trying to cheat
    if ( UpgradeOptions.FirstOrDefault( upgrade ) == null ) return;

    ActiveUpgrades.Add( upgrade );

    UpgradeOptions.Clear();

    if ( UpgradeRound < GameManager.CurrentRound )
    {
      SetUpgradeOptions();
    }
    else
    {
      Ready = true;
    }

    Scene.RunEvent<IEvents>( x => x.OnUpgrade( this, upgrade ) );
  }

  void GameManager.IEvents.OnRoundBreak()
  {
    SetUpgradeOptions();
  }
}
