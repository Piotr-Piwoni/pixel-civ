namespace PixelCiv.Utilities.Types
{
public enum BuildingType
{
	Normal,
	Special,
}

public enum BuildingCategory
{
	General,
	Military,
	Economy,
	Faith,
	Research,
	Unique,
}

public enum BuildingRestriction
{
	InTerritory,
	NeedPreviousTier,
	OnlySea,
	OnlyGrassland,
	OnlyMountain,
}
}
