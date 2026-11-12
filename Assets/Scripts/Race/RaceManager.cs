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
// more lap wins wins the race. Race points are counted live by RaceTracker. Lives on the Race prefab, which also holds the spawn points.
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
    [SerializeField] private TMP_Text pointsText;
    [SerializeField] private Image countdownImage;
    [SerializeField] private Sprite[] countdownSprites; // 3, 2, 1, GO from the pygame game
    [SerializeField] private GameObject finishPanel;
    [SerializeField] private TMP_Text resultText;

    private const int WinPrize = 200;
    private const int TiePrize = 100;
    private const int CoinsShown = 5;
    private const int ObstaclesShown = 4;

    private MapConfig map;
    private bool doubles;
    private CarController car1; // the player in Single
    private CarController car2; // the bot in Single
    private CarCamera camera1;
    private CarCamera camera2; // Doubles only
    private bool racing;
    private Racer racer1;
    private Racer racer2;
    private RaceTracker tracker;

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
        racer1 = new Racer(doubles ? "Car 1" : "You", car1);
        racer2 = new Racer(doubles ? "Car 2" : "Bot", car2);
        tracker = new RaceTracker(new[] { racer1, racer2 }, GameObject.Find("BotRoute").transform);
        // As in pygame, the bot loses no points for walls; obstacles don't touch it at all.
        car1.WallHit += OnWallHit;
        if (doubles) car2.WallHit += OnWallHit;
        Obstacle.Hit += OnObstacleHit;
        SetUpCameras();
        // As in pygame: Single shows 5 random coins of the ones placed, Doubles none; every race has 4 random obstacles.
        KeepRandom(FindObjectsByType<Coin>(), doubles ? 0 : CoinsShown);
        KeepRandom(FindObjectsByType<Obstacle>(), ObstaclesShown);

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

    // Leaves `keep` random items on and turns the rest off.
    private static void KeepRandom(Component[] items, int keep)
    {
        var shown = new List<Component>(items);
        while (shown.Count > keep)
        {
            int i = Random.Range(0, shown.Count);
            shown[i].gameObject.SetActive(false);
            shown.RemoveAt(i);
        }
    }

    private void FixedUpdate()
    {
        if (racing) tracker.Update();
    }

    private void Update()
    {
        pointsText.text = doubles
            ? $"Car 1: {racer1.points}\nCar 2: {racer2.points}"
            : $"Race: {racer1.points}";
    }

    private Racer RacerFor(CarController car)
    {
        return car == car1 ? racer1 : racer2;
    }

    private void OnWallHit(CarController car)
    {
        if (racing) tracker.WallHit(RacerFor(car));
    }

    private void OnObstacleHit(CarController car)
    {
        if (racing) tracker.ObstacleHit(RacerFor(car));
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

        tracker.FinishLap(RacerFor(car));
        StartCoroutine(EndLap());
    }

    private IEnumerator EndLap()
    {
        SetCarsDriving(false);
        int lapsDone = racer1.lapsWon + racer2.lapsWon;
        if (lapsDone >= map.requiredLaps)
        {
            Finish();
            yield break;
        }

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
        tracker.FinishRace(map.requiredLaps);
        int car1Wins = racer1.lapsWon;
        int car2Wins = racer2.lapsWon;
        if (car1Wins > car2Wins)
            resultText.text = doubles ? "Car 1 wins!" : "You win!";
        else if (car1Wins < car2Wins)
            resultText.text = doubles ? "Car 2 wins!" : "Bot wins!";
        else
            resultText.text = "It's a tie!";
        resultText.text += $"<size=50%>\n{racer1.name}: {racer1.points} race points\n{racer2.name}: {racer2.points} race points</size>";

        // Prize coins and race points are saved for Single only, as in pygame.
        if (!doubles)
        {
            if (car1Wins >= car2Wins)
                SaveService.Data.coins += car1Wins > car2Wins ? WinPrize : TiePrize;
            SaveService.Data.AddRaceResult(racer1.points, racer2.points);
            SaveService.Save();
            ShowCoins();
        }

        lapText.gameObject.SetActive(false);
        finishPanel.SetActive(true);
    }

    // Coin.Collected and Obstacle.Hit are static, so they have to be let go of when the map scene closes.
    private void OnDestroy()
    {
        Coin.Collected -= ShowCoins;
        Obstacle.Hit -= OnObstacleHit;
    }

    private void ShowCoins()
    {
        coinsText.text = $"Coins: {SaveService.Data.coins}";
    }

    private void ShowLap()
    {
        lapText.text = $"Lap {racer1.lapsWon + racer2.lapsWon + 1}/{map.requiredLaps}";
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
