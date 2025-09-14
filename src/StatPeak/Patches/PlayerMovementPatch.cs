using System;
using HarmonyLib;
using UnityEngine;

namespace StatPeak.Patches;

public class PlayerMovementPatch
{
    private static Vector3 PreviousPosition = Vector3.zero;

    private static void IncrementWithoutY(string statName, Vector3 updatedPosition, Vector3 previousPosition)
    {
        var tempY = updatedPosition.y;
        var tempPrevY = previousPosition.y;

        updatedPosition.y = 0f;
        previousPosition.y = 0f;

        var distance = Vector3.Distance(updatedPosition, previousPosition);
        // Because position uses units rather than meters, we need to convert before incrementing
        var distanceInMeters = distance * CharacterStats.unitsToMeters;

        PlayerStats.Increment(statName, distanceInMeters);

        updatedPosition.y = tempY;
        previousPosition.y = tempPrevY;
    }

    private static void IncrementOnlyWithY(string statName, Vector3 updatedPosition, Vector3 previousPosition)
    {
        var distance = Math.Sqrt(Math.Pow(updatedPosition.y - previousPosition.y, 2));
        // Because position uses units rather than meters, we need to convert before incrementing
        var distanceInMeters = distance * CharacterStats.unitsToMeters;

        PlayerStats.Increment(statName, distanceInMeters);
    }

    private static void IncrementDistanceTraveled(Vector3 currentPosition, Vector3 previousPosition)
    {
        IncrementWithoutY(Stat.DistanceWalked, currentPosition, previousPosition);
    }

    private static void IncrementDistanceClimbed(Vector3 currentPosition, Vector3 previousPosition)
    {
        IncrementOnlyWithY(Stat.DistanceClimbed, currentPosition, previousPosition);
    }

    private static void IncrementDistanceVineClimbed(Vector3 currentPosition, Vector3 previousPosition)
    {
        IncrementWithoutY(Stat.DistanceClimbedOnVines, currentPosition, previousPosition);
    }

    private static void IncrementDistanceInAir(Vector3 currentPosition, Vector3 previousPosition)
    {
        IncrementOnlyWithY(Stat.DistanceWhileAirborne, currentPosition, previousPosition);
    }

    private static void IncrementDistanceRopeClimbed(Vector3 currentPosition, Vector3 previousPosition)
    {
        IncrementOnlyWithY(Stat.DistanceClimbedOnRopes, currentPosition, previousPosition);
    }

    [HarmonyPatch(typeof(CharacterMovement), nameof(CharacterMovement.FixedUpdate))]
    [HarmonyPostfix]
    public static void IncrementCharacterMovement(CharacterMovement __instance)
    {
        if (!__instance.character.IsLocal) return;

        Vector3 currentPosition = __instance.character.Center;

        if (PlayerMovementPatch.PreviousPosition == Vector3.zero)
        {
            PlayerMovementPatch.PreviousPosition = currentPosition;
            return;
        }

        // Using if statements to track different movement types within one patch.
        // This ensures that exactly one stat is incremeneted at the same time.
        if (__instance.character.data.isGrounded)
        {
            IncrementDistanceTraveled(currentPosition, PlayerMovementPatch.PreviousPosition);
        }
        else if (__instance.character.data.isClimbing)
        {
            IncrementDistanceClimbed(currentPosition, PlayerMovementPatch.PreviousPosition);
        }
        else if (__instance.character.data.isVineClimbing)
        {
            IncrementDistanceVineClimbed(currentPosition, PlayerMovementPatch.PreviousPosition);
        }
        else if (__instance.character.data.isRopeClimbing)
        {
            IncrementDistanceRopeClimbed(currentPosition, PlayerMovementPatch.PreviousPosition);
        }
        else if (!__instance.character.data.isGrounded && !__instance.character.data.isClimbingAnything)
        {
            IncrementDistanceInAir(currentPosition, PlayerMovementPatch.PreviousPosition);
        }

        PreviousPosition = currentPosition;
    }

    [HarmonyPatch(typeof(Character), nameof(Character.UpdateVariablesFixed))]
    [HarmonyPostfix]
    public static void IncrementCharacterGreatestContinuousClimb(Character __instance)
    {
        if (!__instance.IsLocal) { return; }

        if (!__instance.data.isClimbing) { return; }

        if (__instance.data.sinceGrounded > __instance.data.sinceClimb + 1f)
        {
            // This is a debug to show when endurance achivement can be thrown
            Plugin.Logger.LogDebug($"[GreatestContinuousClimb] {{ SinceGrounded: {__instance.data.sinceGrounded} }} > {{ SinceClimb + 1f: {__instance.data.sinceClimb + 1f} }}");
        }

        var currentClimb = __instance.Center.y - __instance.data.lastGroundedHeight;
        var currentClimbInMeters = currentClimb * CharacterStats.unitsToMeters;

        PlayerStats.Set(
            Stat.GreatestContinuousClimb,
            // Only record the biggest of the two!
            Mathf.Max((float)PlayerStats.Get(Stat.GreatestContinuousClimb), currentClimbInMeters)
        );
    }
}
