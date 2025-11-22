using System;
using Sandbox;

public sealed class PlayerCharacter : Component, HealthComponent.IEvents, NetworkPlayer.IEvents
{
  [Property] public PlayerController PlayerController { get; set; }
  [Property] public Dresser Dresser;
  [Property] public WeaponController WeaponController { get; set; }
  [Property] public SkinnedModelRenderer PlayerModel { get; set; }
  [Property] public HealthComponent HealthComponent { get; set; }

  [Sync] public bool IsRunning { get; set; }

  [Property]
  [ReadOnly]
  public NetworkPlayer NetworkPlayer;

  private ModelPhysics _ragdoll;

  private int _movementSpeed;
  public int MovementSpeed
  {
    get => _movementSpeed;
    set
    {
      _movementSpeed = value;
      PlayerController.WalkSpeed = (_movementSpeed * 50) + 100;
      PlayerController.RunSpeed = (_movementSpeed * 75) + 200;
    }
  }

  protected override void OnStart()
  {
    if ( !IsProxy )
      Scene.GetComponentInChildren<PlayerClient>().PlayerCharacter = this;

    PlayerController.UseInputControls = true;

    PlayerModel.Set( "holdtype", 1 );

    Dresser.Apply();

    MovementSpeed = 1;
  }

  public void Ragdoll()
  {
    if ( _ragdoll.IsValid() ) return;

    _ragdoll = AddComponent<ModelPhysics>();
    _ragdoll.Renderer = PlayerModel;
    _ragdoll.Model = PlayerModel.Model;

    PlayerController.UseInputControls = false;
  }

  public void UnRagdoll()
  {
    if ( !_ragdoll.IsValid() ) return;

    _ragdoll.Destroy();

    PlayerController.UseInputControls = true;
  }

  public async void Die()
  {
    Ragdoll();

    PlayerController.ThirdPerson = true;

    PlayerController.WishVelocity = 0;

    await Task.DelaySeconds( 3f );

    Respawn();
  }

  public void Respawn()
  {
    UnRagdoll();

    PlayerController.ThirdPerson = false;

    HealthComponent.Respawn();
  }

  public void HandleUpgrade( Upgrade upgrade )
  {
    if ( upgrade.Type == Upgrade.UpgradeType.MovementSpeed )
    {
      MovementSpeed += 1;
    }

  }

  void NetworkPlayer.IEvents.OnUpgrade( NetworkPlayer networkPlayer, Upgrade upgrade )
  {
    if ( networkPlayer != NetworkPlayer ) return;

    HandleUpgrade( upgrade );
  }

  void HealthComponent.IEvents.OnKilled( GameObject gameObject )
  {
    var character = gameObject.GetComponent<PlayerCharacter>();

    if ( !character.IsValid() ) return;

    if ( character != this ) return;

    Die();
  }
}
