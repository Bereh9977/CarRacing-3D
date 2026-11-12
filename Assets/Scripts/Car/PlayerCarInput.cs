using UnityEngine;
using UnityEngine.InputSystem;

// Passes keyboard input to the car: Player1 actions (WASD) or Player2 actions (arrows).
[RequireComponent(typeof(CarController))]
public class PlayerCarInput : MonoBehaviour
{
    [SerializeField] private InputActionReference throttle;
    [SerializeField] private InputActionReference steer;

    private CarController car;

    // Player 2 in Doubles gets the Player2 actions (arrows).
    public void SetActions(InputActionReference throttle, InputActionReference steer)
    {
        this.throttle = throttle;
        this.steer = steer;
        throttle.action.Enable();
        steer.action.Enable();
    }

    private void Awake()
    {
        car = GetComponent<CarController>();
    }

    private void OnEnable()
    {
        throttle.action.Enable();
        steer.action.Enable();
    }

    private void Update()
    {
        car.SetInput(throttle.action.ReadValue<float>(), steer.action.ReadValue<float>());
    }
}
