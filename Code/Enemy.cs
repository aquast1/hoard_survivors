using System;
using System.Data;
using System.Threading.Tasks;
using Sandbox;

public enum EnemyState
{
  Pursue,
  Attack,
  Dead
}

public sealed class Enemy : Component, HealthComponent.IEvents
{
  [Property] public ModelRenderer HeadModel { get; set; }
  [Property] public ModelRenderer BodyModel { get; set; }
  [Property] public NavMeshAgent Agent { get; set; }
  [Property] public float MeleeRange { get; set; } = 50f;
  [Property] public ParticleSphereEmitter Emitter;
  [Property] public HealthComponent HealthComponent;
  [Property] public SoundPointComponent HeadExplosionSound;

  private GameObject target;

  private EnemyState currentState = EnemyState.Pursue;

  protected override void OnUpdate()
  {
    if ( currentState == EnemyState.Dead )
    {
      // Color BodyModel
      Color currentTint = BodyModel.Tint;
      Color lerpedColor = Color.Lerp( currentTint, new Color( 0, 0, 0, 0 ), .01f );
      BodyModel.Tint = lerpedColor;
      return;
    }

    FindTarget();

    if ( currentState == EnemyState.Pursue )
    {
      Agent.MoveTo( target.WorldPosition );
    }

    if ( currentState == EnemyState.Attack )
    {
      Agent.Stop();
    }
  }

  private void FindTarget()
  {
    float minDistance = float.MaxValue;

    foreach ( var player in Scene.GetAllComponents<PlayerCharacter>() )
    {
      float distance = Vector3.DistanceBetween( WorldPosition, player.WorldPosition );

      // Set target to closest player
      if ( distance < minDistance )
      {
        minDistance = distance;
        target = player.GameObject;
      }
    }

    if ( minDistance < MeleeRange )
    {
      Attack();
    }
  }

  private async void Attack()
  {
    if ( currentState == EnemyState.Attack ) return;
    currentState = EnemyState.Attack;
    // ModelRenderer.Set( "b_attack", true );

    await Task.DelaySeconds( 1f );

    currentState = EnemyState.Pursue;
  }

  private async void Die( bool headshot )
  {
    if ( headshot )
    {
      //TODO: make some gore or special sound effect
      HeadModel.Destroy();
      Emitter.Enabled = true;
      HeadExplosionSound.StartSound();
    }
    currentState = EnemyState.Dead;
    Agent.Stop();
    await Task.DelaySeconds( 1f );
    GameObject.Destroy();
  }

  public void Hurt( float damage, Vector3 push, bool headshot )
  {
    push.z = 0;

    Agent.Velocity += push;

    HealthComponent.Damage( damage );

    if ( HealthComponent.Health <= 0 )
      Die( headshot );
  }
}
