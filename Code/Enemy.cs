using System.Data;
using System.Threading.Tasks;
using Sandbox;

public enum EnemyState
{
  Pursue,
  Attack
}

public sealed class Enemy : Component, HealthComponent.IEvents
{
  [Property] public SkinnedModelRenderer ModelRenderer { get; set; }
  [Property] public NavMeshAgent Agent { get; set; }
  [Property] public float MeleeRange { get; set; } = 50f;
  [Property] public ModelRenderer HeadModel;
  [Property] public ParticleSphereEmitter Emitter;

  private GameObject target;

  private EnemyState currentState = EnemyState.Pursue;

  protected override void OnUpdate()
  {
    FindTarget();

    if ( currentState == EnemyState.Pursue )
    {
      Agent.MoveTo( target.WorldPosition );
    }

    if ( currentState == EnemyState.Attack )
    {
      Agent.Stop();
    }

    // ModelRenderer.Set( "speed", Agent.Velocity.Length );
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

    await Task.DelaySeconds( 3f );

    currentState = EnemyState.Pursue;
  }

  private async void Die( bool headshot )
  {
    if ( headshot )
    {
      //TODO: make some gore or special sound effect
      HeadModel.Destroy();
      Emitter.Enabled = true;
    }
    await Task.DelaySeconds( 1f );
    GameObject.Destroy();
  }

  void HealthComponent.IEvents.OnHurt( GameObject gameObject, float damage, bool headshot )
  {
    if ( gameObject != GameObject ) return;

    // ModelRenderer.Set( "hurt", true );
  }

  void HealthComponent.IEvents.OnKilled( GameObject gameObject, bool headshot )
  {
    Die( headshot );
  }
}
