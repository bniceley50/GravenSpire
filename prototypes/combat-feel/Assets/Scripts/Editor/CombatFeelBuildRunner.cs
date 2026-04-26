// PROTOTYPE - NOT FOR PRODUCTION
// Question: Can Cleric tab-target combat, slow cast cadence, mana pressure, and med-break recovery make the silence between pulls feel intentional rather than empty?
// Date: 2026-04-26

#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Gravenspire.Prototypes.CombatFeel.Editor
{
    public static class CombatFeelBuildRunner
    {
        private const string ScenePath = "Assets/CombatFeelPrototype.unity";
        private const string BuildDirectory = "Builds/CombatFeelPrototype";
        private const string ExecutableName = "CombatFeelPrototype.exe";

        [MenuItem("Gravenspire/Prototypes/Combat Feel/Build Windows Player")]
        public static void BuildWindowsPlayer()
        {
            if (!File.Exists(ScenePath))
            {
                CombatFeelSceneBuilder.CreatePrototypeScene();
                AssetDatabase.Refresh();
            }

            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            if (string.IsNullOrWhiteSpace(projectRoot))
            {
                throw new InvalidOperationException("Could not resolve combat-feel prototype project root.");
            }

            var outputDirectory = Path.Combine(projectRoot, BuildDirectory);
            Directory.CreateDirectory(outputDirectory);

            var outputPath = Path.Combine(outputDirectory, ExecutableName);
            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = outputPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            };

            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException($"Combat feel Windows build failed: {report.summary.result}");
            }

            Debug.Log($"Combat feel Windows build created at {outputPath}.");
        }
    }
}
#endif
