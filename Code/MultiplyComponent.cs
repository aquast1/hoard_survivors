using System.Dynamic;
using Sandbox;

public sealed class MultiplyComponent : Component
{
  [Property]
  public RangedFloat Cooldown { get; set; } = new RangedFloat( 2f, 3f );

  [Property]
  public PrefabScene PrefabToClone { get; set; }

  private TimeUntil _nextClone;

		protected override void OnStart()
  {
    ResetTimer();

    var foundObjects = Scene.FindInPhysics( new Sphere( WorldPosition, 100f ) );

    foreach( var gameObject in foundObjects )
    {
      if ( gameObject.Components.TryGet<PlayerComponent>( out var player ) )
      {
        player.HealthComponent.Damage( 50f );
        DestroyGameObject();
      }
    }
	}

  protected override void OnUpdate()
  {
    if ( _nextClone )
    {
      Multiply();
      ResetTimer();
    }
  }

  private void ResetTimer()
  {
    _nextClone = Cooldown.GetValue();
  }
  
  public void Multiply()
  {
    var randomDirection = (Vector3)Game.Random.VectorInCircle().Normal;
    var startPos = WorldPosition + Vector3.Up * 20f;
    var endPos = startPos + randomDirection * 100f;
    var traceCheck = Scene.Trace.Ray( startPos, endPos )
      .Radius( 10f )
      .WithoutTags( "player" )
      .IgnoreGameObjectHierarchy( GameObject )
      .Run();
    
    if ( !traceCheck.Hit )
    {
      var spawnPos = traceCheck.EndPosition + Vector3.Down * 20f;
      PrefabToClone.Clone( spawnPos );
    }
  }
}
