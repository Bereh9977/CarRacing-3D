using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

// Runs the race in a map scene, with the pygame rules. Single: the player races the bot. Doubles: two players
// on one keyboard (WASD and arrows) with a split screen. The first car over the finish line wins the lap, then
// both cars go back to the start and a 3-2-1-GO countdown starts the next lap. After the last lap the car with
// more lap wins wins the race. Lives on the Race prefab, which also holds the spawn points.
public class RaceManager : MonoBehaviour
{
    [SerializeField] private MapConfig[] maps;
    [SerializeField] private CarDefinition[] cars;
    [FormerlySerializedAs("playerSpawn")] [SerializeField] private Transform car1Spawn;
    [FormerlySerializedAs("botSpawn")] [SerializeField] private Transform car2Spawn;
    [SerializeField] private InputActionReference player2Throttle;
    [SerializeField] private InputActionReference player2Steer;
    [SerializeField] private TMP_Text lapText;
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Image countdownImage;
    [SerializeField] private Sprite[] countdownSprites; // 3, 2, 1, GO from the pygame game
    [SerializeField] private GameObject finishPanel;
    [SerializeField] private TMP_Text resultText;

    private const int WinPrize = 200;
    private const int TiePrize = 100;
    private const int CoinsShown = 5;

    private MapConfig map;
    private bool doubles;
    private CarController car1; // the player in Single
    private CarController car2; // the bot in Single
    private CarCamera camera1;
    private CarCamera camera2; // Doubles only
    private bool racing;
    private int car1Wins;
    private int car2Wins;

    // Crossing the line backwards has to be undone by crossing it forwards before a crossing counts.
    private readonly Dictionary<CarController, int> backwardCrossings = new Dictionary<CarController, int>();

    private void Start()
    {
        map = FindMap(SceneManager.GetActiveScene().name);
        doubles = GameSession.Mode == GameMode.Doubles;

        if (doubles)
        {
            // As in pygame: car1 on WASD and car2 on the arrows, both with the player top speed.
            car1 = SpawnCar(FindCar("car1"), car1Spawn, map.carMaxSpeed);
            car2 = SpawnCar(FindCar("car2"), car2Spawn, map.carMaxSpeed);
            car2.GetComponent<PlayerCarInput>().SetActions(player2Throttle, player2Steer);
        }
        else
        {
            car1 = SpawnCar(FindCar(GameSession.CarId), car1Spawn, map.carMaxSpeed);
            car2 = SpawnCar(cars[Random.Range(0, cars.Length)], car2Spawn, map.botMaxSpeed);
            Destroy(car2.GetComponent<PlayerCarInput>());
            car2.gameObject.AddComponent<BotDriver>().SetRoute(GameObject.Find("BotRoute").transform);
        }
        SetUpCameras();
        SetUpCoins();

        FindFirstObjectByType<FinishLine>().Crossed += OnFinishLineCrossed;
        Coin.Collected += ShowCoins;
        finishPanel.SetActive(false);
        coinsText.gameObject.SetActive(!doubles); // Doubles has no coins
        ShowCoins();
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

    // Single: Main Camera follows the player on the full screen.
    // Doubles: Main Camera follows car 1 on the left half and a copy of it follows car 2 on the right half.
    private void SetUpCameras()
    {
        Camera main = Camera.main;
        if (doubles)
        {
            Camera second = Instantiate(main);
            second.tag = "Untagged";
            Destroy(second.GetComponent<AudioListener>()); // a scene can have only one
            main.rect = new Rect(0f, 0f, 0.5f, 1f);
            second.rect = new Rect(0.5f, 0f, 0.5f, 1f);
            camera2 = second.gameObject.AddComponent<CarCamera>();
            camera2.SetTarget(car2.transform);
        }
        camera1 = main.gameObject.AddComponent<CarCamera>();
        camera1.SetTarget(car1.transform);
    }

    // As in pygame, Single shows 5 random coins of the ones placed on the map. Doubles has no coins.
    private void SetUpCoins()
    {
        Coin[] coins = FindObjectsByType<Coin>();
        var shown = new List<Coin>(coins);
        int keep = doubles ? 0 : CoinsShown;
        while (shown.Count > keep)
        {
            int i = Random.Range(0, shown.Count);
            shown[i].gameObject.SetActive(false);
            shown.RemoveAt(i);
        }
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

        if (car == car1) car1Wins++;
        else car2Wins++;
        StartCoroutine(EndLap());
    }

    private IEnumerator EndLap()
    {
        SetCarsDriving(false);
        int lapsDone = car1Wins + car2Wins;
        if (lapsDone >= map.requiredLaps)
        {
            Finish();
            yield break;
        }

        messageText.text = $"Lap {lapsDone} of {map.requiredLaps} done!";
        messageText.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);

        car1.ResetTo(car1Spawn);
        car2.ResetTo(car2Spawn);
        if (!doubles) car2.GetComponent<BotDriver>().ResetRoute();
        backwardCrossings[car1] = 0;
        backwardCrossings[car2] = 0;
        // jump behind the cars instead of flying back
        camera1.SetTarget(car1.transform);
        if (doubles) camera2.SetTarget(car2.transform);
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
        car1.GetComponent<PlayerCarInput>().enabled = driving;
        if (doubles)
            car2.GetComponent<PlayerCarInput>().enabled = driving;
        else
            car2.GetComponent<BotDriver>().enabled = driving;

        if (!driving)
        {
            car1.Stop(); // a full stop, so a car past the finish doesn't roll into the fences
            car2.Stop();
        }
    }

    private void Finish()
    {
        if (car1Wins > car2Wins)
            resultText.text = doubles ? "Car 1 wins!" : "You win!";
        else if (car1Wins < car2Wins)
            resultText.text = doubles ? "Car 2 wins!" : "Bot wins!";
        else
            resultText.text = "It's a tie!";

        // Prize coins are for Single only, as in pygame.
        if (!doubles && car1Wins >= car2Wins)
        {
            SaveService.Data.coins += car1Wins > car2Wins ? WinPrize : TiePrize;
            SaveService.Save();
            ShowCoins();
        }

        lapText.gameObject.SetActive(false);
        messageText.gameObject.SetActive(false);
        finishPanel.SetActive(true);
    }

    // Coin.Collected is static, so it has to be let go of when the map scene closes.
    private void OnDestroy()
    {
        Coin.Collected -= ShowCoins;
    }

    private void ShowCoins()
    {
        coinsText.text = $"Coins: {SaveService.Data.coins}";
    }

    private void ShowLap()
    {
        lapText.text = $"Lap {car1Wins + car2Wins + 1}/{map.requiredLaps}";
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
