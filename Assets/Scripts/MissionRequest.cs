/// <summary>
/// Lightweight static channel for passing a mission start request across a scene load.
/// Set <see cref="PendingMissionName"/> before loading the gameplay scene; 
/// <see cref="MissionManager"/> will consume it during initialization.
/// </summary>
public static class MissionRequest
{
    /// <summary>
    /// The name of the mission to start once the gameplay scene initializes.
    /// Matches <see cref="MissionData.missionName"/>. Null or empty means no pending request.
    /// </summary>
    public static string PendingMissionName { get; set; }

    /// <summary>Clears the pending request without starting anything.</summary>
    public static void Clear() => PendingMissionName = null;
}
