using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Finds the race objects every map scene must contain (see Docs/SceneContract.md).
// Child order in the Hierarchy is the order of the bot path.
public class TrackLayout
{
    public Transform PlayerSpawn;
    public Transform BotSpawn;
    public Transform FinishLine;
    public Transform[] BotPath;
    public Transform[] CoinSpots;
    public Transform[] ObstacleSpots;

    public static TrackLayout FromScene(List<string> problems)
    {
        var layout = new TrackLayout();

        Transform spawns = Find("SpawnPoints", problems);
        if (spawns != null)
        {
            layout.PlayerSpawn = spawns.Find("Player");
            layout.BotSpawn = spawns.Find("Bot");
            if (layout.PlayerSpawn == null) problems.Add("Missing 'SpawnPoints/Player'.");
            if (layout.BotSpawn == null) problems.Add("Missing 'SpawnPoints/Bot'.");
        }

        layout.FinishLine = Find("FinishLine", problems);
        if (layout.FinishLine != null && !layout.FinishLine.GetComponentsInChildren<Collider>().Any(c => c.isTrigger))
            problems.Add("'FinishLine' needs a Collider with Is Trigger enabled.");

        layout.BotPath = ChildrenOf(Find("BotPath", problems));
        layout.CoinSpots = ChildrenOf(Find("CoinSpots", problems));
        GameObject obstacleSpots = GameObject.Find("ObstacleSpots"); // only on maps with obstacles
        layout.ObstacleSpots = ChildrenOf(obstacleSpots != null ? obstacleSpots.transform : null);

        if (layout.BotPath.Length < 3) problems.Add($"'BotPath' needs at least 3 points, found {layout.BotPath.Length}.");
        if (layout.CoinSpots.Length < 5) problems.Add($"'CoinSpots' needs at least 5 points, found {layout.CoinSpots.Length}.");

        return layout;
    }

    private static Transform Find(string name, List<string> problems)
    {
        GameObject found = GameObject.Find(name);
        if (found == null) problems.Add($"Missing object '{name}'.");
        return found != null ? found.transform : null;
    }

    private static Transform[] ChildrenOf(Transform parent)
    {
        return parent != null ? parent.Cast<Transform>().ToArray() : new Transform[0];
    }
}
