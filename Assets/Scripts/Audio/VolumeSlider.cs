using UnityEngine;
using UnityEngine.UI;

// Put on the music volume slider (0..1). It shows the saved volume and changes the music volume when moved.
[RequireComponent(typeof(Slider))]
public class VolumeSlider : MonoBehaviour
{
    private void Start()
    {
        Slider slider = GetComponent<Slider>();
        slider.value = SaveService.Data.musicVolume;
        slider.onValueChanged.AddListener(AudioManager.Instance.SetVolume);
    }
}
