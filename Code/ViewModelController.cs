using System;
using Sandbox;

public sealed class ViewModelController : Component, PlayerController.IEvents, WeaponController.IEvents
{
  [Property] public PlayerComponent Player;

  [Property] public SkinnedModelRenderer WeaponModel { get; set; }

  [Property] public SkinnedModelRenderer ArmsModel { get; set; }

  [Property] public ScreenPanel UI { get; set; }

  private bool _isRunning = false;
  private bool _isReloading = false;
  private float _reloadSpeed = 1f;

  protected override void OnStart()
  {
    WeaponModel.Set( "b_twohanded", true );
    _reloadSpeed = Player.WeaponController.ReloadSpeed;
    WeaponModel.Set( "speed_reload", _reloadSpeed );
  }

  protected override void OnUpdate()
  {
    WorldTransform = Scene.Camera.WorldTransform;
    WeaponModel.Set( "move_bob", Player.PlayerController.Velocity.Length );

    // Check running
    if ( Player.IsRunning && !_isRunning )
    {
      _isRunning = true;
      WeaponModel.Set( "b_sprint", true );
    }
    if ( !Player.IsRunning && _isRunning )
    {
      _isRunning = false;
      WeaponModel.Set( "b_sprint", false );
    }

    // Check reloading
    if ( Player.WeaponController.IsReloading && !_isReloading )
    {
      _isReloading = true;
      WeaponModel.Set( "b_empty", false );
      WeaponModel.Set( "b_reload", true );
    }

    // Check reload speed
    if ( _reloadSpeed != Player.WeaponController.ReloadSpeed )
    {
      _reloadSpeed = Player.WeaponController.ReloadSpeed;
      WeaponModel.Set( "speed_reload", _reloadSpeed );
    }

    if ( !Player.WeaponController.IsReloading && _isReloading )
    {
      _isReloading = false;
    }
  }

  public void Hide()
  {
    WeaponModel.Tint = "#FFFFFF00";
    ArmsModel.Tint = "#FFFFFF00";
    UI.Opacity = 0;
  }

  public void Show()
  {
    WeaponModel.Tint = "#FFFFFF";
    ArmsModel.Tint = "#FFFFFF";
    UI.Opacity = 1;
  }

  void WeaponController.IEvents.OnShoot( WeaponController weaponController )
  {
    if ( weaponController != Player.WeaponController ) return;

    WeaponModel.Set( "b_attack", true );

    if ( Player.WeaponController.Ammo == 0 )
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
