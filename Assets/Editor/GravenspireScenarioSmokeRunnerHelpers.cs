#if UNITY_EDITOR
#nullable enable

using UnityEngine;

namespace Gravenspire.Editor
{
    /// <summary>
    /// Shared helpers for Gravenspire scenario-smoke verification runners
    /// (Sprint 2 M2 + M3 and beyond). Concentrates cross-runner editor-mode
    /// utilities so new scenarios inherit them without re-implementing
    /// per-runner logic.
    /// </summary>
    public static class GravenspireScenarioSmokeRunnerHelpers
    {
        /// <summary>
        /// True when a captured editor log entry matches a diagnosed Unity
        /// editor-startup noise condition that must not be allowed to false-fail
        /// a scenario smoke. Runners that subscribe to
        /// <see cref="Application.logMessageReceived"/> should call this at the
        /// top of their <c>CaptureLog</c> body and <c>return</c> when it returns
        /// <c>true</c>.
        /// </summary>
        /// <remarks>
        /// Currently filtered conditions:
        /// - <c>UnityEditor.Search.SearchInit.IndexationOnStartup</c> throws
        ///   <c>ArgumentOutOfRangeException</c> on batchmode launch of a fresh
        ///   worktree with no default Search database asset. The exception is
        ///   a Unity editor-startup delay-function side effect outside any
        ///   Gravenspire scenario runtime, controller, or runner and would
        ///   false-fail an otherwise clean smoke.
        ///
        /// To add a future filter: extend the body with another branch and
        /// document why the condition is safe to suppress, including a
        /// reference to the runner evidence or carryover that diagnosed it.
        /// </remarks>
        public static bool IsEditorStartupNoise(string condition, string stackTrace, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(stackTrace) && stackTrace.Contains("UnityEditor.Search.SearchInit"))
            {
                return true;
            }

            return false;
        }
    }
}

#endif
