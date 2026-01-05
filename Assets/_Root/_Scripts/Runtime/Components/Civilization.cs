using System;
using System.Collections.Generic;
using System.Linq;
using PixelCiv.Managers;
using PixelCiv.Utilities.Extensions;
using PixelCiv.Utilities.Hex;
using PixelCiv.Utilities.Types;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PixelCiv.Components
{
[Serializable]
public class Civilization
{
	public const int UNIT_SPAWN_RANGE = 1;

	[ShowInInspector, ReadOnly,]
	public bool IsPlayer { get; private set; }
	[ShowInInspector, ReadOnly,]
	public CivilizationType Type { get; private set; }
	[ShowInInspector, ReadOnly,]
	public Color Colour { get; private set; } = Color.white;
	[ShowInInspector, ReadOnly,]
	public List<Guid> Units { get; } = new();
	[ShowInInspector, ReadOnly,]
	public List<HexCoords> Territory { get; } = new();


	public Civilization(CivilizationType type = CivilizationType.Random,
			bool isPlayer = false)
	{
		// Reserve space.
		Units.Capacity = 50;
		Territory.Capacity = 50;

		if (type == CivilizationType.Random)
		{
			// Compose an array of available civilization types, ignore the random member.
			CivilizationType[] types = Enum.GetValues(typeof(CivilizationType))
										   .Cast<CivilizationType>()
										   .Where(t => t != CivilizationType.Random)
										   .ToArray();
			Type = types[Random.Range(0, types.Length)];
		}
		else
			Type = type;

		IsPlayer = isPlayer;

		// Assign a colour based on Civilization type.
		switch (Type)
		{
		case CivilizationType.Rumos:
			Colour = Colour.FromHex("f0db7d");
			break;
		case CivilizationType.Veltran:
			Colour = Colour.FromHex("7430FF");
			break;
		case CivilizationType.Aruna:
			Colour = Colour.FromHex("21D0FF");
			break;
		default: throw new ArgumentOutOfRangeException();
		}
	}

	public void AddHexTile(HexCoords coords)
	{
		Territory.Add(coords);
	}

	public void AddUnit(Guid unitID)
	{
		Units.Add(unitID);
	}

	public Hex GetCapitalTile()
	{
		return Territory.Select(hexCoords => GameManager.Instance.HexMap.Find(hexCoords))
						.FirstOrDefault(hex => hex != null && hex.Building != null);
	}

	public void Reset()
	{
		Units.Clear();
		Territory.Clear();
	}
}
}
