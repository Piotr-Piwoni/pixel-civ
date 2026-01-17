using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class BuildingCreator : EditorWindow
{
	[SerializeField]
	private VisualTreeAsset _VisualTreeAsset;

	public void CreateGUI()
	{
		// Each editor window contains a root VisualElement object
		VisualElement root = rootVisualElement;

		// Instantiate UXML
		VisualElement labelFromUxml = _VisualTreeAsset.Instantiate();
		root.Add(labelFromUxml);
	}

	[MenuItem("Window/UI Toolkit/BuildingCreator")]
	public static void ShowExample()
	{
		var wnd = GetWindow<BuildingCreator>();
		wnd.titleContent = new GUIContent("BuildingCreator");
	}
}
