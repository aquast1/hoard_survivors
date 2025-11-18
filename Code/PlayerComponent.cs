using System;
using Sandbox;

public sealed class PlayerComponent : Component, HealthComponent.IEvents
{
  public interface IEvents
  {
    void OnUpgrade( GameObject gameObject, Upgrade upgrade ) { }
  }

  [Property] public PlayerController PlayerController { get; set; }

  [Property] public WeaponController WeaponController { get; set; }

  [Property] public ViewModelController ViewModel { get; set; }

  [Property] public SkinnedModelRenderer PlayerModel { get; set; }

  [Property] public HealthComponent HealthComponent { get; set; }

  [Property] public List<Upgrade> ActiveUpgrades { get; set; } = new();

  [Property] public List<Upgrade> UpgradePool { get; set; }

  public List<Upgrade> UpgradeOptions { get; set; }

  public Guid PlayerId;

  [Sync] public bool IsRunning { get; set; }

  private ModelPhysics _ragdoll;

  private Vector3 _spawnPosition;

  protected override void OnStart()
  {
    PlayerId = Network.OwnerId;

    _spawnPosition = WorldPosition;

    PlayerController.UseInputControls = true;

    UpgradeOptions = UpgradePool.Take( 3 ).ToList();
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

    if ( ViewModel.IsValid() )
      ViewModel.Hide();

    await Task.DelaySeconds( 3f );

    Respawn();
  }

  public void Respawn()
  {
    UnRagdoll();

    PlayerController.ThirdPerson = false;

    HealthComponent.Respawn();

    WorldPosition = _spawnPosition;

    if ( ViewModel.IsValid() )
      ViewModel.Show();
  }

  [Rpc.Broadcast]
  public void AddUpgrade( Upgrade upgrade )
  {
    Scene.RunEvent<IEvents>( x => x.OnUpgrade( GameObject, upgrade ) );

    if ( upgrade.ID == "movement_speed" )
    {
      PlayerController.WalkSpeed += 20;
      PlayerController.RunSpeed += 20;
    }

    ActiveUpgrades.Add( upgrade );
  }

  void HealthComponent.IEvents.OnKilled( GameObject gameObject )
  {
    var playerComponent = gameObject.GetComponent<PlayerComponent>();

    if ( !playerComponent.IsValid() ) return;

    if ( playerComponent.PlayerId != PlayerId ) return;

    Die();
  }
}
