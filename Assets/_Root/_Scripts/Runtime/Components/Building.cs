using System;
using PixelCiv.Scriptable_Objects;
using PixelCiv.Utilities.Hex;
using PixelCiv.Utilities.Types;
using UnityEngine;

namespace PixelCiv.Components
{
public class Building
{
	public BuildingState State { get; private set; }
	public BuildingTypeData Data { get; }
	public Guid ID { get; private set; }
	public HexCoords Position { get; private set; }
	public int CurrentHealth { get; private set; }


	public Building(BuildingTypeData data, HexCoords position)
	{
		ID = Guid.NewGuid();
		Data = data;
		CurrentHealth = Data.Health;
		State = BuildingState.Normal;
		Position = position;
	}

	public void Damage(int amount)
	{
		CurrentHealth = Mathf.Clamp(CurrentHealth -= amount, 0, Data.Health);
		UpdateState();

	}

	public void Heal(int amount)
	{
		CurrentHealth = Mathf.Clamp(CurrentHealth += amount, 0, Data.Health);
		UpdateState();
	}

	private void UpdateState()
	{
		if (CurrentHealth <= 0)
			State = BuildingState.Destroyed;
		else if (CurrentHealth >= Data.Health)
			State = BuildingState.Normal;
		else
			State = BuildingState.Damaged;
	}
}
}
