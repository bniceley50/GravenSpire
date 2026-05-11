#nullable enable

#if !NET5_0_OR_GREATER
namespace System.Runtime.CompilerServices;

/// <summary>
/// Unity's netstandard compile profile needs this marker for records and init-only setters.
/// </summary>
internal static class IsExternalInit
{
}
#endif
