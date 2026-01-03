using PixelCiv.Components;
using UnityEditor;
using UnityEngine;

namespace PixelCiv.Editor.Custom
{
[CustomEditor(typeof(Grid)), CanEditMultipleObjects,]
public class ReadOnlyGridEditor : UnityEditor.Editor
{
	public override void OnInspectorGUI()
	{
		var grid = (Grid)target;
		if (grid.GetComponent<HexGrid>())
		{
			EditorGUI.BeginDisabledGroup(true);
			DrawDefaultInspector();
			EditorGUI.EndDisabledGroup();
		}
		else
			DrawDefaultInspector();
	}
}
}
