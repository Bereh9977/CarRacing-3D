using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The rating in the main menu, ported from pygame: the player and the Bot sorted by their race points (Bot above
// when equal, so the player has to get more to beat it), and a mission line under them. Refreshes when the panel opens.
public class RatingPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text firstName;
    [SerializeField] private TMP_Text firstPoints;
    [SerializeField] private Image firstAvatar;
    [SerializeField] private TMP_Text secondName;
    [SerializeField] private TMP_Text secondPoints;
    [SerializeField] private Image secondAvatar;
    [SerializeField] private TMP_Text missionText;
    [SerializeField] private Sprite playerIcon;
    [SerializeField] private Sprite botIcon;

    private const string PlayerName = "You";
    private const string BotName = "Bot";

    private void OnEnable()
    {
        SaveData data = SaveService.Data;
        bool playerFirst = data.racePoints > data.botRacePoints;

        firstName.text = playerFirst ? PlayerName : BotName;
        firstPoints.text = (playerFirst ? data.racePoints : data.botRacePoints).ToString();
        firstAvatar.sprite = playerFirst ? playerIcon : botIcon;
        secondName.text = playerFirst ? BotName : PlayerName;
        secondPoints.text = (playerFirst ? data.botRacePoints : data.racePoints).ToString();
        secondAvatar.sprite = playerFirst ? botIcon : playerIcon;

        int toGo = data.PointsToFirstPlace;
        missionText.text = toGo == 0
            ? $"Mission complete! You beat {BotName}!"
            : $"Mission: beat {BotName}! {toGo} race points to go";
    }
}
