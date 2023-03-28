using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace uprt.editor
{
    /// <summary>
    /// UPRT Unity용 클래스
    /// </summary>
    public partial class UPRTPackageManager
    {
        [MenuItem("UPRT/Builder/Win64")]
        /// <summary>
        /// 윈도우 빌드
        /// </summary>
        public static void BuildWin64()
        {
            BuildPlayerOptions buildOptions = new BuildPlayerOptions();
            buildOptions.scenes = GetScenesToString();
            buildOptions.locationPathName = $"Build/Build.exe";
            buildOptions.target = BuildTarget.StandaloneWindows64;
            buildOptions.options = BuildOptions.None;
            StartBuild(buildOptions);
        }

        private static string[] GetScenesToString()
        {
            List<string> scenes = new List<string>();

            foreach(EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (!scene.enabled) continue;
                scenes.Add(scene.path);
            }
            return scenes.ToArray();
        }

        private static void StartBuild(BuildPlayerOptions bpo)
        {
            BuildReport report = BuildPipeline.BuildPlayer(bpo);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
                Debug.Log($"Build succeeded : {summary.totalSize} bytes");
            else if (summary.result == BuildResult.Failed)
                Debug.LogError("Build failed");
            else if (summary.result == BuildResult.Cancelled)
                Debug.LogWarning("Build canceled");
            else
                Debug.LogError("Build failed by unknown issues");
        }
    }
}

