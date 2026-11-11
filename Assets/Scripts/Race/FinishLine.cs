using System;
using System.Collections.Generic;
using UnityEngine;

// Trigger on the start/finish road piece. Its blue (Z) axis is the driving direction.
// Reports +1 when a car crosses it forwards and -1 when it crosses backwards.
public class FinishLine : MonoBehaviour
{
    public event Action<CarController, int> Crossed;

    private readonly Dictionary<CarController, bool> enteredInFront = new Dictionary<CarController, bool>();

    private void OnTriggerEnter(Collider other)
    {
        CarController car = other.GetComponentInParent<CarController>();
        if (car != null) enteredInFront[car] = IsInFront(car);
    }

    private void OnTriggerExit(Collider other)
    {
        CarController car = other.GetComponentInParent<CarController>();
        if (car == null || !enteredInFront.ContainsKey(car)) return;

        // A car that drives in and backs out on the same side didn't cross.
        bool exitedInFront = IsInFront(car);
        if (exitedInFront != enteredInFront[car])
            Crossed?.Invoke(car, exitedInFront ? 1 : -1);
        enteredInFront.Remove(car);
    }

    private bool IsInFront(CarController car)
    {
        return Vector3.Dot(car.transform.position - transform.position, transform.forward) > 0f;
    }
}
