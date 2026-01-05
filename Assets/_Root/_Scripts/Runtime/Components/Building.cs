using System;
using PixelCiv.Scriptable_Objects;
using PixelCiv.Utilities.Hex;
using PixelCiv.Utilities.Types;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PixelCiv.Components
{
[Serializable]
public class Building

{
	private static int _MaxHealth = 1;

	[ShowInInspector, ReadOnly, PropertyOrder(2),]
	public BuildingState State { get; private set; }
	[ShowInInspector, ReadOnly, PropertyOrder(4),
	 InlineEditor(InlineEditorObjectFieldModes.Boxed),]
	public BuildingTypeData Data { get; }
	[ShowInInspector, ReadOnly, DisplayAsString(TextAlignment.Center), PropertyOrder(1),]
	public Guid ID { get; private set; }
	[ShowInInspector, ReadOnly, PropertyOrder(5),]
	public HexCoords Position { get; private set; }
	[ShowInInspector, ReadOnly, Min(0), PropertyOrder(3),
	 ProgressBar(0, nameof(_MaxHealth), ColorGetter = nameof(GetHealthBarColour)),]
	public int CurrentHealth { get; private set; }


	public Building(BuildingTypeData data, HexCoords position)
	{
		ID = Guid.NewGuid();
		Data = data;
		CurrentHealth = Data.Health;
		State = BuildingState.Normal;
		Position = position;
		_MaxHealth = Data.Health;
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

	// Odin only functions.
	private Color GetHealthBarColour(float value)
	{
		return Color.Lerp(Color.red, Color.green, Mathf.Pow(value / _MaxHealth, 2));
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
