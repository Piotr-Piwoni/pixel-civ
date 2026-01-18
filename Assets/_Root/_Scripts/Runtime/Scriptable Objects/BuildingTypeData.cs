using System.Collections.Generic;
using System.Runtime.CompilerServices;
using PixelCiv.Utilities.Types;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

[assembly: InternalsVisibleTo("Project.Editor")]

namespace PixelCiv.Scriptable_Objects
{
[DisableInInlineEditors, HideMonoScript,
 CreateAssetMenu(fileName = "BuildingTypeData", menuName = "Game/BuildingTypeData",
				 order = 0),]
public class BuildingTypeData : SerializedScriptableObject
{
	public BuildingCategory Category => _Category;
	public BuildingRestriction[] Restrictions => _Restrictions.ToArray();
	public BuildingType Type => _Type;
	public BuildingTypeData NextTier => _NextTier;
	public BuildingTypeData PreviousTier => _PreviousTier;
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


	#if UNITY_EDITOR
	internal void Init(BuildingTypeDataParams data)
	{
		_Name = data.Name;
		_Visual = data.Visual;
		_Type = data.Type;
		_Category = data.Category;
		_Health = data.Health;
		_Defence = data.Defence;
		_AttackPower = data.AttackPower;
		_PreviousTier = data.PreviousTier;
		_NextTier = data.NextTier;

		_Production.Clear();
		foreach (KeyValuePair<ResourceType, float> kvp in data.Production)
			_Production[kvp.Key] = kvp.Value;

		_Restrictions.Clear();
		_Restrictions.AddRange(data.Restrictions);
	}
	#endif
}

#if UNITY_EDITOR
public sealed class BuildingTypeDataParams
{
	public int AttackPower;
	public BuildingCategory Category;
	public int Defence;
	public int Health;
	public string Name;
	public BuildingTypeData NextTier;
	public BuildingTypeData PreviousTier;
	public Dictionary<ResourceType, float> Production = new();
	public List<BuildingRestriction> Restrictions = new();
	public BuildingType Type;
	public TileBase Visual;
}
#endif
}
