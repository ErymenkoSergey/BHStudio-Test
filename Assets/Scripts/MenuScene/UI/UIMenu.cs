using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Mirror;
using HBStudio.Test.Other;

namespace HBStudio.Test.UI
{
    public class UIMenu : CommonBehaviour
    {
        [SerializeField] private TextMeshProUGUI _versionBuild;
        [SerializeField] private TMP_InputField _inputNameField;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _quitButton;
        [SerializeField] private GameObject _inputPanel;
        [SerializeField] private bool _isLockMouse;
        private string _playerName;

        [SerializeField] private GameObject _gameNetConfigurator;
        private INetConfigurable _iNetConfigurable;

        private void OnEnable()
        {
            _inputNameField.onEndEdit.AddListener(ChangePlayerName);
            _continueButton.onClick.AddListener(Continue);
            _quitButton.onClick.AddListener(Quit);
        }

        private void OnDisable()
        {
            _inputNameField.onEndEdit.RemoveListener(ChangePlayerName);
            _continueButton.onClick.RemoveListener(Continue);
            _quitButton.onClick.RemoveListener(Quit);
        }

        private void Start()
        {
            SetConfiguration();
            SetBuildVersion();
            LockMouse();
        }

        private void ChangePlayerName(string playerName)
        {
            _playerName = playerName;
            _iNetConfigurable.SetNamePlayer(playerName);
        }

        private void SetConfiguration()
        {
            if (_gameNetConfigurator.TryGetComponent(out INetConfigurable configurable))
                _iNetConfigurable = configurable;
            else
                Debug.LogError($"Configuration reference == Null! {gameObject.name}");
        }

        private void Continue()
        {
            if (_playerName != "")
            {
                OpenInputPanel(false);
                _iNetConfigurable.SetNamePlayer(_playerName);
            }
            else
                Debug.Log($"Name == null");
        }

        private void SetBuildVersion()
        {
            _versionBuild.text = $"Version: {Application.version}";
        }

        private void OpenInputPanel(bool isOpen)
        {
            _inputPanel.SetActive(isOpen);
        }

        private void LockMouse()
        {
            if (_isLockMouse)
                Cursor.lockState = CursorLockMode.Confined;
        }

        private void Quit()
        {
            Application.Quit();
        }
    }
}