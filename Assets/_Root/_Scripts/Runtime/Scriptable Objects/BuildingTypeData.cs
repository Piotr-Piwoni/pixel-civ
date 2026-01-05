using System.Collections.Generic;
using PixelCiv.Utilities.Types;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace PixelCiv.Scriptable_Objects
{
[HideMonoScript, CreateAssetMenu(fileName = "BuildingTypeData",
								 menuName = "Game/BuildingTypeData",
								 order = 0),]
public class BuildingTypeData : SerializedScriptableObject
{
	public BuildingCategory Category => _Category;
	public BuildingRestriction[] Restrictions => _Restrictions.ToArray();
	public BuildingType Type => _Type;
	public BuildingTypeData PreviousTier => _PreviousTier;
	public BuildingTypeData NextTier => _NextTier;
	public Dictionary<ResourceType, float> Production => _Production;
	public int AttackPower => _AttackPower;
	public int Defence => _Defence;
	public int Health => _Health;
	public string Name => _Name;
	public TileBase Visual => _Visual;

	[SerializeField]
	private string _Name = "Building";
	[SerializeField]
	private TileBase _Visual;
	[SerializeField]
	private BuildingType _Type = BuildingType.Normal;
	[SerializeField]
	private BuildingCategory _Category = BuildingCategory.General;
	[SerializeField]
	private Dictionary<ResourceType, float> _Production = new();
	[SerializeField, Min(0),]
	private int _Health = 10;
	[SerializeField, Min(0),]
	private int _Defence = 5;
	[SerializeField, Min(0),]
	private int _AttackPower = 1;
	[SerializeField]
	private BuildingTypeData _NextTier;
	[SerializeField]
	private BuildingTypeData _PreviousTier;
	[SerializeField]
	private readonly List<BuildingRestriction> _Restrictions = new()
	{
			BuildingRestriction.InTerritory,
	};
}
}
