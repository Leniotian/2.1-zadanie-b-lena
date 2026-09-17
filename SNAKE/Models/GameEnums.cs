namespace SNAKE.Models;

public enum BoardSize
{
    Small,  // 10x10 - large tiles, compact fast games
    Normal, // 16x16 - balanced standard Google Snake experience
    Large   // 22x22 - vast forest arena
}

public enum GameSpeed
{
    Sloth,   // 160ms - gentle, beginner friendly
    Deer,    // 115ms - standard Google Snake speed
    Falcon,  // 75ms  - fast pace
    Viper,   // 48ms  - extreme reflexes
    Dynamic  // Starts at 125ms and accelerates as you eat fruits
}

public enum WallMode
{
    Solid,  // Hitting the boundary results in game over
    Portal  // Passing through a boundary wraps around to the opposite side
}

public enum SpecialMode
{
    Classic,         // 1 red apple at a time
    TwinApples,      // 2 fruits on the board at all times
    GoldenAcorn,     // Golden acorns spawn periodically with bonus score and countdown
    ForestObstacles, // Ancient stones and tree stumps scatter the woods
    Peaceful         // Pass through walls and body - pure peaceful harvesting
}

public enum GameState
{
    Menu,
    Ready,
    Running,
    Paused,
    GameOver,
    Victory
}
