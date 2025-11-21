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

  public NetworkPlayer NetworkPlayer;

  private ModelPhysics _ragdoll;

  protected override void OnStart()
  {
    if ( !IsProxy )
      Scene.GetComponentInChildren<PlayerClient>().PlayerCharacter = this;

    PlayerController.UseInputControls = true;

    PlayerModel.Set( "holdtype", 1 );

    Dresser.Apply();
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

  void NetworkPlayer.IEvents.OnUpgrade( NetworkPlayer networkPlayer, Upgrade upgrade )
  {
    if ( networkPlayer != NetworkPlayer ) return;

    if ( upgrade.ID == "movement_speed" )
    {
      PlayerController.WalkSpeed += 100;
      PlayerController.RunSpeed += 100;
    }
  }

  void HealthComponent.IEvents.OnKilled( GameObject gameObject )
  {
    var character = gameObject.GetComponent<PlayerCharacter>();

    if ( !character.IsValid() ) return;

    if ( character != this ) return;

    Die();
  }
}
