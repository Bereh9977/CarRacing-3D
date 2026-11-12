using UnityEngine;

// On an options panel (main menu Settings and the pause menu). Its music On / Off buttons call these,
// because AudioManager isn't in any scene and can't be picked in a button's On Click. The volume slider has VolumeSlider.
public class OptionsPanel : MonoBehaviour
{
    public void MusicOn()
    {
        AudioManager.Instance.SetMusicOn(true);
    }

    public void MusicOff()
    {
        AudioManager.Instance.SetMusicOn(false);
    }
}
