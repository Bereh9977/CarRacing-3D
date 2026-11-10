using UnityEngine;

// Follows a car from behind and above.
public class CarCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 4f, -9f); // relative to the car
    [SerializeField] private float smoothing = 5f;

    private void LateUpdate()
    {
        Vector3 wanted = target.TransformPoint(offset);
        transform.position = Vector3.Lerp(transform.position, wanted, smoothing * Time.deltaTime);
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}
