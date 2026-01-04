using System;
using System.Linq;
using PixelCiv.Components;
using PixelCiv.Managers;
using PixelCiv.Scriptable_Objects;
using PixelCiv.Utilities.Hex;
using PixelCiv.Utilities.Types;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace PixelCiv
{
[HideMonoScript]
public class WorldGenerator : MonoBehaviour
{
	[SerializeField, MinValue(1),]
	private Vector2Int _WorldSize = new(20, 15);
	[SerializeField]
	private Tilemap _GroundTilemap;
	[SerializeField]
	private Tilemap _DetailsTilemap;
	[SerializeField]
	private TileBase _CastleTile;
	[SerializeField, MinValue(0f),]
	private float _NoiseScale = 0.25f;
	[SerializeField, Range(0f, 1f),]
	private float _SeaLevel = 0.4f;
	[SerializeField, Range(0f, 1f),]
	private float _MountainLevel = 0.7f;
	[SerializeField]
	private GameObject _SpawnerPrefab;
	[SerializeField, OnValueChanged(nameof(UpdateTileMapColours)),]
	private TileTypeSet _TileSet;

	#if UNITY_EDITOR
	private readonly HexMap _EditorMap = new();
	#endif

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
		#if UNITY_EDITOR
		_EditorMap.Clear();
		#endif

		if (!Application.isPlaying) return;

		GameManager.Instance.HexMap.Clear();
		Destroy(_PlayerSpawner);
	}

	[Button("Generate")]
	private void GenerateWorld()
	{
		if (!_GroundTilemap || !_TileSet) return;

		Clear();
		Random.InitState(Environment.TickCount);
		HexMap hexMap = GetHexMap();

		// Generate hex map.
		for (var r = 0; r < _WorldSize.y; r++)
		for (int q = -r / 2; q < _WorldSize.x - r / 2; q++)
		{
			var hex = new Hex(q, r);
			float noise = Mathf.PerlinNoise(hex.Coordinates.Offset.x * _NoiseScale,
											hex.Coordinates.Offset.y * _NoiseScale);

			if (noise > _MountainLevel)
				hex.Type = TileType.Mountain;
			else if (noise > _SeaLevel && noise < _MountainLevel)
				hex.Type = TileType.Grassland;
			else
				hex.Type = TileType.Sea;

			hexMap.Add(hex);
		}

		// Decide player capital.
		TileType[] tiles = hexMap.GetTileMap(_WorldSize);
		if (_DetailsTilemap)
			PlacePlayerCapital(tiles);

		// Fill ground tiles.
		BoundsInt bounds = new(0, 0, 0, _WorldSize.x, _WorldSize.y, 1);
		TileBase[] tileHexes = Enumerable.Repeat(_TileSet.Base, tiles.Length).ToArray();
		_GroundTilemap.SetTilesBlock(bounds, tileHexes);

		// Colour tiles.
		Hex[] map = hexMap.GetMap();
		foreach (Hex tile in map)
			_GroundTilemap.SetColor(tile.Coordinates.Offset, _TileSet.Set[tile.Type]);
	}

	private HexMap GetHexMap()
	{
		if (Application.isPlaying && GameManager.Instance)
			return GameManager.Instance.HexMap;

		#if UNITY_EDITOR
		return _EditorMap;
		#endif
	}

	private void PlacePlayerCapital(TileType[] tiles)
	{
		int[] randomTiles = Enumerable.Range(0, tiles.Length)
									  .OrderBy(_ => Random.value)
									  .ToArray();
		var hasFoundCapitalTile = false;
		foreach (int index in randomTiles)
		{
			if (tiles[index] != TileType.Grassland) continue;

			// Convert to axial coordinates.
			int r = index / _WorldSize.x;
			int q = index % _WorldSize.x - (r - (r & 1)) / 2;

			Hex foundHex = GetHexMap().Find(new HexCoords(q, r));
			foundHex.Building = _CastleTile;

			if (Application.isPlaying)
				GameManager.Instance.SetPlayerCapital(foundHex.Coordinates);
			_DetailsTilemap.SetTile(foundHex.Coordinates.Offset, foundHex.Building);

			hasFoundCapitalTile = true;

			if (Application.isPlaying)
				SpawnPlayerSpawner(foundHex.Coordinates.Offset);
			break;
		}

		if (!hasFoundCapitalTile)
			Debug.LogWarning("No valid tile to spawn the player city!");
	}

	private void SpawnPlayerSpawner(Vector3Int capitalOffset)
	{
		Vector3 spawnerPosition = _GroundTilemap.GetCellCenterWorld(capitalOffset);
		spawnerPosition.z = -10f;

		_PlayerSpawner = Instantiate(_SpawnerPrefab, spawnerPosition,
									 Quaternion.identity);
		var spawnerComp = _PlayerSpawner.GetComponent<Spawner>();
		spawnerComp.SpawnerTag = SpawnerTag.Player;
		spawnerComp.Spawn(GameManager.Instance.Player.transform);
	}

	#if UNITY_EDITOR
	private void UpdateTileMapColours()
	{
		if (!_GroundTilemap || !_TileSet) return;

		Hex[] map = GetHexMap().GetMap();
		foreach (Hex tile in map)
			_GroundTilemap.SetColor(tile.Coordinates.Offset, _TileSet.Set[tile.Type]);
	}
	#endif
}
}
