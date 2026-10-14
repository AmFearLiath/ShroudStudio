namespace ShroudStudio.Core;

public sealed record RuntimeState(
    bool IsGameRunning,
    bool HasSelectedSavegame,
    string? SelectedSavegamePath,
    string? GameStatus,
    string? SavegameStatus);
