namespace CustomSecProvider.RA.Models;

/// <summary>
/// Current security posture for incident mode enforcement.
/// </summary>
public enum SecurityPosture
{
    /// <summary>Normal operations.</summary>
    Normal = 0,

    /// <summary>Incident mode: read-only access only, export disabled.</summary>
    IncidentReadOnly = 1,

    /// <summary>Incident mode: all access denied.</summary>
    IncidentDenied = 2
}
