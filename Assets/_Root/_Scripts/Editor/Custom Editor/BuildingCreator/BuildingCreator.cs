using System;
using System.Collections.Generic;
using System.Linq;
using PixelCiv.Editor.Utilities;
using PixelCiv.Scriptable_Objects;
using PixelCiv.Utilities.Types;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

public class BuildingCreator : EditorWindow
{
	[SerializeField]
	private VisualTreeAsset _VisualTreeAsset;
	[SerializeField]
	private VisualTreeAsset _ProductionElementTemplate;
	[SerializeField]
	private VisualTreeAsset _RestrictionElementTemplate;


	public void CreateGUI()
	{
		VisualElement root = rootVisualElement;

		// Instantiate UXML.
		VisualElement labelFromUxml = _VisualTreeAsset.Instantiate();
		root.Add(labelFromUxml);


		// Setup Production list field "Create" button.
		var productionField = root.Q<Foldout>("Production");
		var productionCreateBtn = productionField.Q<Button>("CreateBtn");
		productionCreateBtn.clicked += () =>
		{
			var scrollView = productionField.Q<ScrollView>();
			CreateElement(scrollView, _ProductionElementTemplate);
		};

		// Setup Restrictions list field "Create" button.
		var restrictionsField = root.Q<Foldout>("Restrictions");
		var restrictionsCreateBtn = restrictionsField.Q<Button>("CreateBtn");
		restrictionsCreateBtn.clicked += () =>
		{
			var scrollView = restrictionsField.Q<ScrollView>();
			CreateElement(scrollView, _RestrictionElementTemplate);
		};

		// Set up the Create button.
		var createBtn = root.Q<Button>("FinalCreateBtn");
		createBtn.clicked += CreateBuildingTypeDataObj;
	}

	[MenuItem("Tools/Pixel Civ/Building Creator")]
	public static void ShowExample()
	{
		var wnd = GetWindow<BuildingCreator>();
		wnd.titleContent = new GUIContent("Building Creator");

		// Limit size of the window.
		wnd.minSize = new Vector2(620, 200);
	}

	private void CreateBuildingTypeDataObj()
	{
		VisualElement root = rootVisualElement;
		BuildingTypeDataParams buildingParams = new();

		// Get required field.
		string buildingName = root.Q<TextField>("NameField").value;
		Object buildingVisuals = root.Q<ObjectField>("Visuals").value;
		Enum buildingType = root.Q<EnumField>("Type").value;
		Enum buildingCategory = root.Q<EnumField>("Category").value;
		uint buildingHealth = root.Q<UnsignedIntegerField>("Health").value;
		uint buildingDefence = root.Q<UnsignedIntegerField>("Defence").value;
		uint buildingAttack = root.Q<UnsignedIntegerField>("AttackPower").value;
		var buildingProduction = root.Q<Foldout>("Production");
		var buildingRestrictions = root.Q<Foldout>("Restrictions");
		Object buildingPreviousTier = root.Q<ObjectField>("PreviousTier").value;
		Object buildingNextTier = root.Q<ObjectField>("NextTier").value;
		string folderPath = root.Q<TextField>("FolderSavePath").value;

		// Update the "BuildingTypeDataParams" object values.
		buildingParams.Name = buildingName;
		buildingParams.Visual = buildingVisuals as TileBase;
		buildingParams.Type = (BuildingType)buildingType;
		buildingParams.Category = (BuildingCategory)buildingCategory;
		buildingParams.Health = (int)buildingHealth;
		buildingParams.Defence = (int)buildingDefence;
		buildingParams.AttackPower = (int)buildingAttack;
		buildingParams.PreviousTier = buildingPreviousTier as BuildingTypeData;
		buildingParams.NextTier = buildingNextTier as BuildingTypeData;

		// Get the productions.
		Dictionary<ResourceType, float> production = new();
		var scrollView = buildingProduction.Q<ScrollView>();

		foreach (VisualElement child in scrollView.Children())
		{
			var resourceType = (ResourceType)child.Q<EnumField>().value;
			float amount = child.Q<FloatField>().value;

			production.Add(resourceType, amount);
		}

		// Get the restrictions
		scrollView = buildingRestrictions.Q<ScrollView>();
		List<BuildingRestriction> restrictions =
				scrollView.Children()
						  .Select(n => (BuildingRestriction)n.Q<EnumField>().value)
						  .ToList();

		// Update the "BuildingTypeDataParams" object values.
		buildingParams.Production = production;
		buildingParams.Restrictions = restrictions;

		// Handle saving the final created "BuildingTypeData" asset.
		if (!Utils.EnsureFolderPathExists(folderPath)) return;

		var assetPath = $"{folderPath}/{buildingParams.Name}Data.asset";
		var building = CreateInstance<BuildingTypeData>();
		building.Init(buildingParams);

		// Create the asset in the project.
		AssetDatabase.CreateAsset(building, assetPath);
		AssetDatabase.SaveAssetIfDirty(building);
		AssetDatabase.Refresh();
	}

	private void CreateElement(ScrollView scrollView, VisualTreeAsset template)
	{
		// Create element from a template and add the required functionality to
		// it's "Remove" button.
		TemplateContainer element = template.Instantiate();
		element.Q<Button>().clicked += () => { element.RemoveFromHierarchy(); };

		scrollView.Add(element);
	}
}
