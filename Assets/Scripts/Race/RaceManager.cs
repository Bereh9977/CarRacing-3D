using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Runs the race in a map scene, with the pygame rules: the player races the bot, the first car over
// the finish line wins the lap, then both cars go back to the start and a 3-2-1-GO countdown starts the next lap.
// After the last lap the car with more lap wins wins the race. Lives on the Race prefab, which also holds the spawn points.
public class RaceManager : MonoBehaviour
{
    [SerializeField] private MapConfig[] maps;
    [SerializeField] private CarDefinition[] cars;
    [SerializeField] private Transform playerSpawn;
    [SerializeField] private Transform botSpawn;
    [SerializeField] private TMP_Text lapText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Image countdownImage;
    [SerializeField] private Sprite[] countdownSprites; // 3, 2, 1, GO from the pygame game
    [SerializeField] private GameObject finishPanel;
    [SerializeField] private TMP_Text resultText;

    private const int WinPrize = 200;
    private const int TiePrize = 100;

    private MapConfig map;
    private CarController playerCar;
    private CarController botCar;
    private CarCamera carCamera;
    private bool racing;
    private int playerWins;
    private int botWins;

    // Crossing the line backwards has to be undone by crossing it forwards before a crossing counts.
    private readonly Dictionary<CarController, int> backwardCrossings = new Dictionary<CarController, int>();

    private void Start()
    {
        map = FindMap(SceneManager.GetActiveScene().name);

        playerCar = SpawnCar(FindCar(GameSession.CarId), playerSpawn, map.carMaxSpeed);
        botCar = SpawnCar(cars[Random.Range(0, cars.Length)], botSpawn, map.botMaxSpeed);
        Destroy(botCar.GetComponent<PlayerCarInput>());
        botCar.gameObject.AddComponent<BotDriver>().SetRoute(GameObject.Find("BotRoute").transform);

        carCamera = Camera.main.gameObject.AddComponent<CarCamera>();
        carCamera.SetTarget(playerCar.transform);

        FindFirstObjectByType<FinishLine>().Crossed += OnFinishLineCrossed;
        finishPanel.SetActive(false);
        ShowLap();
        StartCoroutine(Countdown());
    }

    private CarController SpawnCar(CarDefinition definition, Transform spawn, float maxSpeed)
    {
        CarController car = Instantiate(definition.prefab, spawn.position, spawn.rotation).GetComponent<CarController>();
        car.SetMaxSpeed(maxSpeed);
        backwardCrossings[car] = 0;
        return car;
    }

    private void OnFinishLineCrossed(CarController car, int direction)
    {
        if (!racing || !backwardCrossings.ContainsKey(car)) return;

        if (direction < 0)
        {
            backwardCrossings[car]++;
            return;
        }
        if (backwardCrossings[car] > 0)
        {
            backwardCrossings[car]--;
            return;
        }

        if (car == playerCar) playerWins++;
        else botWins++;
        StartCoroutine(EndLap());
    }

    private IEnumerator EndLap()
    {
        SetCarsDriving(false);
        int lapsDone = playerWins + botWins;
        if (lapsDone >= map.requiredLaps)
        {
            Finish();
            yield break;
        }

        messageText.text = $"Lap {lapsDone} of {map.requiredLaps} done!";
        messageText.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);

        playerCar.ResetTo(playerSpawn);
        botCar.ResetTo(botSpawn);
        botCar.GetComponent<BotDriver>().ResetRoute();
        backwardCrossings[playerCar] = 0;
        backwardCrossings[botCar] = 0;
        carCamera.SetTarget(playerCar.transform); // jump behind the car instead of flying back
        ShowLap();
        yield return Countdown();
    }

    // The cars stand still during 3-2-1 and can drive from GO.
    private IEnumerator Countdown()
    {
        SetCarsDriving(false);
        messageText.gameObject.SetActive(false);
        countdownImage.gameObject.SetActive(true);
        for (int i = 0; i < countdownSprites.Length; i++)
        {
            countdownImage.sprite = countdownSprites[i];
            if (i == countdownSprites.Length - 1) SetCarsDriving(true); // the last picture is GO
            yield return new WaitForSeconds(1f);
        }
        countdownImage.gameObject.SetActive(false);
    }

    private void SetCarsDriving(bool driving)
    {
        racing = driving;
        playerCar.GetComponent<PlayerCarInput>().enabled = driving;
        botCar.GetComponent<BotDriver>().enabled = driving;
        if (!driving)
        {
            playerCar.Stop(); // a full stop, so a car past the finish doesn't roll into the fences
            botCar.Stop();
        }
    }

    private void Finish()
    {
        if (playerWins > botWins)
        {
            resultText.text = "You win!";
            SaveService.Data.coins += WinPrize;
        }
        else if (playerWins < botWins)
        {
            resultText.text = "Bot wins!";
        }
        else
        {
            resultText.text = "It's a tie!";
            SaveService.Data.coins += TiePrize;
        }
        SaveService.Save();

        lapText.gameObject.SetActive(false);
        messageText.gameObject.SetActive(false);
        finishPanel.SetActive(true);
    }

    private void ShowLap()
    {
        lapText.text = $"Lap {playerWins + botWins + 1}/{map.requiredLaps}";
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
