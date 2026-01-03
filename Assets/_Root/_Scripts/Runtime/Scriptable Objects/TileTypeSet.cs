using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace PixelCiv.Scriptable_Objects
{
[CreateAssetMenu(fileName = "TileTypeSet", menuName = "Game/TileTypeSet", order = 0),
 HideMonoScript,]
public class TileTypeSet : SerializedScriptableObject
{
	public Dictionary<TileType, Color> Set => _Set;
	public TileBase Base => _Base;

	[SerializeField]
	private Dictionary<TileType, Color> _Set = new()
	{
			[TileType.Grassland] = Color.softGreen,
			[TileType.Sea] = Color.darkBlue,
			[TileType.Mountain] = Color.gray3,
	};
	[SerializeField]
	private TileBase _Base;
}
}
