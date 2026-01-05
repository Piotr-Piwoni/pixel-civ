using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PixelCiv.Utilities.Hex
{
public struct HexCoords : IEquatable<HexCoords>
{
	public int Q => Axial.x;
	public int R => Axial.y;
	public int S => Q + R;
	public int X => Offset.x;
	public int Y => Offset.y;
	public int Z => Offset.z;

	[ShowInInspector]
	public Vector2Int Axial { get; set; }
	[ShowInInspector]
	public Vector3Int Offset => AxialToOffset(Axial);

	public static HexCoords Zero => new(0, 0);
	public static HexCoords NorthEast => new(+1, -1);
	public static HexCoords North => new(0, -1);
	public static HexCoords NorthWest => new(-1, 0);
	public static HexCoords SouthWest => new(-1, +1);
	public static HexCoords South => new(0, +1);
	public static HexCoords SouthEast => new(+1, 0);


	public static readonly HexCoords[] Directions =
	{
			NorthEast,
			North,
			NorthWest,
			SouthWest,
			South,
			SouthEast,
	};

	public HexCoords(int q, int r)
	{
		Axial = new Vector2Int(q, r);
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
		int ds = a.S - b.S;
		return (Mathf.Abs(dq) + Mathf.Abs(dr) + Mathf.Abs(ds)) / 2;
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


	// Operators.
	public static bool operator ==(HexCoords a, HexCoords b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(HexCoords a, HexCoords b)
	{
		return !a.Equals(b);
	}

	public static HexCoords operator +(HexCoords a, HexCoords b)
	{
		return new HexCoords(a.Q + b.Q, a.R + b.R);
	}

	public static HexCoords operator -(HexCoords a, HexCoords b)
	{
		return new HexCoords(a.Q - b.Q, a.R - b.R);
	}

	public static HexCoords operator *(HexCoords a, int scalar)
	{
		return new HexCoords(a.Q * scalar, a.R * scalar);
	}

	public static HexCoords operator *(int scalar, HexCoords a)
	{
		return a * scalar;
	}

	public static HexCoords operator *(HexCoords a, float scalar)
	{
		return new HexCoords(Mathf.RoundToInt(a.Axial.x * scalar),
							 Mathf.RoundToInt(a.Axial.y * scalar));
	}

	public static HexCoords operator *(float scalar, HexCoords a)
	{
		return a * scalar;
	}

	public static HexCoords operator /(HexCoords a, int scalar)
	{
		return new HexCoords(a.Q / scalar, a.R / scalar);
	}

	public static HexCoords operator /(HexCoords a, float scalar)
	{
		return new HexCoords(Mathf.RoundToInt(a.Axial.x / scalar),
							 Mathf.RoundToInt(a.Axial.y / scalar));
	}
}
}
