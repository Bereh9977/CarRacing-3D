using UnityEngine;

// An obstacle on the road, ported from pygame. Only cars driven by people are affected, not the bot.
// A banana spins the car for 1 s and disappears. Sand and ice stay and make the car drift while on them, ice more.
// Needs a trigger collider on the Obstacle layer.
public class Obstacle : MonoBehaviour
{
    [SerializeField] private ObstacleType type;

    private const float SpinSeconds = 1f;
    private const float SandDrift = 135f; // degrees per second, a bit more than steering
    private const float IceDrift = 270f;

    public ObstacleType Type => type;

    private void OnTriggerEnter(Collider other)
    {
        CarController car = HumanCar(other);
        if (car == null) return;

        if (type == ObstacleType.BananaPeel)
        {
            car.Spin(SpinSeconds);
            Destroy(gameObject);
        }
        else if (type == ObstacleType.SandPile)
        {
            car.SetDrift(SandDrift);
        }
        else if (type == ObstacleType.IceCube)
        {
            car.SetDrift(IceDrift);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        CarController car = HumanCar(other);
        if (car != null && type != ObstacleType.BananaPeel) car.SetDrift(0f);
    }

    // The bot has no PlayerCarInput.
    private static CarController HumanCar(Collider other)
    {
        if (other.GetComponentInParent<PlayerCarInput>() == null) return null;
        return other.GetComponentInParent<CarController>();
    }
}
