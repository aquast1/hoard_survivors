using Sandbox;
using System;

public sealed class NetworkPlayer : Component, GameManager.IEvents
{
  public interface IEvents
  {
    void OnUpgrade( NetworkPlayer networkPlayer, Upgrade upgrade ) { }
  }

  public GameManager GameManager;
  public PlayerCharacter PlayerCharacter;

  /// <summary>
  /// Connection ID of the connection that's represented by this NetworkPlayer
  /// This is tracked because the host is the owner of this object
  /// </summary>
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
  [Group( "Upgrades" )]
  [Property]
  [Sync]
  public NetList<Upgrade> UpgradePool { get; set; }

  /// <summary>
  /// Upgrades this player has chosen
  /// </summary>
  [Group( "Upgrades" )]
  [Property]
  [ReadOnly]
  [Sync]
  public NetList<Upgrade> ActiveUpgrades { get; set; } = new();

  /// <summary>
  /// Upgrades currently available for this upgrade round
  /// </summary>
  [Group( "Upgrades" )]
  [Property]
  [ReadOnly]
  [Sync]
  public NetList<Upgrade> UpgradeOptions { get; set; } = new();

  [Group( "Stats" )]
  [Property]
  [ReadOnly]
  [Sync]
  public int StatWeaponDamage { get; set; } = 1;

  [Group( "Stats" )]
  [Property]
  [ReadOnly]
  [Sync]
  public int StatReloadSpeed { get; set; } = 1;

  [Group( "Stats" )]
  [Property]
  [ReadOnly]
  [Sync]
  public int StatMovementSpeed { get; set; } = 1;

  /// <summary>
  /// Set to true once all upgrades have been selected
  /// GameManager checks if all players are ready before auto-starting round
  /// </summary>
  public bool Ready = false;

  /// <summary>
  /// Current round that the player is selecting an upgrade for
  /// </summary>
  public int UpgradeRound = 1;

  protected override void OnStart()
  {
    GameManager = Scene.GetComponentInChildren<GameManager>();

    SetUpgradeOptions();

    if ( GameManager.RoundBreak || GameManager.NextRound > 5 )
      GameManager.SpawnPlayerCharacter( this );
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
    Ready = false;
  }

  [Rpc.Broadcast]
  public void SelectUpgrade( Upgrade upgrade )
  {
    // If this upgrade isn't in the options this player is trying to cheat
    if ( UpgradeOptions.FirstOrDefault( upgrade ) == null ) return;

    UpgradeRound++;

    UpgradeOptions.Clear();

    HandleUpgrade( upgrade );

    ActiveUpgrades.Add( upgrade );

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

  private void HandleUpgrade( Upgrade upgrade )
  {
    if ( upgrade.Type == Upgrade.UpgradeType.WeaponDamage )
    {
      StatWeaponDamage += 1;
    }
    if ( upgrade.Type == Upgrade.UpgradeType.ReloadSpeed )
    {
      StatReloadSpeed += 1;
    }
    if ( upgrade.Type == Upgrade.UpgradeType.MovementSpeed )
    {
      StatMovementSpeed += 1;
    }
  }

  void GameManager.IEvents.OnRoundBreak()
  {
    SetUpgradeOptions();

    if ( !PlayerCharacter.IsValid() )
      GameManager.SpawnPlayerCharacter( this );
  }
}
