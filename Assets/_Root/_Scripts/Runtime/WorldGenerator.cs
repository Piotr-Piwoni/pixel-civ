using System;
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
		#if UNITY_EDITOR
		DestroyImmediate(_PlayerSpawner);
		#else
		Destroy(_PlayerSpawner);
		#endif
	}

	[Button("Generate")]
	private void GenerateWorld()
	{
		if (!_GroundTilemap || !_DetailsTilemap)
			return;

		Clear();

		// Create world data.
		var seed = (int)DateTime.Now.TimeOfDay.TotalSeconds;
		Random.InitState(seed);

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
		do
		{
			int x = Random.Range(0, _WorldSize.x);
			int y = Random.Range(0, _WorldSize.y);
			int index = y * _WorldSize.x + x;
			if (!tiles[index]) continue;

			// Spawn player capital.
			_HasChosenCapitalTile = true;
			GameManager.Instance.SetPlayerCapital(new Vector3Int(x, y, 0));

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
		} while (!_HasChosenCapitalTile);

		// Render.
		_GroundTilemap.SetTilesBlock(bounds, tiles);
		_DetailsTilemap.SetTile(GameManager.Instance.PlayerCapitalPosition, _CastleTile);
	}
}
}
