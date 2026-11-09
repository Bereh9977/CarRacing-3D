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

    private Rigidbody body;
    private int wallLayer;
    private float speed;
    private float throttle;
    private float steer;

    // Both values are -1..1: throttle forward/backward, steer left/right.
    public void SetInput(float throttle, float steer)
    {
        this.throttle = throttle;
        this.steer = steer;
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
        body.MoveRotation(body.rotation * Quaternion.Euler(0f, steer * turnFactor * turnSpeed * dt, 0f));

        Vector3 velocity = body.rotation * Vector3.forward * speed;
        velocity.y = Mathf.Min(body.linearVelocity.y, 0f); // keep gravity, but bumps and walls can't launch the car
        body.linearVelocity = velocity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Only bounce when driving into the wall, so touching two wall pieces at once doesn't flip the speed back.
        bool movingIntoWall = Vector3.Dot(transform.forward * speed, collision.GetContact(0).normal) < 0f;
        if (collision.gameObject.layer == wallLayer && movingIntoWall)
            speed = -bounceFactor * speed;
    }
}
