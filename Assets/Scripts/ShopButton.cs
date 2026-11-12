using UnityEngine;
using UnityEngine.UI;

// The price button under a car or map picture in the menu, ported from the pygame shop.
// While the item is locked: the lock picture shows, the item can't be picked, and this button buys it.
// After buying (or for a free item): the lock and this button are hidden and the item can be picked.
// Set either `car` or `map`.
[RequireComponent(typeof(Button))]
public class ShopButton : MonoBehaviour
{
    [SerializeField] private CarDefinition car;
    [SerializeField] private MapConfig map;
    [SerializeField] private Button selectButton; // the button that picks the car or map
    [SerializeField] private GameObject lockImage;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(Buy);
    }

    private void OnEnable()
    {
        Refresh();
    }

    // Not enough coins: nothing happens, as in pygame.
    private void Buy()
    {
        SaveData data = SaveService.Data;
        bool bought = car != null ? data.TryBuyCar(car) : data.TryBuyMap(map);
        if (!bought) return;

        SaveService.Save();
        AudioManager.Instance.PlayEffect("purchase");
        Refresh();
    }

    private void Refresh()
    {
        SaveData data = SaveService.Data;
        bool owned = car != null ? data.OwnsCar(car) : data.OwnsMap(map);
        lockImage.SetActive(!owned);
        selectButton.interactable = owned;
        gameObject.SetActive(!owned);
    }
}
