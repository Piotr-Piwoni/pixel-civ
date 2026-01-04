using System;
using System.Collections.Generic;
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
	private Tilemap _TerritoriesTilemap;
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

	private GameObject _PlayerSpawner;


	private void Start()
	{
		GenerateWorld();
	}

	private void ClaimTerritory()
	{
		if (!_TerritoriesTilemap) return;

		// Claim an area around each civilization's capital.
		foreach (Civilization civ in GameManager.Instance.Civilizations)
		{
			const int CLAIM_AREA = 2;
			List<HexCoords> territory = civ.GetCapitalTile().GetSpiral(CLAIM_AREA);
			foreach (HexCoords hexCoords in territory)
			{
				// Make sure the candidate tile isn't already in another civilization's territory.
				var validTile = true;
				foreach (Civilization other in GameManager.Instance.Civilizations)
				{
					if (other.Territory.Any(tile => tile == hexCoords))
						validTile = false;

					if (!validTile)
						break;
				}

				// Claim the tile as territory and render it on screen.
				if (!validTile) continue;
				civ.AddHexTile(hexCoords);
				_TerritoriesTilemap.SetTile(hexCoords.Offset, _TileSet.Base);
				_TerritoriesTilemap.SetColor(hexCoords.Offset, civ.Colour);
			}
		}
	}

	[Button]
	private void Clear()
	{
		_GroundTilemap?.ClearAllTiles();
		_DetailsTilemap?.ClearAllTiles();
		_TerritoriesTilemap?.ClearAllTiles();
		#if UNITY_EDITOR
		_EditorMap.Clear();
		_Civilizations.Clear();
		#endif

		if (!Application.isPlaying) return;

		GameManager.Instance.HexMap.Clear();
		Destroy(_PlayerSpawner);

		foreach (Civilization civ in GameManager.Instance.Civilizations)
			civ.Reset();
	}

	[Button("Generate")]
	private void GenerateWorld()
	{
		if (!_GroundTilemap || !_TileSet) return;

		Clear();
		Random.InitState(Environment.TickCount);
		HexMap hexMap = GetHexMap();

		#if UNITY_EDITOR
		if (!Application.isPlaying)
			SimulateGame();
		#endif

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
		if (_DetailsTilemap && Application.isPlaying)
			PlacePlayerCapital(tiles);
		else if (_DetailsTilemap && !Application.isPlaying)
			PlacePlayerCapitalEditor(tiles);

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
		foreach (Civilization civ in GameManager.Instance.Civilizations)
		{
			var hasFoundCapitalTile = false;
			foreach (int index in randomTiles)
			{
				if (tiles[index] != TileType.Grassland) continue;

				// Convert to axial coordinates.
				int r = index / _WorldSize.x;
				int q = index % _WorldSize.x - (r - (r & 1)) / 2;
				var candidateCoords = new HexCoords(q, r);
				Hex candidateHex = GetHexMap().Find(candidateCoords);

				// Skip if any other civ already has a capital here.
				bool isTaken = GameManager.Instance.Civilizations
										  .Where(other => other != civ)
										  .Any(other =>
										  {
											  Hex otherCapital = other.GetCapitalTile();
											  return otherCapital != null &&
													 otherCapital.Building ==
													 _CastleTile &&
													 otherCapital.Coordinates.Equals(
															 candidateCoords);
										  });


				if (isTaken) continue;

				Debug.Log($"Spawned the capital for {civ.Type}.");

				// Place the capital.
				candidateHex.Building = _CastleTile;
				GameManager.Instance.SetPlayerCapital(candidateHex.Coordinates);

				civ.AddHexTile(candidateHex.Coordinates);
				_DetailsTilemap.SetTile(candidateHex.Coordinates.Offset,
										candidateHex.Building);

				if (civ.IsPlayer)
					SpawnPlayerSpawner(candidateHex.Coordinates.Offset);

				hasFoundCapitalTile = true;
				break;
			}

			if (!hasFoundCapitalTile)
				Debug.LogWarning(
						$"No valid tile to spawn the {civ.Type}'s capital city!");
		}

		ClaimTerritory();
	}

	private void PlacePlayerCapitalEditor(TileType[] tiles)
	{
		#if UNITY_EDITOR
		int[] randomTiles = Enumerable.Range(0, tiles.Length)
									  .OrderBy(_ => Random.value)
									  .ToArray();
		foreach (Civilization civ in _Civilizations)
		{
			var hasFoundCapitalTile = false;
			Debug.Log("Spawning the capital...");

			foreach (int index in randomTiles)
			{
				if (tiles[index] != TileType.Grassland)
					continue;

				// Convert to axial coordinates.
				int r = index / _WorldSize.x;
				int q = index % _WorldSize.x - (r - (r & 1)) / 2;
				var candidateCoords = new HexCoords(q, r);
				Hex candidateHex = GetHexMap().Find(candidateCoords);

				// Skip if any other civ already has a capital here.
				bool isTaken = _Civilizations
							   .Where(other => other != civ)
							   .Any(other =>
							   {
								   Hex otherCapital = other
													  .Territory
													  .Select(hexCoords => GetHexMap()
															  .Find(hexCoords))
													  .FirstOrDefault(hex =>
															  hex != null &&
															  hex.Building);

								   return otherCapital != null &&
										  otherCapital.Building == _CastleTile &&
										  otherCapital.Coordinates
													  .Equals(candidateCoords);
							   });
				if (isTaken) continue;

				Debug.Log($"Spawned the capital for {civ.Type}.");

				// Place the capital.
				candidateHex.Building = _CastleTile;
				civ.AddHexTile(candidateHex.Coordinates);
				_DetailsTilemap.SetTile(candidateHex.Coordinates.Offset,
										candidateHex.Building);

				hasFoundCapitalTile = true;
				break; // stop searching for this civ.
			}

			if (!hasFoundCapitalTile)
				Debug.LogWarning("No valid tile to spawn the player city!");
		}

		ClaimTerritoryEditor();
		#endif
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
	private readonly HexMap _EditorMap = new();
	[SerializeField]
	private int _CivNumber = 1;
	[SerializeField, ReadOnly,]
	private List<Civilization> _Civilizations = new();


	private void SimulateGame()
	{
		Debug.Log("Initializing Fields...");

		// Init civilizations.
		for (var i = 0; i < _CivNumber; i++)
			_Civilizations.Add(new Civilization());
	}

	private void ClaimTerritoryEditor()
	{
		if (!_TerritoriesTilemap) return;

		// Claim an area around each civilization's capital.
		foreach (Civilization civ in _Civilizations)
		{
			Hex otherCapital = civ.Territory
								  .Select(hexCoords => GetHexMap().Find(hexCoords))
								  .FirstOrDefault(hex => hex != null && hex.Building);
			if (otherCapital == null) continue;

			List<HexCoords> territory = otherCapital.GetSpiral(2);
			foreach (HexCoords hexCoords in territory)
			{
				// Make sure the candidate tile isn't already in another civilization's territory.
				var validTile = true;
				foreach (Civilization other in _Civilizations)
				{
					if (other.Territory.Any(tile => tile == hexCoords))
						validTile = false;

					if (!validTile)
						break;
				}

				// Claim the tile as territory and render it on screen.
				if (!validTile) continue;
				civ.AddHexTile(hexCoords);
				_TerritoriesTilemap.SetTile(hexCoords.Offset, _TileSet.Base);
				_TerritoriesTilemap.SetColor(hexCoords.Offset, civ.Colour);
			}
		}
	}

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
