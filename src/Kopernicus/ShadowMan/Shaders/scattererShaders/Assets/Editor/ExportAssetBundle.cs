using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.IO;
using System.Collections;

namespace scattererShaders
{

	public class CreateAssetBundles
	{
		[MenuItem ("Assets/Build AssetBundles for release")]
		static void BuildAllAssetBundles ()
		{
			BuildTarget[] platforms = { BuildTarget.StandaloneWindows, BuildTarget.StandaloneOSX, BuildTarget.StandaloneLinux64 };
			string[] platformExts = { "-windows", "-macosx", "-linux" };

			BuildBundles(platforms, platformExts);
		}

		[MenuItem ("Assets/Build AssetBundles for local openGL")]
		static void BuildOpenGL ()
		{
			BuildTarget[] platforms = {BuildTarget.StandaloneLinux64};
			string[] platformExts = { "-linux"};

			BuildBundles(platforms, platformExts);
		}

		[MenuItem ("Assets/Build AssetBundles for local dx11")]
		static void BuildDx11 ()
		{
			BuildTarget[] platforms = {BuildTarget.StandaloneWindows};
			string[] platformExts = { "-windows"};

			BuildBundles(platforms, platformExts);
		}


		static void BuildBundles (BuildTarget[] platforms, string[] platformExts)
		{
			// Put the bundles in a folder called "AssetBundles"
			//var outDir = "Assets/AssetBundles";
			var outDir = "C:/Steam/steamapps/common/Kerbal Space Program/GameData/scatterer/shaders";
			var outDir2 = "C:/Steam/steamapps/common/Kerbal Space Program 1.9/GameData/scatterer/shaders";

			if (!Directory.Exists (outDir))
				Directory.CreateDirectory (outDir);

			if (!Directory.Exists (outDir2))
				Directory.CreateDirectory (outDir2);

			var opts = BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.ForceRebuildAssetBundle;

			for (var i = 0; i < platforms.Length; ++i)
			{
				BuildPipeline.BuildAssetBundles(outDir, opts, platforms[i]);
				var outFile  = outDir  + "/scatterershaders" + platformExts[i];
				var outFile2 = outDir2 + "/scatterershaders" + platformExts[i];
				FileUtil.ReplaceFile(outDir  + "/scatterershaders", outFile);
				FileUtil.ReplaceFile(outDir  + "/scatterershaders", outFile2);
			}

			//cleanup
			foreach (string file in Directory.GetFiles(outDir, "*.*").Where(item => (item.EndsWith(".meta") || item.EndsWith(".manifest"))))
			{
				File.Delete(file);
			}
			File.Delete (outDir + "/CompiledAssetBundles");
			File.Delete(outDir+"/scatterershaders");
			File.Delete(outDir+"/shaders");
		}
	}

}