using System;
using System.Collections.Generic;

// Everything the player keeps between sessions (pygame: score.txt, purchased_*.txt, race_points.json).
[Serializable]
public class SaveData
{
    public int coins;
    public List<string> purchasedCars = new List<string>();
    public List<string> purchasedMaps = new List<string>();
    public int racePoints;
    public int botRacePoints = 5000;

    public bool OwnsCar(CarDefinition car) => car.isFree || purchasedCars.Contains(car.id);

    public bool OwnsMap(MapConfig map) => map.isFree || purchasedMaps.Contains(map.sceneName);

    public bool TryBuyCar(CarDefinition car) => TryBuy(car.id, car.price, OwnsCar(car), purchasedCars);

    public bool TryBuyMap(MapConfig map) => TryBuy(map.sceneName, map.price, OwnsMap(map), purchasedMaps);

    private bool TryBuy(string id, int price, bool alreadyOwned, List<string> owned)
    {
        if (alreadyOwned || coins < price) return false;

        coins -= price;
        owned.Add(id);
        return true;
    }

    // Shown in the rating as "Mission: beat Bot! N race points to go".
    public int PointsToFirstPlace => Math.Max(0, botRacePoints - racePoints + 1);
}
