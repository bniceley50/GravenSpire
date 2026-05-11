#nullable enable

using System;
#if NET5_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif

namespace Gravenspire.Gameplay.Combat;

internal static class CombatArgumentNull
{
#if NET5_0_OR_GREATER
    public static void ThrowIfNull(object? value, [CallerArgumentExpression("value")] string? parameterName = null)
#else
    public static void ThrowIfNull(object? value, string? parameterName = null)
#endif
    {
        if (value is null)
        {
            throw new ArgumentNullException(parameterName);
        }
    }
}
