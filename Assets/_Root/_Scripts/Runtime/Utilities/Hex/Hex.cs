using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace PixelCiv.Utilities.Hex
{
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
	public TileType Type;
	public Guid UnitID = Guid.Empty;


	public Hex(HexCoords coords, TileType? type = null)
	{
		Coordinates = coords;
		Type = type ?? TileType.Sea;
	}

	public Hex(int q, int r, TileType? type = null) :
			this(new HexCoords(q, r), type) { }

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
}
