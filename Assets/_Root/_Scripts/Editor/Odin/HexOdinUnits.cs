using PixelCiv.Scriptable_Objects;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace PixelCiv.Editor.Odin
{
[InitializeOnLoad]
public static class HexOdinUnits
{
	static HexOdinUnits()
	{
		EditorApplication.delayCall += RegisterHexUnit;
	}

	private static void RegisterHexUnit()
	{
		var settings = Resources.Load<GridSettings>("Game/Settings/GridSettings");
		if (!settings) return;

		UnitNumberUtility.AddCustomUnit("Hex",
										new[] { "hex", "hx", },
										UnitCategory.Distance,
										1m / (decimal)settings.CellSize.x);
	}
}
}
