[AssetType( Name = "Upgrade Type", Extension = "upgrade" )]
public partial class Upgrade : GameResource
{
  public enum UpgradeType
  {
    WeaponDamage,
    MovementSpeed,
    ReloadSpeed
  }

  public UpgradeType Type { get; set; }

  public string Title { get; set; }

  public string Icon { get; set; }

  protected override Bitmap CreateAssetTypeIcon( int width, int height )
  {
    return CreateSimpleAssetTypeIcon( "upgrade", width, height, "#60fd60ff", "black" );
  }
}