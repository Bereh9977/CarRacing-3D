using UnityEngine;

// One race participant and its race points (pygame RacerStats).
public class Racer
{
    public readonly string name; // "You", "Bot", "Car 1", "Car 2"
    public readonly CarController car;
    public int points;
    public int lapsWon;
    public int streak;              // laps won in a row
    public int nextCheckpoint;      // 0-7 this lap
    public int lapCheckpointPoints; // multiplied by the streak when the lap is won
    public float lastWallHitTime = -1f;
    public Vector3 previousPosition;

    public Racer(string name, CarController car)
    {
        this.name = name;
        this.car = car;
        previousPosition = car.transform.position;
    }

    public void ResetLap()
    {
        nextCheckpoint = 0;
        lapCheckpointPoints = 0;
        previousPosition = car.transform.position;
    }
}
