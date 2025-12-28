using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace PixelCiv.Editor.Utilities
{
public static class AssemblyDefUpdater
{
	private const string _HISTORY_FILE_NAME = "LastProductName.txt";

	[MenuItem("Tools/Update Assembly Definitions and Namespaces")]
	public static void UpdateAssemblyDefinitions()
	{
		// Remove whitespaces from product name.
		string productName = PlayerSettings.productName.Replace(" ",
				string.Empty);

		const string ROOT_FOLDER_PATH = "Assets/_Root";
		// Construct a path to the product name history file.
		string historyPath = Path.Combine(Application.dataPath,
										"_Root/_Scripts/Editor",
										_HISTORY_FILE_NAME);

		if (!Directory.Exists(ROOT_FOLDER_PATH))
		{
			Debug.LogError($"The folder {ROOT_FOLDER_PATH} does not exist!");
			return;
		}

		// List of filtered assembly definitions.
		string[] asmdefFiles = Directory.GetFiles(ROOT_FOLDER_PATH, "*.asmdef",
												SearchOption.AllDirectories);
		asmdefFiles =
			Array.FindAll(asmdefFiles, file => !ShouldIgnorePath(file));

		// Load an old name if "LastProductName.txt" exits,
		// if not, infer the namespace.
		string oldName;
		if (File.Exists(historyPath))
			oldName = File.ReadAllText(historyPath).Trim();
		else
		{
			// If namespaces already match, nothing to do.
			string inferredName = InferRootNamespace(asmdefFiles);
			if (string.IsNullOrEmpty(inferredName) ||
				inferredName == productName)
			{
				Debug.Log("Namespaces already matches the product name.");
				return;
			}

			// Treat the inferred name as the old name.
			oldName = inferredName;
		}

		// List of filtered scripts.
		string[] scriptFiles = Directory.GetFiles(ROOT_FOLDER_PATH, "*.cs",
												SearchOption.AllDirectories);
		scriptFiles = Array.FindAll(scriptFiles,
									file => !ShouldIgnorePath(file));

		var changedAsmDefs = new ConcurrentBag<string>();
		var changedScripts = new ConcurrentBag<string>();
		var anyChanges = false;

		// --- Parallel Update of Assembly Definitions ---
		Parallel.ForEach(asmdefFiles, file =>
		{
			if (ShouldIgnorePath(file))
				return;

			string[] lines = File.ReadAllLines(file);
			var changed = false;

			for (var i = 0; i < lines.Length; i++)
			{
				string trimmed = lines[i].TrimStart();
				if (!trimmed.StartsWith("\"rootNamespace\""))
					continue;

				// Find the "rootNamespace" field.
				Match match = Regex.Match(lines[i],
										@"(""rootNamespace""\s*:\s*"")(.*?)(\"")");

				if (!match.Success)
					continue;

				// Once found check its values and update it.
				string currentNamespace = match.Groups[2].Value;
				if (!currentNamespace.Contains(oldName))
					continue;

				string newNamespace = currentNamespace.Replace(oldName,
						productName);
				lines[i] = match.Groups[1].Value +
							newNamespace +
							match.Groups[3].Value + ",";
				changed = true;
			}

			if (!changed)
				return;

			File.WriteAllLines(file, lines);
			changedAsmDefs.Add(file);
		});

		// --- Parallel Update of Scripts ---
		Parallel.ForEach(scriptFiles, file =>
		{
			if (ShouldIgnorePath(file))
				return;

			string content = File.ReadAllText(file);
			string originalContent = content;
			var changed = false;

			// Replace old name in USING statements.
			var usingPattern = new Regex(@"using\s+(?:\w+\s*=\s*)?([\w\.]+);");
			content = usingPattern.Replace(content, match =>
			{
				string value = match.Groups[1].Value;
				if (!value.Contains(oldName))
					return match.Value;

				changed = true;
				string replaced = value.Replace(oldName, productName);
				// Preserve alias if present.
				return match.Value.Replace(value, replaced);
			});

			// Replace in namespace declarations.
			Match namespaceMatch = Regex.Match(content,
												@"namespace\s+([\w\.]+)");
			if (namespaceMatch.Success)
			{
				string Value = namespaceMatch.Groups[1].Value;
				if (Value.Contains(oldName))
				{
					string updatedNs = Value.Replace(oldName, productName);
					content = content.Replace(Value, updatedNs);
					changed = true;
				}
			}

			if (!changed || content == originalContent)
				return;
			File.WriteAllText(file, content);
			changedScripts.Add(file);
		});

		// Store the new product name.
		Directory.CreateDirectory(Path.GetDirectoryName(historyPath) ??
								string.Empty);
		File.WriteAllText(historyPath, productName);

		// Log updates.
		foreach (string file in changedAsmDefs)
			Debug.Log($"Updated rootNamespace in: {file}");

		foreach (string file in changedScripts)
			Debug.Log($"Updated using statements and/or namespace in: {file}");

		// Refresh the product.
		anyChanges = changedAsmDefs.Count > 0 || changedScripts.Count > 0;
		if (anyChanges)
		{
			AssetDatabase.Refresh();
			Debug.Log("AssetDatabase refreshed.");
		}
		else
			Debug.Log("No changes detected.");
	}

	private static string InferRootNamespace(string[] asmdefFiles)
	{
		foreach (string file in asmdefFiles)
		{
			string text = File.ReadAllText(file);
			Match match = Regex.Match(text,
									@"""rootNamespace""\s*:\s*""([\w\.]+)""");

			if (!match.Success)
				continue;

			// Assume first segment is the product name.
			return match.Groups[1].Value.Split('.')[0];
		}

		return string.Empty;
	}

	// INFO: Hardcoded to ignore the anything todo with demos.
	private static bool ShouldIgnorePath(string path)
	{
		string lower = path.ToLowerInvariant();
		return lower.Contains("/demo") || lower.Contains("\\demo");
	}
}
}