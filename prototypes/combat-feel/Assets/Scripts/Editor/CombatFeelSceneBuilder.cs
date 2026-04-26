// PROTOTYPE - NOT FOR PRODUCTION
// Question: Can Cleric tab-target combat, slow cast cadence, mana pressure, and med-break recovery make the silence between pulls feel intentional rather than empty?
// Date: 2026-04-26

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gravenspire.Prototypes.CombatFeel.Editor
{
    public static class CombatFeelSceneBuilder
    {
        [MenuItem("Gravenspire/Prototypes/Combat Feel/Create Prototype Scene")]
        public static void CreatePrototypeScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "CombatFeelPrototype";

            var cameraObject = new GameObject("Prototype Camera");
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.035f, 0.032f, 0.03f, 1f);
            camera.transform.position = new Vector3(0f, 1.2f, -8f);
            camera.transform.rotation = Quaternion.identity;
            cameraObject.AddComponent<AudioListener>();

            var bootstrap = new GameObject("Prototype Bootstrap");
            bootstrap.AddComponent<PrototypeBootstrap>();

            var scenePath = "Assets/CombatFeelPrototype.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            Selection.activeGameObject = bootstrap;
            EditorGUIUtility.PingObject(bootstrap);
            Debug.Log($"Combat feel prototype scene created at {scenePath}.");
        }
    }
}
#endif
