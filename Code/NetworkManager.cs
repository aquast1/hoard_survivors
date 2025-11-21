using System;
using System.Threading.Tasks;
namespace Sandbox;


public sealed class NetworkManager : Component, Component.INetworkListener
{
  [Property] public GameObject NetworkPlayerPrefab { get; set; }

  /// <summary>
  /// A list of connected players (PlayerCharacter)
  /// </summary>
  [Property]
  [ReadOnly]
  [Sync]
  public NetList<NetworkPlayer> NetworkPlayers { get; set; } = new();

  /// <summary>
  /// Create a server (if we're not joining one)
  /// </summary>
  [Property] public bool StartServer { get; set; } = true;

  protected override async Task OnLoad()
  {
    if ( Scene.IsEditor )
      return;

    if ( StartServer && !Networking.IsActive )
    {
      LoadingScreen.Title = "Creating Lobby";
      await Task.DelayRealtimeSeconds( 0.1f );
      Networking.CreateLobby( new() );
    }
  }

  // Create NetworkPlayer instance for new connection
  public void OnActive( Connection connection )
  {
    GameObject networkPlayerObject = NetworkPlayerPrefab.Clone( WorldTransform, name: $"NetworkPlayer - {connection.DisplayName}" );

    NetworkPlayer networkPlayer = networkPlayerObject.GetComponent<NetworkPlayer>();
    // networkPlayer.Connection = connection;
    networkPlayer.ConnectionId = connection.Id;

    NetworkPlayers.Add( networkPlayer );

    networkPlayerObject.NetworkSpawn( Connection.Host );

    Log.Info( $"Player '{connection.DisplayName}' has joined the game" );
  }

  public void OnDisconnected( Connection connection )
  {
    NetworkPlayer networkPlayer = NetworkPlayers.FirstOrDefault( p => p.ConnectionId == connection.Id );

    if ( networkPlayer == null ) return;

    NetworkPlayers.Remove( networkPlayer );

    networkPlayer.GameObject.Destroy();
  }
}
