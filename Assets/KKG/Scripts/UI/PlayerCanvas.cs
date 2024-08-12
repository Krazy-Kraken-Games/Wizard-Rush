using KrazyKrakenGames.Multiplayer.Data;
using TMPro;
using UnityEngine;

namespace KrazyKrakenGames.UI
{
    /// <summary>
    /// This script will be responsible for visualizing the values
    /// on the player via UI Canvas on top of player
    /// </summary>
    public class PlayerCanvas : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI playerNameText;

        [SerializeField] private TextMeshProUGUI pickedStatus;

        public void PopulateDisplay(PlayerData _data)
        {
            playerNameText.text = _data.playerName.ToString();
        }

        public void PopulateObjectPick(string objectName)
        {
            pickedStatus.text = objectName;
        }
    }
}
