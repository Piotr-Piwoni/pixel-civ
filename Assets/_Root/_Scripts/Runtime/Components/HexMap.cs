using System.Collections.Generic;
using PixelCiv.Utilities.Hex;
using PixelCiv.Utilities.Types;
using UnityEngine;

namespace PixelCiv.Components
{
public class HexMap
{
	private readonly List<Hex> _HexTiles = new();


	public HexMap()
	{
		_HexTiles.Capacity = 1000;
	}

	public Hex Find(HexCoords coords)
	{
		return _HexTiles.Find(n => n.Coordinates == coords);
	}

	public void Add(Hex hex)
	{
		_HexTiles.Add(hex);
	}

	public void Clear()
	{
		_HexTiles.Clear();
	}

	public TileType[] GetTileMap(Vector2Int worldSize)
	{
		var tiles = new TileType[worldSize.x * worldSize.y];
		foreach (Hex hex in _HexTiles)
		{
			// Skip hexes outside the render bounds.
			if (hex.Coordinates.Offset.x < 0 || hex.Coordinates.Offset.x >= worldSize.x ||
				hex.Coordinates.Offset.y < 0 || hex.Coordinates.Offset.y >= worldSize.y)
				continue;

			int index = hex.Coordinates.Offset.y * worldSize.x + hex.Coordinates.Offset.x;
			tiles[index] = hex.Type;
		}

		return tiles;
	}

	public Hex[] GetMap()
	{
		return _HexTiles.ToArray();
	}
}
}
