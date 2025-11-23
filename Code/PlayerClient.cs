using Sandbox;

[Description( "This component will only ever exist on the client side, all client side components should be attached to the game object this component is assigned to" )]
public sealed class PlayerClient : Component
{
  [Property] public ViewModelController ViewModelController;
  [Property] public Crosshair Crosshair;

  [Property]
  [ReadOnly]
  public NetworkPlayer NetworkPlayer;

  private PlayerCharacter _playerCharacter;
  [Property]
  [ReadOnly]
  public PlayerCharacter PlayerCharacter
  {
    get => _playerCharacter;
    set
    {
      _playerCharacter = value;

      // Show/hide viewmodel and crosshair when player spawns/despawns
      if ( _playerCharacter.IsValid() )
      {
        ViewModelController.Enabled = true;
        Crosshair.Enabled = true;
      }
      // else
      // {
      //   ViewModelController.Enabled = false;
      //   Crosshair.Enabled = false;
      // }
    }
  }

  protected override void OnStart()
  {
    Log.Info( "valid " + NetworkPlayer.IsValid() );
  }

  protected override void OnUpdate()
  {
    HandleInput();
    HandleCharacterInput();
  }

  private void HandleInput()
  {

  }

  private void HandleCharacterInput()
  {
    if ( !PlayerCharacter.IsValid() ) return;

    if ( !PlayerCharacter.HealthComponent.Alive ) return;

    if ( Input.Pressed( "attack1" ) )
    {
      PlayerCharacter.WeaponController.Fire();
    }

    if ( Input.Pressed( "reload" ) )
    {
      PlayerCharacter.WeaponController.Reload();
    }

    bool isMoving = Input.Down( "forward" ) || Input.Down( "backward" ) || Input.Down( "left" ) || Input.Down( "right" );

    if ( Input.Down( "run" ) && isMoving )
    {
      PlayerCharacter.IsRunning = true;
    }
    else
    {
      PlayerCharacter.IsRunning = false;
    }
  }
}
