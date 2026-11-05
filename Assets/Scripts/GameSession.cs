using UnityEngine;

public enum GameMode { Single, Doubles }

public static class GameSession
{
    public static GameMode Mode;
    public static string CarId = "car1";
    public static string MapId;
}
