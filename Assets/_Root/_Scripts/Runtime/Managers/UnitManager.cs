using System;
using System.Collections.Generic;
using System.Linq;
using PixelCiv.Utilities;
using UnityEngine;

namespace PixelCiv.Managers
{
public class UnitManager : Singleton<UnitManager>
{
	[SerializeField]
	private GameObject _UnitPrefab;

	private readonly List<Unit> _Units = new();


	public void CreateUnit(Vector3Int position, Color? colour = null)
	{
		if (_Units.Any(unit => unit.CellPosition == position))
		{
			Debug.Log($"A unit already exists on the specified tile{position}.");
			return;
		}

		var unit = Instantiate(_UnitPrefab, transform).GetComponent<Unit>();
		unit.Initialize(Guid.NewGuid(), colour ?? Color.white, position);
		_Units.Add(unit);
	}
}
}
