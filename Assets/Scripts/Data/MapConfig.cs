using UnityEngine;

// Per-map settings from the pygame maps_config.py. The scene name is also the map id.
[CreateAssetMenu(menuName = "Car Racing/Map Config", fileName = "MapConfig")]
public class MapConfig : ScriptableObject
{
    public string sceneName;
    public bool isFree;
    public int price = 5000;
    [Min(1)] public int requiredLaps = 1;
    public ObstacleType obstacleType;

    [Header("Top speeds, m/s")]
    public float carMaxSpeed;
    public float botMaxSpeed;
}
