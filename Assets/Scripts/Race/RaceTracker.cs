using UnityEngine;

// Race points, ported from the pygame RaceTracker. They are separate from coins and change live during the race:
// checkpoints give points (more to the first car), walls and obstacles take points (never below 0),
// a won lap gives points multiplied by the win streak, and winning every lap of a longer race gives a bonus.
public class RaceTracker
{
    private const int CheckpointCount = 8;
    private const float CheckpointHalfWidth = 10f; // m; the road is 15 m wide
    private const int CheckpointFirstPoints = 100;
    private const int CheckpointSecondPoints = 50;
    private const int LapWinPoints = 200;
    private const int WallHitPenalty = 20;
    private const int ObstacleHitPenalty = 50;
    private const float WallHitCooldown = 0.5f; // s
    private static readonly float[] StreakMultipliers = { 1f, 1.5f, 2f };
    private const int FlawlessBonus = 200;

    private readonly Racer[] racers;
    private readonly Vector3[] checkpoints;           // a point in the middle of the road
    private readonly Vector3[] checkpointDirections;  // the driving direction there
    private readonly bool[] checkpointTaken = new bool[CheckpointCount];

    // The checkpoints are invisible lines across the road, spread evenly along the bot route, like the pygame gates.
    public RaceTracker(Racer[] racers, Transform route)
    {
        this.racers = racers;
        checkpoints = new Vector3[CheckpointCount];
        checkpointDirections = new Vector3[CheckpointCount];
        int count = route.childCount;
        for (int i = 0; i < CheckpointCount; i++)
        {
            int index = Mathf.Clamp(Mathf.RoundToInt((i + 1) * count / (CheckpointCount + 1f)), 1, count - 2);
            checkpoints[i] = route.GetChild(index).position;
            Vector3 direction = route.GetChild(index + 1).position - route.GetChild(index - 1).position;
            direction.y = 0f;
            checkpointDirections[i] = direction.normalized;
        }
    }

    // Called every physics step while the cars can drive.
    public void Update()
    {
        foreach (Racer racer in racers)
        {
            Vector3 position = racer.car.transform.position;
            if (racer.nextCheckpoint < CheckpointCount && Crossed(racer.nextCheckpoint, racer.previousPosition, position))
                PassCheckpoint(racer);
            racer.previousPosition = position;
        }
    }

    private bool Crossed(int checkpoint, Vector3 from, Vector3 to)
    {
        Vector3 offset = to - checkpoints[checkpoint];
        offset.y = 0f;
        bool wasBehind = Vector3.Dot(from - checkpoints[checkpoint], checkpointDirections[checkpoint]) < 0f;
        bool isInFront = Vector3.Dot(offset, checkpointDirections[checkpoint]) >= 0f;
        return wasBehind && isInFront && offset.magnitude < CheckpointHalfWidth;
    }

    // Checkpoints count only in order: +100 for the first car, +50 for the second.
    private void PassCheckpoint(Racer racer)
    {
        int points = checkpointTaken[racer.nextCheckpoint] ? CheckpointSecondPoints : CheckpointFirstPoints;
        checkpointTaken[racer.nextCheckpoint] = true;
        racer.lapCheckpointPoints += points;
        racer.points += points;
        racer.nextCheckpoint++;
    }

    // Touching walls again within 0.5 s doesn't count.
    public void WallHit(Racer racer)
    {
        if (racer.lastWallHitTime < 0f || Time.time - racer.lastWallHitTime >= WallHitCooldown)
            Penalty(racer, WallHitPenalty);
        racer.lastWallHitTime = Time.time;
    }

    public void ObstacleHit(Racer racer)
    {
        Penalty(racer, ObstacleHitPenalty);
    }

    // The winner gets 200, and its checkpoint and win points this lap are multiplied by its win streak.
    // Penalties aren't multiplied. The other racers lose their streak.
    public void FinishLap(Racer winner)
    {
        foreach (Racer racer in racers)
        {
            if (racer == winner)
            {
                racer.streak++;
                racer.lapsWon++;
                float multiplier = StreakMultipliers[Mathf.Min(racer.streak, StreakMultipliers.Length) - 1];
                int earned = racer.lapCheckpointPoints + LapWinPoints;
                racer.points += Mathf.RoundToInt(earned * multiplier) - racer.lapCheckpointPoints;
            }
            else
            {
                racer.streak = 0;
            }
            racer.ResetLap();
        }
        for (int i = 0; i < CheckpointCount; i++)
            checkpointTaken[i] = false;
    }

    // Winning every lap of a race with more than one lap gives a bonus.
    public void FinishRace(int requiredLaps)
    {
        foreach (Racer racer in racers)
            if (requiredLaps > 1 && racer.lapsWon == requiredLaps)
                racer.points += FlawlessBonus;
    }

    private static void Penalty(Racer racer, int amount)
    {
        racer.points = Mathf.Max(0, racer.points - amount);
    }
}
