using HBStudio.Test.Mechanics.Character;
using HBStudio.Test.Mechanics.Inputs;
using HBStudio.Test.Other;
using HBStudio.Test.UI;
using Mirror;
using System.Collections;
using UnityEngine;

namespace HBStudio.Test.Mechanics.NetWork
{
    public sealed class SceneObserver : CommonBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private GameNetConfigurator _netConfigurator;
        [SerializeField] private GameUI _gameUI;
        [SerializeField] private InputControl _inputControl;
        [SerializeField] private float _timeWaitWin = 15f;

        private void Awake()
        {
            FindNetConfigurator();
        }

        private void FindNetConfigurator()
        {
            _netConfigurator = FindObjectOfType<GameNetConfigurator>();
        }

        public InputControl GetInput()
        {
            return _inputControl;
        }

        public GameNetConfigurator GetGameNetConfigurator()
        {
            return _netConfigurator;
        }

        public Camera GetCamera()
        {
            return _camera;
        }

        public void Winn(string myPlayer)
        {
            _gameUI.OpenPausePanel(true);
            _gameUI.SetWinText($"{myPlayer} Winner!");

            StartCoroutine(WinTimer());
        }

        public IEnumerator WinTimer()
        {
            Debug.Log($"WinTimer Start, Wait: {_timeWaitWin} seconds ");
            yield return new WaitForSeconds(_timeWaitWin);
            NetworkManager.singleton.ServerChangeScene(NetworkManager.networkSceneName);
        }
    }
}