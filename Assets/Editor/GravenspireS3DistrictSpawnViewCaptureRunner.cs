#if UNITY_EDITOR
#nullable enable

using System;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gravenspire.Editor
{
    public static class GravenspireS3DistrictSpawnViewCaptureRunner
    {
        private const string ScenePath = "Assets/Scenes/_DevEntry.unity";
        private const string OutputPathArgumentName = "-gravenspireScreenshotPath";
        private const int ScreenshotWidth = 1280;
        private const int ScreenshotHeight = 720;
        private const float FieldOfViewDegrees = 70.0f;
        private const float EyeHeightOffsetMeters = 0.7f;

        [MenuItem("Gravenspire/Capture S3 District Spawn View")]
        public static void Run()
        {
            var outputPath = ResolveOutputPathFromCommandLine(DefaultOutputPath());
            var exitCode = 1;

            try
            {
                Capture(outputPath);
                exitCode = 0;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                exitCode = 1;
            }
            finally
            {
                EditorApplication.Exit(exitCode);
            }
        }

        private static void Capture(string outputPath)
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                throw new IOException($"Failed to open scene at {ScenePath}.");
            }

            VerifyRequiredSceneObject("FirstDistrict_Greybox");
            VerifyRequiredSceneObject("Greybox_CaretakerHall_Massing");
            VerifyRequiredSceneObject("Greybox_CourtVendorHall_Massing");
            VerifyRequiredSceneObject("M3_Caretaker");

            var spawn = VerifyRequiredSceneObject("ClericShellMarker").transform.position;
            var cameraObject = new GameObject("S3_DistrictSpawnViewCaptureCamera")
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            RenderTexture? renderTexture = null;
            Texture2D? texture = null;

            try
            {
                var camera = cameraObject.AddComponent<Camera>();
                camera.transform.position = spawn + (Vector3.up * EyeHeightOffsetMeters);
                camera.transform.rotation = Quaternion.LookRotation(Vector3.back, Vector3.up);
                camera.fieldOfView = FieldOfViewDegrees;
                camera.aspect = (float)ScreenshotWidth / ScreenshotHeight;
                camera.nearClipPlane = 0.03f;
                camera.farClipPlane = 100.0f;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color32(0x90, 0x90, 0x90, 0xFF);
                camera.enabled = false;

                renderTexture = new RenderTexture(ScreenshotWidth, ScreenshotHeight, 24, RenderTextureFormat.ARGB32)
                {
                    antiAliasing = 1
                };
                texture = new Texture2D(ScreenshotWidth, ScreenshotHeight, TextureFormat.RGB24, mipChain: false);

                camera.targetTexture = renderTexture;
                var previousActive = RenderTexture.active;
                RenderTexture.active = renderTexture;
                try
                {
                    camera.Render();
                    texture.ReadPixels(new Rect(0, 0, ScreenshotWidth, ScreenshotHeight), 0, 0);
                    texture.Apply(updateMipmaps: false);
                }
                finally
                {
                    RenderTexture.active = previousActive;
                    camera.targetTexture = null;
                }

                Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? ".");
                File.WriteAllBytes(outputPath, texture.EncodeToPNG());
                Debug.Log($"S3-05 spawn-to-Caretaker discoverability screenshot wrote {outputPath}.");
            }
            finally
            {
                if (renderTexture != null)
                {
                    renderTexture.Release();
                    UnityEngine.Object.DestroyImmediate(renderTexture);
                }

                if (texture != null)
                {
                    UnityEngine.Object.DestroyImmediate(texture);
                }

                UnityEngine.Object.DestroyImmediate(cameraObject);
            }
        }

        private static GameObject VerifyRequiredSceneObject(string objectName)
        {
            var gameObject = FindSceneObjectIncludingInactive(objectName);
            if (gameObject == null)
            {
                throw new IOException($"{objectName} is missing from {ScenePath}.");
            }

            return gameObject;
        }

        private static GameObject? FindSceneObjectIncludingInactive(string objectName)
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                return null;
            }

            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var transform in root.GetComponentsInChildren<Transform>(includeInactive: true))
                {
                    if (transform.name == objectName)
                    {
                        return transform.gameObject;
                    }
                }
            }

            return null;
        }

        private static string ResolveOutputPathFromCommandLine(string defaultOutputPath)
        {
            var arguments = Environment.GetCommandLineArgs();
            for (var i = 0; i < arguments.Length - 1; i++)
            {
                if (string.Equals(arguments[i], OutputPathArgumentName, StringComparison.OrdinalIgnoreCase))
                {
                    return arguments[i + 1];
                }
            }

            return defaultOutputPath;
        }

        private static string DefaultOutputPath()
        {
            return Path.Combine(
                "tests",
                "evidence",
                "S3-05",
                $"spawn-to-caretaker-discoverability-{DateTimeOffset.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}.png");
        }
    }
}
#endif
