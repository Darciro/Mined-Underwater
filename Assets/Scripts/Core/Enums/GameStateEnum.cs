/// <summary>
/// Represents the different states the game can be in.
/// </summary>
public enum GameStateEnum
{
    /// <summary>
    /// Game is in the main menu
    /// </summary>
    MainMenu,

    /// <summary>
    /// The game is not active (e.g., waiting to start)
    /// </summary>
    Idle,

    /// <summary>
    /// Game is actively being played
    /// </summary>
    Playing,

    /// <summary>
    /// Game is paused
    /// </summary>
    Paused,

    /// <summary>
    /// Level has been completed successfully
    /// </summary>
    LevelComplete,

    /// <summary>
    /// Game has ended (player died)
    /// </summary>
    GameOver
}
