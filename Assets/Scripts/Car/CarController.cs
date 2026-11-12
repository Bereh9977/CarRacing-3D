using System;
using UnityEngine;

// Arcade car movement ported from the pygame Cars class.
[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    [SerializeField] private float maxSpeed = 20f;          // m/s
    [SerializeField] private float maxReverseSpeed = 8f;    // m/s
    [SerializeField] private float acceleration = 12f;      // m/s²
    [SerializeField] private float coastDeceleration = 8f;  // m/s², slowing down with no key pressed
    [SerializeField] private float turnSpeed = 120f;        // degrees per second
    [SerializeField] private float bounceFactor = 0.65f;    // share of speed kept, reversed, after hitting a wall
    [SerializeField] private float spinSpeed = 720f;        // degrees per second on a banana

    public event Action<CarController> WallHit;

    private Rigidbody body;
    private int wallLayer;
    private float speed;
    private float throttle;
    private float steer;

    // Obstacle effects, ported from pygame. While spinning the car stands still and keeps its speed for later.
    private float spinningUntil;
    private float drift;            // degrees per second, 0 when not on sand or ice
    private float driftDirection;   // -1 or 1
    private float driftChangeTime;  // when the drift picks a new direction

    public void SetMaxSpeed(float value)
    {
        maxSpeed = value;
    }

    // Both values are -1..1: throttle forward/backward, steer left/right.
    public void SetInput(float throttle, float steer)
    {
        this.throttle = throttle;
        this.steer = steer;
    }

    // Banana.
    public void Spin(float seconds)
    {
        spinningUntil = Time.time + seconds;
    }

    // Sand and ice: while on them, the car turns by itself to the left or right at `degreesPerSecond`,
    // picking a new random side every 1.5 s. 0 ends the drift.
    public void SetDrift(float degreesPerSecond)
    {
        drift = degreesPerSecond;
    }

    // Stops the car on the spot, e.g. right after it crosses the finish line. Also ends any obstacle effect.
    public void Stop()
    {
        speed = 0f;
        throttle = 0f;
        steer = 0f;
        spinningUntil = 0f;
        drift = 0f;
        body.linearVelocity = Vector3.zero;
    }

    // Puts the car back on a spawn point, standing still.
    public void ResetTo(Transform spawn)
    {
        Stop();
        body.position = spawn.position;
        body.rotation = spawn.rotation;
        transform.SetPositionAndRotation(spawn.position, spawn.rotation);
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.Continuous; // fast cars, thin walls
        wallLayer = LayerMask.NameToLayer("Wall");
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        float gravity = Mathf.Min(body.linearVelocity.y, 0f); // keep gravity, but bumps and walls can't launch the car

        if (Time.time < spinningUntil)
        {
            body.MoveRotation(body.rotation * Quaternion.Euler(0f, spinSpeed * dt, 0f));
            body.linearVelocity = new Vector3(0f, gravity, 0f);
            return;
        }

        if (throttle > 0f)
            speed = Mathf.Min(speed + acceleration * dt, maxSpeed);
        else if (throttle < 0f)
            speed = Mathf.Max(speed - acceleration * dt, -maxReverseSpeed);
        else
            speed = Mathf.MoveTowards(speed, 0f, coastDeceleration * dt);

        // Steering is mirrored when reversing, as in pygame. The pressed key decides the direction,
        // so rolling back from a wall bounce while holding W still steers forwards.
        // It fades out below 5 m/s, so the car doesn't spin on the spot.
        float direction = throttle != 0f ? Mathf.Sign(throttle) : Mathf.Sign(speed);
        float turnFactor = direction * Mathf.Min(Mathf.Abs(speed) / 5f, 1f);
        float turn = steer * turnFactor * turnSpeed;

        if (drift > 0f)
        {
            if (Time.time >= driftChangeTime)
            {
                driftDirection = UnityEngine.Random.value < 0.5f ? -1f : 1f;
                driftChangeTime = Time.time + 1.5f;
            }
            // Fades out at low speed like steering, so a slow car isn't spun on the spot and can drive off.
            turn += driftDirection * drift * Mathf.Min(Mathf.Abs(speed) / 5f, 1f);
        }
        body.MoveRotation(body.rotation * Quaternion.Euler(0f, turn * dt, 0f));

        Vector3 velocity = body.rotation * Vector3.forward * speed;
        velocity.y = gravity;
        body.linearVelocity = velocity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Only bounce when driving into the wall, so touching two wall pieces at once doesn't flip the speed back.
        bool movingIntoWall = Vector3.Dot(transform.forward * speed, collision.GetContact(0).normal) < 0f;
        if (collision.gameObject.layer != wallLayer) return;
        if (movingIntoWall) speed = -bounceFactor * speed;
        WallHit?.Invoke(this);
    }
}
