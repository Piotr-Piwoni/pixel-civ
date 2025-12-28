using System;
using System.Linq;
using PixelCiv.Managers;
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

	private bool _HasChosenCapitalTile;
	private GameObject _PlayerSpawner;


	private void Start()
	{
		GenerateWorld();
	}

	[Button]
	private void Clear()
	{
		_GroundTilemap?.ClearAllTiles();
		_DetailsTilemap?.ClearAllTiles();
		_HasChosenCapitalTile = false;
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

		BoundsInt bounds = new(0, 0, 0, _WorldSize.x, _WorldSize.y, 1);
		int totalWorldSize = _WorldSize.x * _WorldSize.y;
		var tiles = new TileBase[totalWorldSize];

		for (var y = 0; y < _WorldSize.y; y++)
		for (var x = 0; x < _WorldSize.x; x++)
		{
			float noise = Mathf.PerlinNoise(x * _NoiseScale, y * _NoiseScale);

			int index = y * _WorldSize.x + x;
			tiles[index] = noise > _ThreshHold ? _GroundTile : null;
		}

		// Decide player capital placement.
		int[] randomTiles = Enumerable.Range(0, totalWorldSize).OrderBy(_ => Random.value).ToArray();
		foreach (int index in randomTiles)
		{
			if (!tiles[index]) continue;

			int x = index % _WorldSize.x;
			int y = index / _WorldSize.x;

			// Spawn player capital.
			_HasChosenCapitalTile = true;
			GameManager.Instance.SetPlayerCapital(new Vector3Int(x, y, 0));
			break;
		}

		if (!_HasChosenCapitalTile)
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

			_DetailsTilemap.SetTile(GameManager.Instance.PlayerCapitalPosition, _CastleTile);
		}

		_GroundTilemap.SetTilesBlock(bounds, tiles);
	}
}
}
