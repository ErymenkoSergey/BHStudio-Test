using HBStudio.Test.Other;
using UnityEngine;
using TMPro;

namespace HBStudio.Test.UI
{
    public class GameUI : CommonBehaviour
    {
        [SerializeField] private GameObject _pausePanel;
        [SerializeField] private TextMeshProUGUI _winText;

        public void OpenPausePanel(bool isOpen)
        {
            _pausePanel.SetActive(isOpen);
        }

        public void SetWinText(string text)
        {
            _winText.text = text;
        }
    }
}