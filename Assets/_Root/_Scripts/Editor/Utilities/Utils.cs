using UnityEditor;
using UnityEngine;

namespace PixelCiv.Editor.Utilities
{
internal static class Utils
{
	public static bool EnsureFolderPathExists(string folderPath)
	{
		if (AssetDatabase.IsValidFolder(folderPath))
			return true;

		if (!folderPath.Contains("Assets/"))
		{
			Debug.LogError("The provided folder path for the " +
						   "Building Creator is Invalid! The path needs to " +
						   "start with the \"Assets/\" folder.");
			return false;
		}

		// Try and create the missing folders.
		string[] folders = folderPath.Split('/');
		string currentPath = folders[0];

		for (var i = 1; i < folders.Length; i++)
		{
			string nextFolder = folders[i];
			var combined = $"{currentPath}/{nextFolder}";

			// Create folder under currentPath if it doesn't already exist.
			if (!AssetDatabase.IsValidFolder(combined))
				AssetDatabase.CreateFolder(currentPath, nextFolder);

			currentPath = combined;
		}

		return true;
	}
}
}
