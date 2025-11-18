using Sandbox;

public sealed class ClientController : Component
{
  [Property] public PlayerComponent Player { get; set; }

  protected override void OnStart()
  {
    if ( IsProxy ) GameObject.Destroy();
  }

  protected override void OnUpdate()
  {
    if ( IsProxy ) GameObject.Destroy();

    HandleInput();
  }

  private void HandleInput()
  {
    if ( !Player.HealthComponent.Alive ) return;

    if ( Input.Pressed( "attack1" ) )
    {
      Player.WeaponController.Fire();
    }

    if ( Input.Pressed( "reload" ) )
    {
      Player.WeaponController.Reload();
    }

    bool isMoving = Input.Down( "forward" ) || Input.Down( "backward" ) || Input.Down( "left" ) || Input.Down( "right" );

    if ( Input.Down( "run" ) && isMoving )
    {
      Player.IsRunning = true;
    }
    else
    {
      Player.IsRunning = false;
    }
  }
}
