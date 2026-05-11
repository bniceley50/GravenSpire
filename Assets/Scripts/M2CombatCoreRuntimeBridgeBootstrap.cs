#nullable enable

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gravenspire.UnityRuntime.Combat
{
    public static class M2CombatCoreRuntimeBridgeBootstrap
    {
        public const string BridgeObjectName = "M2_CombatCoreRuntimeBridge";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void BootstrapAfterSceneLoad()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            EnsureBridge();
        }

        public static M2CombatCoreRuntimeBridge EnsureBridge()
        {
            var existing = Object.FindFirstObjectByType<M2CombatCoreRuntimeBridge>();
            if (existing != null)
            {
                return existing;
            }

            var bridgeObject = new GameObject(BridgeObjectName);
            Object.DontDestroyOnLoad(bridgeObject);
            return bridgeObject.AddComponent<M2CombatCoreRuntimeBridge>();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureBridge();
        }
    }
}
