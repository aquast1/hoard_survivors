using System;
using Sandbox;

public sealed class ViewModelController : Component, PlayerController.IEvents, WeaponController.IEvents
{
  [Property] public PlayerClient Client;
  [Property] public SkinnedModelRenderer WeaponModel { get; set; }
  [Property] public SkinnedModelRenderer ArmsModel { get; set; }

  private bool _isRunning = false;
  private bool _isReloading = false;
  private float _reloadSpeed = 1f;

  protected override void OnStart()
  {
    WeaponModel.Enabled = true;
    ArmsModel.Enabled = true;

    WeaponModel.Set( "b_twohanded", true );
    _reloadSpeed = Client.PlayerCharacter.WeaponController.ReloadSpeed;
    WeaponModel.Set( "speed_reload", Client.PlayerCharacter.WeaponController.RealReloadSpeed );
  }

  protected override void OnUpdate()
  {
    float moveBob = Client.PlayerCharacter.PlayerController.Velocity.Length / Client.PlayerCharacter.PlayerController.WalkSpeed;

    WeaponModel.Set( "move_bob", moveBob );

    // Check running
    if ( Client.PlayerCharacter.IsRunning && !_isRunning )
    {
      _isRunning = true;
      WeaponModel.Set( "b_sprint", true );
    }
    if ( !Client.PlayerCharacter.IsRunning && _isRunning )
    {
      _isRunning = false;
      WeaponModel.Set( "b_sprint", false );
    }

    // Check reloading
    if ( Client.PlayerCharacter.WeaponController.IsReloading && !_isReloading )
    {
      _isReloading = true;
      WeaponModel.Set( "b_empty", false );
      WeaponModel.Set( "b_reload", true );
    }

    // Check reload speed
    if ( _reloadSpeed != Client.PlayerCharacter.WeaponController.ReloadSpeed )
    {
      _reloadSpeed = Client.PlayerCharacter.WeaponController.ReloadSpeed;
      WeaponModel.Set( "speed_reload", Client.PlayerCharacter.WeaponController.RealReloadSpeed );
    }

    if ( !Client.PlayerCharacter.WeaponController.IsReloading && _isReloading )
    {
      _isReloading = false;
    }
  }

  void WeaponController.IEvents.OnShoot( WeaponController weaponController )
  {
    if ( weaponController != Client.PlayerCharacter.WeaponController ) return;

    WeaponModel.Set( "b_attack", true );

    if ( Client.PlayerCharacter.WeaponController.Ammo == 0 )
    {
      WeaponModel.Set( "b_empty", true );
    }
  }

  void PlayerController.IEvents.OnJumped()
  {
    WeaponModel.Set( "b_grounded", false );
    WeaponModel.Set( "b_jump", true );
  }

  void PlayerController.IEvents.OnLanded( float distance, Vector3 impactVelocity )
  {
    WeaponModel.Set( "b_grounded", true );
  }
}
