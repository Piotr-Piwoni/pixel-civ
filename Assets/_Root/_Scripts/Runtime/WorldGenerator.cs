using System;
using System.Linq;
using PixelCiv.Managers;
using PixelCiv.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace PixelCiv
{
public class WorldGenerator : MonoBehaviour
{
	[SerializeField, MinValue(1),]
	private Vector2Int _WorldSize = new(10, 15);
	[SerializeField]
	private Tilemap _GroundTilemap;
	[SerializeField]
	private Tilemap _DetailsTilemap;
	[SerializeField]
	private TileBase _GroundTile;
	[SerializeField]
	private TileBase _CastleTile;
	[SerializeField, MinValue(0f),]
	private float _NoiseScale = 2f;
	[SerializeField, Range(0f, 1f),]
	private float _ThreshHold = 0.5f;
	[SerializeField]
	private GameObject _SpawnerPrefab;

	private GameObject _PlayerSpawner;


	private void Start()
	{
		GenerateWorld();
	}

	[Button]
	private void Clear()
	{
		GameManager.Instance.HexMap.Clear();
		_GroundTilemap?.ClearAllTiles();
		_DetailsTilemap?.ClearAllTiles();
		if (Application.isPlaying)
			Destroy(_PlayerSpawner);
	}

	[Button("Generate")]
	private void GenerateWorld()
	{
		if (!_GroundTilemap || !_DetailsTilemap)
			return;

		Clear();

		// Create world data.
		Random.InitState(Environment.TickCount);

		for (var r = 0; r < _WorldSize.y; r++)
		for (int q = -r / 2; q < _WorldSize.x - r / 2; q++)
		{
			var hex = new Hex(q, r);
			float noise = Mathf.PerlinNoise(hex.Offset.x * _NoiseScale, hex.Offset.y * _NoiseScale);
			hex.Visuals = noise > _ThreshHold ? _GroundTile : null;
			GameManager.Instance.HexMap.Add(hex);
		}

		BoundsInt bounds = new(0, 0, 0, _WorldSize.x, _WorldSize.y, 1);
		TileBase[] tiles = GameManager.Instance.HexMap.GetTileMap(_WorldSize);
		if (Application.isPlaying)
		{
			// Decide player capital placement.
			int[] randomTiles = Enumerable.Range(0, tiles.Length).OrderBy(_ => Random.value).ToArray();
			var hasFoundCapitalTile = false;
			foreach (int index in randomTiles)
			{
				if (!tiles[index]) continue;

				// Flatten to offset coordinates.
				int x = index % _WorldSize.x;
				int y = index / _WorldSize.x;

				// Spawn player capital.
				hasFoundCapitalTile = true;

				Hex foundHex = GameManager.Instance.HexMap.Find(new Vector3Int(x, y, 0));
				foundHex.Building = _CastleTile;

				GameManager.Instance.SetPlayerCapital(foundHex.Offset);
				_DetailsTilemap.SetTile(foundHex.Offset, foundHex.Building);
				break;
			}

			if (!hasFoundCapitalTile)
				Debug.LogWarning("There is no valid tile to spawn the player city in!");
			else
			{
				// Spawner a player spawner.
				Vector3 spawnerPosition = _GroundTilemap.GetCellCenterWorld(GameManager.Instance
						 .PlayerCapitalPosition);
				spawnerPosition.z = -10f; //< Offset by -10 units so that the camera is in front.
				_PlayerSpawner = Instantiate(_SpawnerPrefab, spawnerPosition, Quaternion.identity);

				// Setup spawner.
				var spawnerComp = _PlayerSpawner.GetComponent<Spawner>();
				spawnerComp.SpawnerTag = SpawnerTag.Player;
				// Spawn player.
				spawnerComp.Spawn(GameManager.Instance.Player.transform);
			}
		}

		_GroundTilemap.SetTilesBlock(bounds, tiles);
	}
}
}
