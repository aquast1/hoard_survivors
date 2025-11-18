using Sandbox;

public enum TeamType
{
  [Icon("🧑")]
  [Description("Good guys")]
  Player,
  [Icon("💩")]
  [Description("Bad guys")]
  Snot
}

public sealed class UnitComponent : Component
{
  /// <summary>
  /// The name displayed for this unit
  /// </summary>
  [Property]
  [Category("Info")]
  public string Name { get; set; }

  /// <summary>
  /// Which team this will be in
  /// </summary>
  [Property]
  [Category("Info")]
  public TeamType Team {get; set;}

  [Property]
  [Category("Health")]
  [Range( 30f, 300f )]
  public float MaxHealth { get; set; } = 100f;

  [Property]
  [Category("Health")]
  [Range(0f, 30f)]
  public float HealthRegeneration { get; set; } = 5f;

  [Property]
  [Category( "Component" )]
  public SkinnedModelRenderer ModelRenderer { get; set; }

  public bool Alive = true;

  private float _health;
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

  private TimeUntil _nextRegen;
  private TimeUntil _fadeIn = 1f;
  private TimeUntil _fadeOut;

  protected override void OnStart()
  {
    _health = MaxHealth;

    ModelRenderer.Tint = ModelRenderer.Tint.WithAlpha( 0f );

    GameObject.BreakFromPrefab();
  }

	protected override void OnUpdate()
	{
    if ( !Alive ) return;

    if ( Team == TeamType.Snot )
    {
      var remappedHealth = MathX.Remap( Health, 0f, MaxHealth, 0f, 100f );
      var currentHealth = ModelRenderer.GetFloat( "health" );
      var lerpedHealth = MathX.Lerp( currentHealth, remappedHealth, Time.Delta * 2f );
      ModelRenderer.Set( "health", lerpedHealth );
    }

    // DebugOverlay.Text( WorldPosition + Vector3.Up * 80f, $"{Name} [{Health}/{MaxHealth}]" );
    // Gizmo.Draw.Text( $"{Name} [{Health}/{MaxHealth}]", WorldTransform );
	}

	protected override void OnFixedUpdate()
  {
    if ( Alive )
    {
      if ( _nextRegen )
      {
        Damage( -HealthRegeneration );
        _nextRegen = 1f;
      }
    }


    if ( !_fadeIn )
      ModelRenderer.Tint = ModelRenderer.Tint.WithAlpha( _fadeIn.Fraction );

    if ( !Alive )
      ModelRenderer.Tint = ModelRenderer.Tint.WithAlpha( 1f - _fadeOut.Fraction );
	}

  [Button]
  public void HurtDebug()
  {
    Damage( 10f );
  }
  
  [Button]
  public void HealDebug()
  {
    Damage(-10f);
  }

  public void Damage(float damage)
  {
    if ( !Alive ) return;

    Health -= damage;
    if (damage >= 0f)
      _nextRegen = 5f;
  }

  private void UpdateHealth( float newHealth )
  {
    var difference = newHealth - Health;

    _health = float.Clamp( newHealth, 0f, MaxHealth );
    
    if ( difference < 0f )
    {
      var remappedDamage = MathX.Remap( -difference, 0f, MaxHealth, 0f, 100f );
      DamageAnimation( remappedDamage );
    }

    if(Health <= 0f)
      Kill();
  }

  private async void DamageAnimation(float damage)
  {
    ModelRenderer.LocalScale *= 1.1f;
    ModelRenderer.Tint = Color.Red;

    await Task.DelaySeconds( damage / 100f );

    ModelRenderer.LocalScale /= 1.1f;
    ModelRenderer.Tint = Color.White;
  }

  private void DeathAnimation()
  {
    ModelRenderer.Set( "dead", true );
    _fadeOut = 1f;
  }
  
  public async void Kill()
  {
    Alive = false;
    DeathAnimation();

    var playerComponent = GetComponent<PlayerComponent>();

    if ( playerComponent.IsValid() )
      playerComponent.Ragdoll();

    await Task.DelaySeconds( 2f );

    if ( playerComponent.IsValid() )
      playerComponent.Respawn();
    else
      GameObject.Destroy();
  }
}
