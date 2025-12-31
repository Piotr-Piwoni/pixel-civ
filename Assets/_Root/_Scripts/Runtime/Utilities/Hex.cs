using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace PixelCiv.Utilities
{
public class Hex
{
	private static readonly Vector2Int[] _AxialDirections =
	{
			new(1, 0), new(1, -1), new(0, -1),
			new(-1, 0), new(-1, 1), new(0, 1),
	};

	public int Q => Axial.x;
	public int R => Axial.y;
	public Vector2Int Axial { get; }
	public Vector3Int Offset { get; }

	public TileBase Building;
	public Guid UnitID = Guid.Empty;
	public TileBase Visuals;


	public Hex(Vector2Int axial, TileBase visuals = null) :
			this(axial.x, axial.y, visuals) { }

	public Hex(int q, int r, TileBase visuals = null)
	{
		Axial = new Vector2Int(q, r);
		Offset = AxialToOffset(Axial);
		Visuals = visuals;
	}

	public static Vector3Int AxialToOffset(Vector2Int axial)
	{
		int x = axial.x + (axial.y - (axial.y & 1)) / 2;
		int y = axial.y;
		return new Vector3Int(x, y, 0);
	}

	public static Vector2Int OffsetToAxial(Vector3Int offset)
	{
		int q = offset.x - (offset.y - (offset.y & 1)) / 2;
		int r = offset.y;
		return new Vector2Int(q, r);
	}

	public static int Distance(Vector2Int a, Vector2Int b)
	{
		int dq = a.x - b.x;
		int dr = a.y - b.y;
		return (Mathf.Abs(dq) + Mathf.Abs(dr) + Mathf.Abs(dq + dr)) / 2;
	}

	public int DistanceTo(Hex other)
	{
		return Distance(Axial, other.Axial);
	}

	public static IEnumerable<Vector2Int> GetNeighbours(Vector2Int axial)
	{
		return _AxialDirections.Select(direction => axial + direction);
	}

	public static IEnumerable<Vector2Int> GetRing(Vector2Int center, int radius)
	{
		if (radius == 0)
		{
			yield return center;
			yield break;
		}

		// Starting hex direction.
		Vector2Int hex = center + _AxialDirections[4] * radius;

		foreach (Vector2Int direction in _AxialDirections)
			for (var step = 0; step < radius; step++)
			{
				yield return hex;
				hex += direction;
			}
	}
}

public class HexMap
{
	// Axial Coordinates -> Hex.
	private readonly Dictionary<Vector2Int, Hex> _HexTiles = new();

	public Hex Find(Vector2Int axial)
	{
		return _HexTiles.GetValueOrDefault(axial);
	}

	public Hex Find(Vector3Int offset)
	{
		return Find(Hex.OffsetToAxial(offset));
	}

	public void Add(Hex hex)
	{
		_HexTiles.Add(hex.Axial, hex);
	}

	public void Clear()
	{
		_HexTiles.Clear();
	}

	public TileBase[] GetTileMap(Vector2Int worldSize)
	{
		var tiles = new TileBase[worldSize.x * worldSize.y];
		foreach (Hex hex in _HexTiles.Values)
		{
			// Skip hexes outside the render bounds.
			if (hex.Offset.x < 0 || hex.Offset.x >= worldSize.x ||
				hex.Offset.y < 0 || hex.Offset.y >= worldSize.y)
				continue;

			int index = hex.Offset.y * worldSize.x + hex.Offset.x;
			tiles[index] = hex.Visuals;
		}

		return tiles;
	}

	public TileBase[] GetTileMap()
	{
		Vector2Int min = new(int.MinValue, int.MinValue);
		Vector2Int max = new(int.MaxValue, int.MaxValue);
		foreach (Hex hex in _HexTiles.Values)
		{
			min.x = Mathf.Min(min.x, hex.Offset.x);
			min.y = Mathf.Min(min.y, hex.Offset.y);
			max.x = Mathf.Max(max.x, hex.Offset.x);
			max.y = Mathf.Max(max.y, hex.Offset.y);
		}

		int width = max.x - min.x + 1;
		int height = max.y - min.y + 1;

		var tiles = new TileBase[width * height];
		foreach (Hex hex in _HexTiles.Values)
		{
			int x = hex.Offset.x - min.x;
			int y = hex.Offset.y - min.y;

			int index = y * width + x;
			tiles[index] = hex.Visuals;
		}

		return tiles;
	}
}
}
