using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CarRacingMenu
{
    [MenuItem("Tools/Car Racing/Validate Open Scene")]
    public static void ValidateOpenScene()
    {
        var problems = new List<string>();
        int races = Object.FindObjectsByType<RaceManager>(FindObjectsSortMode.None).Length;
        int finishLines = Object.FindObjectsByType<FinishLine>(FindObjectsSortMode.None).Length;
        if (races != 1) problems.Add($"Needs exactly one Race prefab, found {races}.");
        if (finishLines != 1) problems.Add($"Needs exactly one start/finish road piece, found {finishLines}.");
        GameObject route = GameObject.Find("BotRoute");
        if (route == null || route.transform.childCount < 2) problems.Add("Needs a BotRoute with points (Tools → Car Racing → Generate Bot Route).");
        string scene = EditorSceneManager.GetActiveScene().name;

        if (problems.Count == 0)
            Debug.Log($"'{scene}' is ready for racing.");
        else
            Debug.LogWarning($"'{scene}' has race setup problems:\n- " + string.Join("\n- ", problems));
    }

    private static readonly string[] GroupNames = { "Lighting", "Environment", "Track", "Decorations", "Coins", "Obstacles" };

    // Moves the root objects of the open scene into group objects. Cameras, cars and the Race prefab stay at the root.
    // Running it again only moves the new objects, so it can be used after adding things to the map.
    [MenuItem("Tools/Car Racing/Organize Open Scene")]
    public static void OrganizeOpenScene()
    {
        var scene = EditorSceneManager.GetActiveScene();
        var groups = new Dictionary<string, Transform>();
        foreach (GameObject root in scene.GetRootGameObjects())
            if (System.Array.IndexOf(GroupNames, root.name) >= 0)
                groups[root.name] = root.transform;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (groups.ContainsValue(root.transform)) continue;
            string group = GroupFor(root);
            if (group == null) continue;

            if (!groups.ContainsKey(group))
            {
                var parent = new GameObject(group);
                Undo.RegisterCreatedObjectUndo(parent, "Organize Scene");
                groups[group] = parent.transform;
            }
            Undo.SetTransformParent(root.transform, groups[group], "Organize Scene");
        }

        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log($"'{scene.name}' organized.");
    }

    private static string GroupFor(GameObject root)
    {
        if (root.GetComponent<Camera>() != null || root.GetComponent<CarController>() != null || root.GetComponent<RaceManager>() != null) return null;
        if (root.name == "BotRoute") return null;
        if (root.GetComponent<Light>() != null) return "Lighting";
        if (root.GetComponent<Terrain>() != null) return "Environment";

        string prefab = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(root);
        string prefabName = System.IO.Path.GetFileNameWithoutExtension(prefab);
        if (prefabName == "Coin" || prefabName == "StarCoin") return "Coins";
        if (prefabName == "Sand" || prefabName == "Ice" || prefabName == "Banana") return "Obstacles";
        if (prefab.Contains("Modular_Track_Free") || root.name.StartsWith("Fence")) return "Track";
        return "Decorations";
    }

    private const float TraceStep = 2f;          // m between road checks
    private const float PointSpacing = 10f;      // m between route points
    private const float HalfRoadWidth = 7.5f;
    private const float MaxRouteLength = 6000f;

    // Places the bot's route points along the middle of the road, from the Race start to the finish line.
    // It walks the road in small steps and keeps to the middle between the Wall borders on both sides.
    // Running it again replaces the old route.
    [MenuItem("Tools/Car Racing/Generate Bot Route")]
    public static void GenerateBotRoute()
    {
        var race = Object.FindFirstObjectByType<RaceManager>();
        var finish = Object.FindFirstObjectByType<FinishLine>();
        if (race == null || finish == null)
        {
            Debug.LogWarning("Generate Bot Route needs the Race prefab and the start/finish road piece in the scene.");
            return;
        }

        Physics.SyncTransforms(); // make raycasts see the scene as it is in the Editor
        List<Vector3> points = TraceRoad(race.transform, finish.transform);
        if (points == null) return;

        GameObject old = GameObject.Find("BotRoute");
        if (old != null) Undo.DestroyObjectImmediate(old);

        var route = new GameObject("BotRoute");
        Undo.RegisterCreatedObjectUndo(route, "Generate Bot Route");
        var icon = (Texture2D)EditorGUIUtility.IconContent("sv_icon_dot3_pix16_gizmo").image;
        for (int i = 0; i < points.Count; i++)
        {
            var point = new GameObject($"Point {i:000}");
            point.transform.SetParent(route.transform);
            point.transform.position = points[i];
            EditorGUIUtility.SetIconForObject(point, icon); // visible in the Scene view
        }

        EditorSceneManager.MarkSceneDirty(route.scene);
        Debug.Log($"Bot route generated: {points.Count} points.");
    }

    private static List<Vector3> TraceRoad(Transform start, Transform finish)
    {
        var points = new List<Vector3>();
        Vector3 position = start.position;
        Vector3 direction = Flat(start.forward);
        if (!SnapToRoad(ref position))
        {
            Debug.LogWarning("Generate Bot Route: there's no road under the Race object.");
            return null;
        }

        float travelled = 0f;
        float sinceLastPoint = 0f;
        bool wasInFront = IsInFront(position, finish);

        while (travelled < MaxRouteLength)
        {
            direction = ChooseDirection(position, direction);
            Vector3 next = KeepToMiddle(position + direction * TraceStep, direction);
            if (!SnapToRoad(ref next))
            {
                Debug.LogWarning($"Generate Bot Route lost the road at {next}. Check that the road pieces there have walls on both sides.");
                return null;
            }

            direction = Flat(next - position);
            travelled += TraceStep;
            sinceLastPoint += TraceStep;

            // The finish line's plane is endless, so a crossing only counts close to the line itself.
            bool inFront = IsInFront(next, finish);
            Vector3 toFinish = next - finish.position;
            toFinish.y = 0f;
            bool nearFinish = toFinish.magnitude < HalfRoadWidth + 2f;
            if (!wasInFront && inFront && nearFinish && travelled > 50f)
            {
                points.Add(next + direction * 4f); // a last point just past the line, so the bot crosses it
                return points;
            }
            wasInFront = inFront;

            if (sinceLastPoint >= PointSpacing)
            {
                points.Add(next);
                sinceLastPoint = 0f;
            }
            position = next;
        }

        Debug.LogWarning($"Generate Bot Route didn't reach the finish line within {MaxRouteLength} m. Check the Race direction.");
        return null;
    }

    // Keeps the direction while the road ahead is clear. Before a corner it turns, trying angles
    // left and right in 5° steps, and takes the first one with no wall for ClearAhead metres.
    private const float ClearAhead = 20f;

    private static Vector3 ChooseDirection(Vector3 position, Vector3 direction)
    {
        Vector3 origin = position + Vector3.up;
        int walls = LayerMask.GetMask("Wall");
        Vector3 bestDirection = direction;
        float bestFree = 0f;

        for (int angle = 0; angle <= 90; angle += 5)
        {
            foreach (int side in new[] { 1, -1 })
            {
                Vector3 tryDirection = Quaternion.Euler(0f, angle * side, 0f) * direction;
                float free = Physics.Raycast(origin, tryDirection, out RaycastHit hit, ClearAhead, walls, QueryTriggerInteraction.Ignore)
                    ? hit.distance
                    : ClearAhead;
                if (free >= ClearAhead) return tryDirection;
                if (free > bestFree)
                {
                    bestFree = free;
                    bestDirection = tryDirection;
                }
            }
        }
        return bestDirection; // nothing fully clear: go where there's the most room
    }

    // Moves the point sideways to the middle between the walls on the left and right.
    private static Vector3 KeepToMiddle(Vector3 point, Vector3 direction)
    {
        Vector3 right = Vector3.Cross(Vector3.up, direction);
        Vector3 origin = point + Vector3.up; // walls are 2 m high
        int walls = LayerMask.GetMask("Wall");
        bool hitRight = Physics.Raycast(origin, right, out RaycastHit r, HalfRoadWidth + 5f, walls, QueryTriggerInteraction.Ignore);
        bool hitLeft = Physics.Raycast(origin, -right, out RaycastHit l, HalfRoadWidth + 5f, walls, QueryTriggerInteraction.Ignore);

        Vector3 middle = point;
        if (hitRight && hitLeft) middle = (r.point + l.point) / 2f;
        else if (hitRight) middle = r.point - right * HalfRoadWidth;  // some corners have a wall on one side only
        else if (hitLeft) middle = l.point + right * HalfRoadWidth;
        return new Vector3(middle.x, point.y, middle.z);
    }

    // Puts the point on the road surface. Only the BEDRILL road pieces count, so coins, obstacles,
    // the terrain and decorations are ignored. Returns false when there's no road under the point.
    private static bool SnapToRoad(ref Vector3 point)
    {
        int notWalls = ~LayerMask.GetMask("Wall");
        RaycastHit[] hits = Physics.RaycastAll(point + Vector3.up * 3f, Vector3.down, 6f, notWalls, QueryTriggerInteraction.Ignore);
        foreach (RaycastHit hit in hits)
        {
            string prefab = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(hit.collider.gameObject);
            if (prefab.Contains("Modular_Track_Free"))
            {
                point.y = hit.point.y;
                return true;
            }
        }
        return false;
    }

    private static bool IsInFront(Vector3 point, Transform finish)
    {
        return Vector3.Dot(point - finish.position, finish.forward) > 0f;
    }

    private static Vector3 Flat(Vector3 v)
    {
        v.y = 0f;
        return v.normalized;
    }

    [MenuItem("Tools/Car Racing/Reset Save Data")]
    public static void ResetSaveData()
    {
        SaveService.ResetProgress();
        Debug.Log("Save data reset.");
    }
}
