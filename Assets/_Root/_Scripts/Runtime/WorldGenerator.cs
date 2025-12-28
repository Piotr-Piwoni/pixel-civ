using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace PixelCiv
{
public class WorldGenerator : MonoBehaviour
{
	[SerializeField, MinValue(1),]
	private Vector2Int _WorldSize = new(10, 15);
	[SerializeField]
	private Tilemap _GroundTilemap;
	[SerializeField]
	private TileBase _GroundTile;


	private void Start()
	{
		GenerateWorld();
	}

	private void GenerateWorld()
	{
		_GroundTilemap.ClearAllTiles();

		BoundsInt bounds = new(0, 0, 0, _WorldSize.x, _WorldSize.y, 1);
		var tiles = new TileBase[_WorldSize.x * _WorldSize.y];

		for (var y = 0; y < _WorldSize.y; y++)
		for (var x = 0; x < _WorldSize.x; x++)
		{
			int index = y * _WorldSize.x + x;
			tiles[index] = _GroundTile;
		}

		_GroundTilemap.SetTilesBlock(bounds, tiles);
	}
}
}
