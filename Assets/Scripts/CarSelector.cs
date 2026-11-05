using UnityEngine;

public class CarSelector : MonoBehaviour
{
    [SerializeField] private PanelNavigator navigator;
    [SerializeField] private GameObject mapsPanel;

    public void SelectCar(string carId)
    {
        GameSession.CarId = carId;
        navigator.Show(mapsPanel);   
    }
}
