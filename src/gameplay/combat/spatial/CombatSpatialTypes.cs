#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace Gravenspire.Gameplay.Combat;

/// <summary>
/// Small immutable point type used by pure combat-domain spatial tests before Unity scene objects exist.
/// </summary>
public readonly record struct CombatPoint3(double X, double Y, double Z)
{
    /// <summary>
    /// Returns Euclidean distance in meters.
    /// </summary>
    public double DistanceTo(CombatPoint3 other)
    {
        return Math.Sqrt(DistanceSquaredTo(other));
    }

    /// <summary>
    /// Returns squared Euclidean distance in meters.
    /// </summary>
    public double DistanceSquaredTo(CombatPoint3 other)
    {
        var x = X - other.X;
        var y = Y - other.Y;
        var z = Z - other.Z;
        return (x * x) + (y * y) + (z * z);
    }

    /// <summary>
    /// Returns rounded millimeter distance for deterministic query sorting.
    /// </summary>
    public long DistanceMillimetersTo(CombatPoint3 other)
    {
        return (long)Math.Round(DistanceTo(other) * 1000d, MidpointRounding.AwayFromZero);
    }
}

/// <summary>
/// Logical T1 line-of-sight layers. Unity LayerMasks map onto these authored names later.
/// </summary>
[Flags]
public enum CombatLosLayer
{
    /// <summary>
    /// No layer.
    /// </summary>
    None = 0,

    /// <summary>
    /// World geometry that blocks combat LoS.
    /// </summary>
    WorldSolid = 1 << 0,

    /// <summary>
    /// Closed doors block combat LoS.
    /// </summary>
    ClosedDoor = 1 << 1,

    /// <summary>
    /// Large props block combat LoS.
    /// </summary>
    LargeProp = 1 << 2,

    /// <summary>
    /// Combat actors do not block combat LoS.
    /// </summary>
    CombatActor = 1 << 3,

    /// <summary>
    /// Trigger-only volumes do not block combat LoS.
    /// </summary>
    TriggerOnly = 1 << 4,

    /// <summary>
    /// Soft interactables do not block combat LoS.
    /// </summary>
    InteractableSoft = 1 << 5,

    /// <summary>
    /// Visual effects do not block combat LoS.
    /// </summary>
    Vfx = 1 << 6
}

/// <summary>
/// Anchor points used by targeting, body-pull, and social-assist LoS queries.
/// </summary>
public sealed record CombatSpatialAnchorSet(
    CombatPoint3 AggroOrigin,
    CombatPoint3 TargetPoint,
    string AggroOriginSource,
    string TargetPointSource);

/// <summary>
/// Diagnostic emitted when a bounded query buffer is full.
/// </summary>
public sealed record CombatQueryBufferOverflowDiagnostic(
    string Code,
    int CombatQueryBufferSize,
    int ReturnedCandidateCount);

/// <summary>
/// Shared T1 LoS contract from the Combat Core GDD.
/// </summary>
public static class T1CombatLineOfSight
{
    /// <summary>
    /// The exact T1 blocker layers: WorldSolid, ClosedDoor, LargeProp.
    /// </summary>
    public static IReadOnlyList<CombatLosLayer> BlockingLayers { get; } = new[]
    {
        CombatLosLayer.WorldSolid,
        CombatLosLayer.ClosedDoor,
        CombatLosLayer.LargeProp
    };

    /// <summary>
    /// True when the layer blocks T1 combat line of sight.
    /// </summary>
    public static bool BlocksLineOfSight(CombatLosLayer layer)
    {
        return BlockingLayers.Contains(layer);
    }

    /// <summary>
    /// True when none of the supplied layers blocks T1 combat line of sight.
    /// </summary>
    public static bool HasLineOfSight(IEnumerable<CombatLosLayer>? layersBetween)
    {
        return layersBetween is null || !layersBetween.Any(BlocksLineOfSight);
    }

    /// <summary>
    /// Creates an overflow diagnostic using the approved event code.
    /// </summary>
    public static CombatQueryBufferOverflowDiagnostic CreateOverflowDiagnostic(int bufferSize, int returnedCandidateCount)
    {
        return new CombatQueryBufferOverflowDiagnostic("CombatQueryBufferOverflow", bufferSize, returnedCandidateCount);
    }
}
