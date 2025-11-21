using Sandbox;

public sealed class WeaponController : Component, NetworkPlayer.IEvents
{
  public interface IEvents
  {
    void OnShoot( WeaponController weaponController );
  }

  [Property] public PlayerCharacter Player { get; set; }

  [Property] public SoundPointComponent PistolFireSound { get; set; }

  [Property] public SoundPointComponent PistolReloadSound { get; set; }

  [Property] public float WeaponDamage { get; set; } = 50f;

  [Property] public float FireCooldown { get; set; } = .2f;

  [Property]
  [Sync( SyncFlags.FromHost )]
  public int MaxAmmo { get; set; } = 6;

  [Property]
  [Sync( SyncFlags.FromHost )]
  public int Ammo { get; set; }

  [Property]
  [ReadOnly]
  public bool IsReloading = false;

  private float _reloadSpeed;
  public float ReloadSpeed
  {
    get
    {
      return _reloadSpeed;
    }
    set
    {
      _reloadSpeed = value;

      if ( !IsProxy )
      {
        Player.PlayerModel.Set( "speed_reload", value );
      }
    }
  }

  private TimeUntil _nextShot;

  protected override void OnStart()
  {
    Ammo = MaxAmmo;
    ReloadSpeed = .6f;
  }

  [Rpc.Broadcast]
  public void Fire()
  {
    if ( !_nextShot || Player.IsRunning || IsReloading ) return;

    if ( Ammo <= 0 )
    {
      Reload();
      return;
    }

    _nextShot = FireCooldown;
    PistolFireSound.StartSound();

    Ammo--;

    Player.PlayerModel.Set( "b_attack", true );

    Scene.RunEvent<IEvents>( x => x.OnShoot( this ) );

    HandleTrace();
  }

  private void HandleTrace()
  {
    var shootDirection = Player.PlayerController.EyeAngles.Forward;
    var shotStart = Player.PlayerController.EyePosition;
    var shotEnd = shotStart + shootDirection * 10000f;

    var shotTrace = Scene.Trace.Ray( shotStart, shotEnd )
      .Radius( 1f )
      .IgnoreGameObjectHierarchy( GameObject )
      .WithoutTags( ["player"] )
      .Run();

    if ( !shotTrace.Hit ) return;

    if ( shotTrace.GameObject.Components.TryGet<HealthComponent>( out var enemy ) )
    {
      enemy.Damage( WeaponDamage );
      return;
    }
  }

  [Rpc.Broadcast]
  public async void Reload()
  {
    if ( IsReloading || Ammo == MaxAmmo ) return;

    IsReloading = true;
    Player.PlayerModel.Set( "b_reload", true );

    PistolReloadSound.StartSound();

    await Task.DelaySeconds( 1.4f / ReloadSpeed );

    Ammo = MaxAmmo;

    IsReloading = false;
  }

  // void NetworkPlayer.IEvents.OnUpgrade( GameObject gameObject, Upgrade upgrade )
  // {
  //   if ( gameObject != Player.GameObject ) return;

  //   if ( upgrade.ID == "damage" )
  //   {
  //     WeaponDamage += 5;
  //   }

  //   if ( upgrade.ID == "reload_speed" )
  //   {
  //     ReloadSpeed += .4f;
  //   }
  // }
}
