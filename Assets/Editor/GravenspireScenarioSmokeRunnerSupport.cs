#if UNITY_EDITOR
#nullable enable

namespace Gravenspire.Editor
{
    /// <summary>
    /// Shared editor-only helpers for Gravenspire scenario-smoke verification
    /// runners (S2-M2-01 through S2-M2-04 today, extending to S2-M3-* runners
    /// as the M3 slate lands).
    ///
    /// Owns the diagnosed editor-startup noise filter so each scenario runner
    /// does not duplicate the same stack-trace match. See
    /// <c>tests/evidence/S2-M2-04/verification.md</c> Runtime Notes for the
    /// original diagnosis of the <c>UnityEditor.Search.SearchInit</c> exception
    /// thrown on batchmode launch of a fresh worktree with no default Search
    /// database asset.
    /// </summary>
    public static class GravenspireScenarioSmokeRunnerSupport
    {
        /// <summary>
        /// Returns true when the captured log entry's stack trace is diagnosed
        /// Unity editor-startup noise that must not fail a scenario-smoke run.
        ///
        /// Callers are expected to invoke this only for error-class log types
        /// (Error / Exception / Assert); warning-class logs are passed through
        /// to the runner's warning channel regardless.
        ///
        /// Currently filters:
        /// - <c>UnityEditor.Search.SearchInit.IndexationOnStartup</c> throws
        ///   <c>ArgumentOutOfRangeException</c> on batchmode launch of a fresh
        ///   worktree (no default Search database asset). The entire stack
        ///   trace is within <c>UnityEditor.Search</c> and
        ///   <c>EditorApplication.Internal_CallDelayFunctions</c> — outside
        ///   the scenario-smoke runtime, the loop controller, and the runner.
        /// </summary>
        public static bool IsDiagnosedEditorStartupNoise(string stackTrace)
        {
            return !string.IsNullOrEmpty(stackTrace)
                && stackTrace.Contains("UnityEditor.Search.SearchInit");
        }
    }
}
#endif
