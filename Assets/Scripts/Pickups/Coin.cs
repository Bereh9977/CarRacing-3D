using System;
using UnityEngine;

// A coin on the road, ported from the pygame Score coins: +100 coins when the player drives through it.
// The bot doesn't collect coins. Needs a trigger collider on the Pickup layer.
public class Coin : MonoBehaviour
{
    public static event Action Collected;

    [SerializeField] private int value = 100;
    [SerializeField] private float spinSpeed = 90f; // degrees per second

    private void Update()
    {
        transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerCarInput>() == null) return; // the bot has no PlayerCarInput

        SaveService.Data.coins += value;
        SaveService.Save();
        Collected?.Invoke();
        Destroy(gameObject);
    }
}
