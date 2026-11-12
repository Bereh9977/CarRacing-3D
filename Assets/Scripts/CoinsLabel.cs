using TMPro;
using UnityEngine;

// Shows the player's coins in the menu. It reads the save every frame, so it's always up to date, e.g. after buying a car.
[RequireComponent(typeof(TMP_Text))]
public class CoinsLabel : MonoBehaviour
{
    private TMP_Text label;

    private void Awake()
    {
        label = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        label.text = $"Coins: {SaveService.Data.coins}";
    }
}
