using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// Starts the race in a map scene: spawns the chosen car, follows it with the camera, counts laps and ends the race.
// Lives on the Race prefab, which also holds the spawn points.
public class RaceManager : MonoBehaviour
{
    [SerializeField] private MapConfig[] maps;
    [SerializeField] private CarDefinition[] cars;
    [SerializeField] private Transform playerSpawn;
    [SerializeField] private TMP_Text lapText;
    [SerializeField] private GameObject finishPanel;

    private MapConfig map;
    private CarController playerCar;
    private int lapsDone;

    private void Start()
    {
        map = FindMap(SceneManager.GetActiveScene().name);
        CarDefinition carDefinition = FindCar(GameSession.CarId);

        GameObject car = Instantiate(carDefinition.prefab, playerSpawn.position, playerSpawn.rotation);
        playerCar = car.GetComponent<CarController>();
        playerCar.SetMaxSpeed(map.carMaxSpeed);
        Camera.main.gameObject.AddComponent<CarCamera>().SetTarget(car.transform);

        FindFirstObjectByType<FinishLine>().Crossed += OnFinishLineCrossed;
        finishPanel.SetActive(false);
        ShowLap();
    }

    private void OnFinishLineCrossed(CarController car, int direction)
    {
        if (car != playerCar || lapsDone >= map.requiredLaps) return;

        // Crossing backwards takes a lap away, so reversing over the line and back doesn't count.
        lapsDone += direction;
        if (lapsDone >= map.requiredLaps)
            Finish();
        else
            ShowLap();
    }

    private void Finish()
    {
        playerCar.GetComponent<PlayerCarInput>().enabled = false;
        playerCar.SetInput(0f, 0f); // the car rolls to a stop
        lapText.gameObject.SetActive(false);
        finishPanel.SetActive(true);
    }

    private void ShowLap()
    {
        int lap = Mathf.Clamp(lapsDone + 1, 1, map.requiredLaps);
        lapText.text = $"Lap {lap}/{map.requiredLaps}";
    }

    // Called by the button on the finish panel.
    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private MapConfig FindMap(string sceneName)
    {
        foreach (MapConfig m in maps)
            if (m.sceneName == sceneName) return m;
        Debug.LogError($"No MapConfig for scene '{sceneName}' in RaceManager.");
        return null;
    }

    // Falls back to the first car when the id is unknown, e.g. when the map is started from the Editor.
    private CarDefinition FindCar(string id)
    {
        foreach (CarDefinition c in cars)
            if (c.id == id) return c;
        return cars[0];
    }
}
