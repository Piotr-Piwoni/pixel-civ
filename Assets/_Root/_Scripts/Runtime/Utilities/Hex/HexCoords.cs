using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PixelCiv.Utilities.Hex
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
}
