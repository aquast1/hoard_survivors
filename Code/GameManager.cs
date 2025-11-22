using Sandbox;
using System;

public sealed class GameManager : Component, Component.INetworkListener, ISceneStartup, NetworkPlayer.IEvents
{
  public interface IEvents
  {
    void OnRoundBreak() { }
  }

  [Property] public GameObject PlayerClientPrefab { get; set; }
  [Property] public GameObject PlayerCharacterPrefab { get; set; }
  [Property] public List<GameObject> SpawnPoints { get; set; }
  [Property] public GameObject SpectatorCameraSpawnPoint { get; set; }
  [Property] public NetworkManager NetworkManager { get; set; }
  [Property] public EnemyManager EnemyManager { get; set; }

  /// <summary>
  /// A list of spawned players (PlayerCharacter)
  /// </summary>
  [Property]
  [ReadOnly]
  [Sync]
  public NetList<PlayerCharacter> PlayerCharacters { get; set; } = new();

  public int RoundBreakTime = 120;

  [Property]
  [ReadOnly]
  [Sync]
  public int CurrentRound { get; set; } = 0;

  /// <summary>
  /// Timer counting down to next round start
  /// </summary>
  [Sync] public TimeUntil NextRound { get; set; }

  private bool _roundBreak;
  [Property]
  [ReadOnly]
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
    StartRoundBreak();
  }

  protected override void OnUpdate()
  {
    if ( NextRound <= 0 && RoundBreak ) StartRound();
  }

  [Rpc.Broadcast( NetFlags.HostOnly )]
  public void StartRoundBreak()
  {
    CurrentRound++;

    RoundBreak = true;

    Scene.RunEvent<IEvents>( x => x.OnRoundBreak() );
  }

  [Rpc.Broadcast( NetFlags.HostOnly )]
  private void StartRound()
  {
    RoundBreak = false;
    EnemyManager.EnemiesKilled = 0;
    EnemyManager.EnemiesSpawned = 0;
  }

  public void OnDisconnected( Connection connection )
  {
    foreach ( var player in PlayerCharacters )
    {
      if ( player.Network.Owner != connection ) continue;

      PlayerCharacters.Remove( player );
      break;
    }
  }

  public void SpawnPlayerCharacter( NetworkPlayer networkPlayer )
  {
    if ( IsProxy ) return;

    Connection connection = Connection.All.FirstOrDefault( c => c.Id == networkPlayer.ConnectionId );

    var startLocation = FindSpawnLocation().WithScale( 1 );

    // Spawn this object and make the client the owner
    var player = PlayerCharacterPrefab.Clone( startLocation, name: $"PlayerCharacter - {connection.DisplayName}" );

    Log.Info( player.Enabled );

    player.NetworkSpawn( connection );

    PlayerCharacter playerCharacter = player.GetComponent<PlayerCharacter>();

    PlayerCharacters.Add( playerCharacter );

    playerCharacter.NetworkPlayer = networkPlayer;

    // foreach ( Upgrade upgrade in networkPlayer.ActiveUpgrades )
    // {
    //   playerCharacter.HandleUpgrade( upgrade );
    // }

    // networkPlayer.PlayerCharacter = playerCharacter;
  }

  private Transform FindSpawnLocation()
  {
    // If they have spawn point set then use those
    if ( SpawnPoints is not null && SpawnPoints.Count > 0 )
    {
      return Random.Shared.FromList( SpawnPoints, default ).WorldTransform;
    }

    // If we have any SpawnPoint components in the scene, then use those
    var spawnPoints = Scene.GetAllComponents<SpawnPoint>().ToArray();
    if ( spawnPoints.Length > 0 )
    {
      return Random.Shared.FromArray( spawnPoints ).WorldTransform;
    }

    // Failing that, spawn where we are
    return WorldTransform;
  }

  // This is only called client side, spawning player client stuff
  void ISceneStartup.OnClientInitialize()
  {
    PlayerClientPrefab.Clone( SpectatorCameraSpawnPoint.WorldTransform );
  }

  void NetworkPlayer.IEvents.OnUpgrade( NetworkPlayer networkPlayer, Upgrade upgrade )
  {
    // Check if all players are ready
    if ( NetworkManager.NetworkPlayers.FirstOrDefault( p => !p.Ready ) == null && RoundBreak ) NextRound = 5;
  }
}
