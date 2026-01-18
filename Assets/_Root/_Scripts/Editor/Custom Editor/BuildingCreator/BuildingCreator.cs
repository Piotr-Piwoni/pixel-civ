using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using PixelCiv.Editor.Utilities;
using PixelCiv.Scriptable_Objects;
using PixelCiv.Utilities.Types;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class BuildingCreator : EditorWindow
{
	[SerializeField]
	private VisualTreeAsset _VisualTreeAsset;
	[SerializeField]
	private VisualTreeAsset _ProductionElementTemplate;
	[SerializeField]
	private VisualTreeAsset _RestrictionElementTemplate;
	[SerializeField]
	private VisualTreeAsset _SettingsTemplate;
	[SerializeField]
	private string _DefaultFolderPath = @"Assets\Resources\Game\Buildings";

	private readonly List<BuildingTypeData> _BuildingTypeDatas = new();
	private BuildingTypeData _CurrentSelected;
	private ListView _ListView;
	private TwoPaneSplitView _SplitView;
	private VisualElement _RightPanel;


	private void OnEnable()
	{
		EditorApplication.projectChanged += OnProjectChanged;
	}

	private void OnDisable()
	{
		EditorApplication.projectChanged -= OnProjectChanged;
	}

	public void CreateGUI()
	{
		VisualElement root = rootVisualElement;

		// Instantiate UXML.
		VisualElement labelFromUxml = _VisualTreeAsset.Instantiate();
		root.Add(labelFromUxml);

		// Left/Right panels.
		_SplitView = new TwoPaneSplitView(0, 250,
										  TwoPaneSplitViewOrientation.Horizontal);
		root.Add(_SplitView);

		var leftPanel = root.Q<VisualElement>("LeftPanel");
		_RightPanel = root.Q<VisualElement>("RightPanel");
		_SplitView.Add(leftPanel);
		_SplitView.Add(_RightPanel);

		// Load all BuildingTypeData assets.
		BuildBuildingList();

		// ListView setup.
		_ListView = leftPanel.Q<ListView>();
		_ListView.itemsSource = _BuildingTypeDatas;

		_ListView.bindItem = (element, index) =>
		{
			BuildingTypeData data = _BuildingTypeDatas[index];
			element.Q<Label>().text = data.Name;
			var tile = data.Visual as Tile;
			element.Q<Image>().sprite = tile?.sprite;
		};
		_ListView.RegisterCallback<KeyDownEvent>(evt =>
		{
			if (!_CurrentSelected) return;
			if (evt.keyCode is not (KeyCode.Delete or KeyCode.Backspace)) return;
			DeleteSelectedBuilding();
			evt.StopPropagation();
		});

		// New building button.
		var newBtn = leftPanel.Q<ToolbarButton>("NewBtn");
		newBtn.clicked += () =>
		{
			_CurrentSelected = null;
			_ListView.ClearSelection();
			PopulateRightPanel();
		};

		// Delete building button.
		var deleteBtn = leftPanel.Q<ToolbarButton>("DeleteBtn");
		deleteBtn.clicked += DeleteSelectedBuilding;

		// Handle selection.
		_ListView.selectionChanged += objects =>
		{
			if (objects.FirstOrDefault() is not BuildingTypeData selected) return;
			_CurrentSelected = selected;
			deleteBtn.SetEnabled(_CurrentSelected);
			PopulateRightPanel();
		};

		// Initially populate right panel with no existing building.
		PopulateRightPanel();
	}

	[MenuItem("Tools/Pixel Civ/Building Creator")]
	public static void DisplayWindow()
	{
		var wnd = GetWindow<BuildingCreator>();
		wnd.titleContent = new GUIContent("Building Creator");

		// Limit size of the window.
		wnd.minSize = new Vector2(620, 200);
	}

	private void BuildBuildingList()
	{
		_BuildingTypeDatas.Clear();

		string[] guids = AssetDatabase.FindAssets($"t:{nameof(BuildingTypeData)}");
		foreach (string guid in guids)
		{
			string assetPath = AssetDatabase.GUIDToAssetPath(guid);
			var obj = AssetDatabase.LoadAssetAtPath<BuildingTypeData>(assetPath);
			_BuildingTypeDatas.Add(obj);
		}

		if (_BuildingTypeDatas.Count == 0)
		{
			_SplitView.style.display = DisplayStyle.None;
			_SplitView.SetEnabled(false);
			rootVisualElement.Add(_RightPanel);
		}
		else
		{
			_SplitView.style.display = DisplayStyle.Flex;
			_SplitView.Add(_RightPanel);
			_SplitView.SetEnabled(true);
		}
	}

	private TemplateContainer CreateElement(ScrollView scrollView,
			VisualTreeAsset template)
	{
		// Create element from a template and add the required functionality to
		// it's "Remove" button.
		TemplateContainer element = template.Instantiate();
		var button = element.Q<Button>();
		button.clicked -= null;
		button.clicked += () => { element.RemoveFromHierarchy(); };
		scrollView.Add(element);
		return element;
	}

	private void DeleteSelectedBuilding()
	{
		if (!_CurrentSelected) return;

		string path = AssetDatabase.GetAssetPath(_CurrentSelected);
		if (string.IsNullOrEmpty(path)) return;

		if (!EditorUtility.DisplayDialog("Delete Building",
										 $"Delete '{_CurrentSelected.Name}'?\nThis cannot be undone.",
										 "Delete", "Cancel"))
			return;

		Undo.DestroyObjectImmediate(_CurrentSelected);
		AssetDatabase.DeleteAsset(path);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();

		_CurrentSelected = null;
	}

	private void OnProjectChanged()
	{
		RebuildBuildingList();
	}

	// Populate right panel with a selected building
	private void PopulateRightPanel()
	{
		VisualElement root = _RightPanel;
		root.Clear();

		TemplateContainer template = _SettingsTemplate.Instantiate();
		root.Add(template);

		var production = root.Q<Foldout>("Production");
		var productionView = production.Q<ScrollView>();
		var restrictions = root.Q<Foldout>("Restrictions");
		var restrictionsView = restrictions.Q<ScrollView>();
		var saveAssetFolderPath = root.Q<TextField>("FolderSavePath");
		saveAssetFolderPath.value = _DefaultFolderPath;

		// Fill fields with building data if a building was provided,
		// otherwise just show the default.
		if (_CurrentSelected)
		{
			root.Q<TextField>("NameField").value = _CurrentSelected.Name;
			root.Q<ObjectField>("Visuals").value = _CurrentSelected.Visual;
			root.Q<EnumField>("Type").value = _CurrentSelected.Type;
			root.Q<EnumField>("Category").value = _CurrentSelected.Category;
			root.Q<UnsignedIntegerField>("Health").value = (uint)_CurrentSelected.Health;
			root.Q<UnsignedIntegerField>("Defence").value =
					(uint)_CurrentSelected.Defence;
			root.Q<UnsignedIntegerField>("AttackPower").value =
					(uint)_CurrentSelected.AttackPower;
			root.Q<ObjectField>("PreviousTier").value = _CurrentSelected.PreviousTier;
			root.Q<ObjectField>("NextTier").value = _CurrentSelected.NextTier;


			// Populate Production and Restrictions scroll views
			foreach (KeyValuePair<ResourceType, float> valuePair in _CurrentSelected
							 .Production)
			{
				TemplateContainer element = CreateElement(productionView,
														  _ProductionElementTemplate);
				element.Q<EnumField>().value = valuePair.Key;
				element.Q<FloatField>().value = valuePair.Value;
			}

			foreach (BuildingRestriction restriction in _CurrentSelected.Restrictions)
			{
				TemplateContainer element = CreateElement(restrictionsView,
														  _RestrictionElementTemplate);
				element.Q<EnumField>().value = restriction;
			}

			// Get the folder containing the asset.
			string folderPath = Path.GetDirectoryName(
					AssetDatabase.GetAssetPath(_CurrentSelected));
			saveAssetFolderPath.value = folderPath;
		}

		// Production create element button.
		var productionBtn = production.Q<Button>();
		productionBtn.clicked -= null;
		productionBtn.clicked += () =>
		{
			CreateElement(productionView, _ProductionElementTemplate);
		};

		// Restrictions create element button.
		var restrictionBtn = restrictions.Q<Button>();
		restrictionBtn.clicked -= null;
		restrictionBtn.clicked += () =>
		{
			CreateElement(restrictionsView, _RestrictionElementTemplate);
		};

		// Save button.
		var saveBtn = root.Q<Button>("FinalCreateBtn");
		saveBtn.clicked -= null;
		saveBtn.clicked += () => UpdateAssetDatabase(root, _CurrentSelected);
	}

	private void RebuildBuildingList()
	{
		BuildBuildingList();

		_ListView.Rebuild();

		// Handle deleted currently-selected asset.
		if (_CurrentSelected && _BuildingTypeDatas.Contains(_CurrentSelected))
			return;

		_CurrentSelected = null;
		_ListView.ClearSelection();
		// Clear the build settings.
		PopulateRightPanel();
	}

	private void UpdateAssetDatabase(VisualElement root,
			[CanBeNull] BuildingTypeData building = null)
	{
		// Update the "BuildingTypeDataParams" object values.
		BuildingTypeDataParams buildingParams = new()
		{
				Name = root.Q<TextField>("NameField").value,
				Visual = root.Q<ObjectField>("Visuals").value as TileBase,
				Type = (BuildingType)root.Q<EnumField>("Type").value,
				Category = (BuildingCategory)root.Q<EnumField>("Category").value,
				Health = (int)root.Q<UnsignedIntegerField>("Health").value,
				Defence = (int)root.Q<UnsignedIntegerField>("Defence").value,
				AttackPower = (int)root.Q<UnsignedIntegerField>("AttackPower").value,
				PreviousTier =
						root.Q<ObjectField>("PreviousTier").value as BuildingTypeData,
				NextTier = root.Q<ObjectField>("NextTier").value as BuildingTypeData,
		};

		// Get the productions.
		Dictionary<ResourceType, float> production = new();
		var scrollView = root.Q<Foldout>("Production").Q<ScrollView>();
		foreach (VisualElement child in scrollView.Children())
		{
			var resourceType = (ResourceType)child.Q<EnumField>().value;
			float amount = child.Q<FloatField>().value;

			production.Add(resourceType, amount);
		}

		// Get the restrictions
		scrollView = root.Q<Foldout>("Restrictions").Q<ScrollView>();
		List<BuildingRestriction> restrictions =
				scrollView.Children()
						  .Select(n => (BuildingRestriction)n.Q<EnumField>().value)
						  .ToList();

		// Update the "BuildingTypeDataParams" object values.
		buildingParams.Production = production;
		buildingParams.Restrictions = restrictions;

		// If an asset was provided to be updated, update it and return.
		if (building)
		{
			Undo.RecordObject(building, $"Update {nameof(BuildingTypeData)} Asset");
			building.Init(buildingParams);
			// Update the asset in the project.
			EditorUtility.SetDirty(building);
			AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(building),
									  $"{buildingParams.Name}Data");
			AssetDatabase.SaveAssetIfDirty(building);
			AssetDatabase.Refresh();
			return;
		}

		// Handle saving the final created "BuildingTypeData" asset.
		string folderPath = root.Q<TextField>("FolderSavePath").value;
		if (!Utils.EnsureFolderPathExists(folderPath)) return;

		var buildingDataAsset = CreateInstance<BuildingTypeData>();
		Undo.RecordObject(buildingDataAsset, $"Create {nameof(BuildingTypeData)} Asset");

		var assetPath = $"{folderPath}/{buildingParams.Name}Data.asset";
		buildingDataAsset.Init(buildingParams);

		// Create the asset in the project.
		AssetDatabase.CreateAsset(buildingDataAsset, assetPath);
		AssetDatabase.SaveAssetIfDirty(buildingDataAsset);
		AssetDatabase.Refresh();

		// Add the newly created asset to the left panel and select it.
		EditorApplication.delayCall += () =>
		{
			RebuildBuildingList();

			int index = _BuildingTypeDatas.IndexOf(buildingDataAsset);
			if (index < 0) return;
			_ListView.SetSelection(index);
			_CurrentSelected = buildingDataAsset;
		};
	}
}
