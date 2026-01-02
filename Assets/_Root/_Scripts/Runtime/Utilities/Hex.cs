using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace PixelCiv.Utilities
{
public readonly struct HexCoords : IEquatable<HexCoords>
{
	public int Q => Axial.x;
	public int R => Axial.y;
	public int S => Q + R;
	[ShowInInspector]
	public Vector2Int Axial { get; }
	[ShowInInspector]
	public Vector3Int Offset { get; }

	public static HexCoords Zero => new(0, 0);


	public HexCoords(int q, int r)
	{
		Axial = new Vector2Int(q, r);
		Offset = AxialToOffset(Axial);
	}

	public HexCoords(Vector2Int axial) : this(axial.x, axial.y) { }

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

	public static int Distance(HexCoords a, HexCoords b)
	{
		int dq = a.Q - b.Q;
		int dr = a.R - b.R;
		return (Mathf.Abs(dq) + Mathf.Abs(dr) + Mathf.Abs(dq + dr)) / 2;
	}

	public int DistanceTo(HexCoords other)
	{
		return Distance(this, other);
	}

	public bool Equals(HexCoords other)
	{
		return Axial.Equals(other.Axial);
	}

	public override bool Equals(object obj)
	{
		return obj is HexCoords other && Equals(other);
	}

	public override int GetHashCode()
	{
		return Axial.GetHashCode();
	}

	public static bool operator ==(HexCoords a, HexCoords b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(HexCoords a, HexCoords b)
	{
		return !a.Equals(b);
	}
}

public class Hex
{
	private static readonly Vector2Int[] _AxialDirections =
	{
			new(+1, -1),
			new(0, -1),
			new(-1, 0),
			new(-1, +1),
			new(0, +1),
			new(+1, 0),
	};


	public HexCoords Coordinates { get; }
	public TileBase Building;
	public Guid UnitID = Guid.Empty;
	public TileBase Visuals;


	public Hex(HexCoords coords, TileBase visuals = null)
	{
		Coordinates = coords;
		Visuals = visuals;
	}

	public Hex(int q, int r, TileBase visuals = null) :
			this(new HexCoords(q, r), visuals) { }

	public int DistanceTo(Hex other)
	{
		return HexCoords.Distance(Coordinates, other.Coordinates);
	}

	public HexCoords[] GetNeighbours()
	{
		var neighbours = new HexCoords[6];
		for (var i = 0; i < _AxialDirections.Length; i++)
			neighbours[i] = new HexCoords(Coordinates.Axial + _AxialDirections[i]);
		return neighbours;
	}

	public static HexCoords[] GetNeighbours(HexCoords coords)
	{
		var neighbours = new HexCoords[6];
		for (var i = 0; i < _AxialDirections.Length; i++)
			neighbours[i] = new HexCoords(coords.Axial + _AxialDirections[i]);
		return neighbours;
	}

	public List<HexCoords> GetRing(int radius)
	{
		var results = new List<HexCoords>();
		if (radius == 0)
		{
			results.Add(Coordinates);
			return results;
		}

		// Start at top-right.
		Vector2Int hex = Coordinates.Axial + _AxialDirections[4] * radius;

		for (var i = 0; i < 6; i++)
		for (var j = 0; j < radius; j++)
		{
			results.Add(new HexCoords(hex));
			hex += _AxialDirections[i];
		}

		return results;
	}

	public static List<HexCoords> GetRing(HexCoords center, int radius)
	{
		var results = new List<HexCoords>();
		if (radius == 0)
		{
			results.Add(center);
			return results;
		}

		// Start at top-right.
		Vector2Int hex = center.Axial + _AxialDirections[4] * radius;

		for (var i = 0; i < 6; i++)
		for (var j = 0; j < radius; j++)
		{
			results.Add(new HexCoords(hex));
			hex += _AxialDirections[i];
		}

		return results;
	}
}

public class HexMap
{
	// Axial Coordinates -> Hex.
	private readonly Dictionary<Vector2Int, Hex> _HexTiles = new();

	public Hex Find(HexCoords coords)
	{
		return _HexTiles.GetValueOrDefault(coords.Axial);
	}

	public void Add(Hex hex)
	{
		_HexTiles.Add(hex.Coordinates.Axial, hex);
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
			if (hex.Coordinates.Offset.x < 0 || hex.Coordinates.Offset.x >= worldSize.x ||
				hex.Coordinates.Offset.y < 0 || hex.Coordinates.Offset.y >= worldSize.y)
				continue;

			int index = hex.Coordinates.Offset.y * worldSize.x + hex.Coordinates.Offset.x;
			tiles[index] = hex.Visuals;
		}

		return tiles;
	}

	public TileBase[] GetTileMap()
	{
		Vector2Int min = new(int.MaxValue, int.MaxValue);
		Vector2Int max = new(int.MinValue, int.MinValue);
		foreach (Hex hex in _HexTiles.Values)
		{
			min.x = Mathf.Min(min.x, hex.Coordinates.Offset.x);
			min.y = Mathf.Min(min.y, hex.Coordinates.Offset.y);
			max.x = Mathf.Max(max.x, hex.Coordinates.Offset.x);
			max.y = Mathf.Max(max.y, hex.Coordinates.Offset.y);
		}

		int width = max.x - min.x + 1;
		int height = max.y - min.y + 1;

		var tiles = new TileBase[width * height];
		foreach (Hex hex in _HexTiles.Values)
		{
			int x = hex.Coordinates.Offset.x - min.x;
			int y = hex.Coordinates.Offset.y - min.y;

			int index = y * width + x;
			tiles[index] = hex.Visuals;
		}

		return tiles;
	}
}
}
