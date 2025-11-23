using System;
using System.Diagnostics;
using Sandbox;

public sealed class HealthComponent : Component
{
  public interface IEvents
  {
    void OnKilled( GameObject gameObject, bool headshot = false ) { }
    void OnHurt( GameObject gameObject, float damage, bool headshot = false ) { }
  }

  [Property]
  public float MaxHealth { get; set; } = 100f;

  public bool Alive { get; set; } = true;

  private float _health;
  [Sync]
  public float Health
  {
    get
    {
      return _health;
    }

    set
    {
      UpdateHealth( value );
    }
  }

  protected override void OnStart()
  {
    Health = MaxHealth;
  }

  protected override void OnUpdate()
  {
    Gizmo.Draw.Text( $"[{Health}/{MaxHealth}]", WorldTransform, "Roboto", 30 );
  }

  public void Damage( float damage, bool headshot = false )
  {
    Health -= (float)Math.Round( damage );

    if ( Health <= 0 )
      Kill( headshot );

    Scene.RunEvent<IEvents>( x => x.OnHurt( GameObject, damage, headshot ) );
  }

  private void UpdateHealth( float newHealth )
  {
    _health = float.Clamp( newHealth, 0f, MaxHealth );
  }

  public void Respawn()
  {
    Alive = true;
    _health = MaxHealth;
  }

  public void Kill( bool headshot = false )
  {
    Scene.RunEvent<IEvents>( x => x.OnKilled( GameObject, headshot ) );
    Alive = false;
  }

  [Button]
  public void TestDamage()
  {
    Damage( 50 );
  }

  [Button]
  public void TestHeal()
  {
    Damage( -50 );
  }
}
