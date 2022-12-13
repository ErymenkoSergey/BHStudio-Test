using Cinemachine;
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
        [SerializeField] private CinemachineFreeLook _cinemachineFreeLook;
        [SerializeField] private GameNetConfigurator _netConfigurator;
        [SerializeField] private GameUI _gameUI;
        [SerializeField] private InputControl _inputControl;
        [SerializeField] private float _timeWaitWin = 5f;

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

        public CinemachineFreeLook GetCinemachine()
        {
            return _cinemachineFreeLook;
        }

        public void Winn(PlayerSync myPlayer)
        {
            _gameUI.OpenPausePanel(true);
            _gameUI.SetWinText($"{myPlayer.PlayerName} Win!");

            StartCoroutine(WinTimer());
        }

        public IEnumerator WinTimer()
        {
            yield return new WaitForSeconds(_timeWaitWin);
            NetworkManager.singleton.ServerChangeScene(NetworkManager.networkSceneName);
        }
    }
}