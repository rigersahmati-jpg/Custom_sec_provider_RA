namespace CustomSecProvider.RA.Models;

/// <summary>
/// Seat type for concurrent user licensing.
/// </summary>
public enum SeatType
{
    /// <summary>Read-only analytics viewer.</summary>
    Viewer = 0,

    /// <summary>Report designer and developer.</summary>
    Designer = 1,

    /// <summary>Administrator with full control.</summary>
    Admin = 2
}
