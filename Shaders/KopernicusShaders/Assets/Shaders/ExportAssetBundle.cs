using UnityEditor;
using System;
using System.Linq;
using System.IO;
using System.Collections;

public class CreateAssetBundles
{
	[MenuItem ("Assets/Build AssetBundles")]
	static void BuildAllAssetBundles ()
	{
		
		var outDir = "Assets/CompiledAssetBundles";
		if (!Directory.Exists (outDir))
			Directory.CreateDirectory (outDir);
		
		var opts = BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.ForceRebuildAssetBundle;
		BuildTarget[] platforms = { BuildTarget.StandaloneWindows, BuildTarget.StandaloneOSXUniversal, BuildTarget.StandaloneLinux };
		string[] platformExts = { "-windows", "-macosx", "-linux" };

		for (var i = 0; i < platforms.Length; ++i)
		{
			BuildPipeline.BuildAssetBundles(outDir, opts, platforms[i]);
			var outFile = outDir + "/kopernicusshaders" + platformExts[i];
			FileUtil.ReplaceFile(outDir + "/kopernicusshaders", outFile);
		}

		//cleanup
		foreach (string file in Directory.GetFiles(outDir, "*.*").Where(item => (item.EndsWith(".meta") || item.EndsWith(".manifest"))))
		{
				File.Delete(file);
		}
		File.Delete (outDir + "/CompiledAssetBundles");
		File.Delete(outDir+"/kopernicusshaders");
	}
}