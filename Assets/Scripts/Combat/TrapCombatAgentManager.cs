using UnityEngine;

public static class TrapCombatAgentManager
{
    public static int Charges { get; private set; } = 0;

    public static int BeamDeactivateCount { get; private set; } = 0;
    public static int SpikeDeactivateCount { get; private set; } = 0;
    public static int DividerDeactivateCount { get; private set; } = 0;

    private static float activeUntil = -1f;
    private static string activeTrapKey = string.Empty;

    public static bool IsActiveFor(string trapKey)
    {
        return Time.time < activeUntil && activeTrapKey == trapKey;
    }

    public static void AddCharge(int amount = 1)
    {
        Charges += amount;
    }

    public static bool TryActivate(string trapKey, float duration)
    {
        if (Charges <= 0) return false;

        Charges--;
        activeTrapKey = trapKey;
        activeUntil = Time.time + duration;
        RecordDeactivation(trapKey);
        return true;
    }

    static void RecordDeactivation(string trapKey)
    {
        if (string.IsNullOrEmpty(trapKey)) return;
        switch (trapKey.ToLowerInvariant())
        {
            case "beam":
                BeamDeactivateCount++;
                break;
            case "spikes":
                SpikeDeactivateCount++;
                break;
            case "divider":
                DividerDeactivateCount++;
                break;
        }
    }

    public static void ResetAll()
    {
        Charges = 0;
        activeUntil = -1f;
        activeTrapKey = string.Empty;
        BeamDeactivateCount = 0;
        SpikeDeactivateCount = 0;
        DividerDeactivateCount = 0;
    }
}