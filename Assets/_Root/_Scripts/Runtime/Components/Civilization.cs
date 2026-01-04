using System;
using System.Collections.Generic;
using System.Linq;
using PixelCiv.Managers;
using PixelCiv.Utilities.Hex;
using PixelCiv.Utilities.Types;
using Sirenix.OdinInspector;
using Random = UnityEngine.Random;

namespace PixelCiv.Components
{
[Serializable]
public class Civilization
{
	[ShowInInspector]
	public bool IsPlayer { get; private set; }
	[ShowInInspector]
	public CivilizationType Type { get; private set; }
	[ShowInInspector]
	public List<Guid> Units { get; } = new();
	[ShowInInspector]
	public List<HexCoords> Territory { get; } = new();


	public Civilization(CivilizationType type = CivilizationType.Random,
			bool isPlayer = false)
	{
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
						.FirstOrDefault(hex => hex.Building);
	}
}
}
