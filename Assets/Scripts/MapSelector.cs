using UnityEngine;
using UnityEngine.SceneManagement;   // for choose scene (future)

public class MapSelector : MonoBehaviour
{
    public void SelectMap(string mapId)
    {
        GameSession.MapId = mapId;

        // Choose map by id (future)
        SceneManager.LoadScene(mapId);
    }
}
