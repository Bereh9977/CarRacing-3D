using UnityEngine;

// Drives a car along the BotRoute points, ported from the pygame Bots class:
// full throttle, steering towards the next point, which counts as reached within a few metres.
[RequireComponent(typeof(CarController))]
public class BotDriver : MonoBehaviour
{
    private const float ReachDistance = 8f; // m; route points are 10 m apart
    private const float FullSteerAngle = 30f; // degrees off the target that give full steering

    private CarController car;
    private Transform[] points;
    private int currentPoint;

    public void SetRoute(Transform route)
    {
        points = new Transform[route.childCount];
        for (int i = 0; i < points.Length; i++)
            points[i] = route.GetChild(i);
        currentPoint = 0;
    }

    // Called when the car goes back to the start for a new lap.
    public void ResetRoute()
    {
        currentPoint = 0;
    }

    private void Awake()
    {
        car = GetComponent<CarController>();
    }

    private void Update()
    {
        if (currentPoint >= points.Length)
        {
            car.SetInput(0f, 0f);
            return;
        }

        Vector3 toTarget = points[currentPoint].position - transform.position;
        toTarget.y = 0f;
        if (toTarget.magnitude < ReachDistance)
        {
            currentPoint++;
            return;
        }

        float angle = Vector3.SignedAngle(transform.forward, toTarget, Vector3.up);
        car.SetInput(1f, Mathf.Clamp(angle / FullSteerAngle, -1f, 1f));
    }
}
