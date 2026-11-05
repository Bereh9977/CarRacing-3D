using UnityEngine;

public class ModeSelector : MonoBehaviour
{
    [SerializeField] private PanelNavigator navigator;
    [SerializeField] private GameObject carsPanel;
    [SerializeField] private GameObject mapsPanel;

    public void SelectSingle()
    {
        GameSession.Mode = GameMode.Single;
        navigator.Show(carsPanel);    
    }

    public void SelectDoubles()
    {
        GameSession.Mode = GameMode.Doubles;
        navigator.Show(mapsPanel);
    }
}
