// This file copied from TUFX and used under GPL v3 license
// Copyright (C) 2020  Shadowmage (John Cummens)

using UnityEngine;
using UnityEditor;
using System.Collections;

public class AssetBundleCompiler
{
	[MenuItem("Assets/Build Selected AssetBundle Win64")]
	static void BuildAssetBundleWin64()
	{
		exportAssetBundle(BuildTarget.StandaloneWindows64);
	}

	[MenuItem("Assets/Build Selected AssetBundle OSX")]
	static void BuildAssetBundleOSX()
	{
		exportAssetBundle(BuildTarget.StandaloneOSX);
	}

	[MenuItem("Assets/Build Selected AssetBundle Linux")]
	static void BuildAssetBundleLinux()
	{
		exportAssetBundle(BuildTarget.StandaloneLinux64);
	}

	private static void exportAssetBundle(BuildTarget target)
	{
		string directory = string.Empty;
		string name = string.Empty;
		string path = EditorUtility.SaveFilePanel("Build Asset Bundle", "Assets", "NewAssetBundle", "assetbundle");
		try
		{
			directory = path.Substring(0, path.LastIndexOf('/'));
			name = path.Substring(path.LastIndexOf('/') + 1);
		}
		catch
		{
			return;
		}
		Object[] selection = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
		AssetBundleBuild build = new AssetBundleBuild();
		build.assetBundleName = name;
		build.assetNames = new string[selection.Length];
		int len = selection.Length;
		for (int i = 0; i < len; i++)
		{
			build.assetNames[i] = AssetDatabase.GetAssetPath((UnityEngine.Object)selection[i]);
			MonoBehaviour.print("Building asset: " + build.assetNames[i]);
		}
		BuildPipeline.BuildAssetBundles(directory, new AssetBundleBuild[] { build }, BuildAssetBundleOptions.ForceRebuildAssetBundle, target);
		MonoBehaviour.print("Renaming from: " + path + " to: " + path.Replace(".assetbundle", ""));
		System.IO.File.Move(path, path.Replace(".assetbundle", ""));
	}

}
