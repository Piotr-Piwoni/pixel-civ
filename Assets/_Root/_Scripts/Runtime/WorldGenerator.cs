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
	[SerializeField, MinValue(0f),]
	private float _NoiseScale = 2f;
	[SerializeField, Range(0f, 1f),]
	private float _ThreshHold = 0.5f;


	private void Start()
	{
		GenerateWorld();
	}

	[Button]
	private void Clear()
	{
		_GroundTilemap?.ClearAllTiles();
	}

	[Button("Generate")]
	private void GenerateWorld()
	{
		Random.InitState(Time.frameCount);
		_GroundTilemap.ClearAllTiles();

		BoundsInt bounds = new(0, 0, 0, _WorldSize.x, _WorldSize.y, 1);
		var tiles = new TileBase[_WorldSize.x * _WorldSize.y];

		for (var y = 0; y < _WorldSize.y; y++)
		for (var x = 0; x < _WorldSize.x; x++)
		{
			float noise = Mathf.PerlinNoise(x * _NoiseScale, y * _NoiseScale);

			int index = y * _WorldSize.x + x;
			tiles[index] = noise > _ThreshHold ? _GroundTile : null;
		}

		_GroundTilemap.SetTilesBlock(bounds, tiles);
	}
}
}
