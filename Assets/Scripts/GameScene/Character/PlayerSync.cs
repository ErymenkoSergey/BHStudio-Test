using UnityEngine;
using Mirror;
using System.Collections;
using TMPro;
using BHStudio.Test.Mechanics.NetWork;
using BHStudio.Test.Data;
using BHStudio.Test.Interface;

namespace BHStudio.Test.Mechanics.Character
{
    public sealed class PlayerSync : BaseCharacter, IMoveble, ICharacterable
    {
        [SyncVar(hook = nameof(UpdatePlayerName))] public string PlayerName;
        [SyncVar(hook = nameof(UpdateHitCount))] public int Score;
        [SyncVar(hook = nameof(UpdateColor))] public Color PlayerColor;
        [SyncVar(hook = nameof(UpdateWinStatus))] public string Winner;

        [SyncVar] public bool IsInvincibilityMode;
        [SyncVar] public bool IsDashing;
        [SyncVar] public bool IsWinerFinded;

        [SerializeField] private TMP_Text _playerNameText;
        [SerializeField] private TMP_Text _playerScoreText;

        [SerializeField] private float _durationInvincibilityMode;
        [SerializeField] private float _dashDistance;
        [SerializeField] private int _winsToWin;
        private bool _isPushAway;

        [SerializeField] private float _rotationSpeed;
        [SerializeField] private float _moveSpeed;

        private float _sensitivityCamera = 0.25f;
        private Vector3 _mousePosition = new Vector3(255, 255, 255);

        private bool _enemyDetected;
        private float _dashTimeDetected = 1f;
        private float _horizontalMoved;
        private float _verticalMoved;
        private Vector3 _direction;

        public override void OnStartLocalPlayer()
        {
            _characterController.enabled = true;

            var player = SetReferences();
            CmdSetPlayerName(player.GetName());
        }

        private GameNetConfigurator SetReferences()
        {
            SceneObserver observer = FindObjectOfType<SceneObserver>();

            if (observer == null)
                return null;

            _sceneObserver = observer;
            SetData(observer.GetGameNetConfigurator().GetConfiguration());
            _camera = observer.GetCamera();
            _camera.transform.SetParent(_cameraPosition);
            _camera.transform.localPosition = Vector3.zero;

            var input = observer.GetInput();
            input.SetPlayer(gameObject);
            var _netConfigurator = observer.GetGameNetConfigurator();

            return _netConfigurator;
        }

        private void SetData(Configuration configuration)
        {
            _dashDistance = configuration.JerkDistance;
            _durationInvincibilityMode = configuration.DurationInvincibilityMode;
            _winsToWin = configuration.CountWinsToWin;
        }

        private void FixedUpdate()
        {
            if (!isOwned)
                return;

            if (!isLocalPlayer || _characterController == null || !_characterController.enabled)
                return;

            Vector3 direction = new Vector3(_horizontalMoved, 0, _verticalMoved);
            direction = Vector3.ClampMagnitude(direction, 1f);
            direction = transform.TransformDirection(direction);
            direction *= _moveSpeed;
            _direction = direction;

            _characterController.SimpleMove(direction);

            if (direction.magnitude > 0.1f)
            {
                SetAnimatorStatus("State", 1);
            }
            else
                SetAnimatorStatus("State", 0);
        }

        public void SetAnimatorStatus(string name, int value)
        {
            _animator.SetInteger(name, value);
        }

        public string GetName()
        {
            return PlayerName;
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.gameObject.name == "Plane" || _enemyDetected)
                return;

            if (hit.gameObject.TryGetComponent(out ICharacterable character))
            {
                if (IsDashing && isLocalPlayer && !_enemyDetected)
                {
                    _enemyDetected = true;
                    StartCoroutine(CollisionTimer());

                    if (IsDashing)
                    {
                        if (isServer)
                            SetAddScore();
                        else
                            CmdAddScore();
                    }
                }

                if (character.GetIsInvincibilityMode())
                    return;

                character.SetInvincibilityStatusOn();
            }
        }

        [Client]
        private IEnumerator CollisionTimer()
        {
            yield return new WaitForSeconds(_dashTimeDetected);
            _enemyDetected = false;
        }

        private IEnumerator Dash()
        {
            if (isServer)
                SetDashBool(true);
            else
                CmdSetDashBool(true);

            if (!IsInvincibilityMode)
                CmdSetColor(Color.blue);

            Vector3 currentPosition = transform.position;

            SetAnimatorStatus("State", 5);

            while (Vector3.Distance(transform.position, currentPosition) < _dashDistance && IsDashing)
            {
                _characterController.Move(_moveSpeed * 2f * Time.deltaTime * _direction);
                yield return null;
            }

            yield return new WaitForSeconds(_dashTimeDetected);

            SetAnimatorStatus("State", 1);

            if (!IsInvincibilityMode)
                CmdSetColor(Color.white);

            if (isServer)
                SetDashBool(false);
            else
                CmdSetDashBool(false);
        }

        public bool GetIsInvincibilityMode()
        {
            return IsInvincibilityMode;
        }

        [Server]
        private void SetColor(Color newColor)
        {
            PlayerColor = newColor;
        }

        [Command]
        private void CmdSetColor(Color newColor)
        {
            SetColor(newColor);
        }

        [Server]
        private void SetDashBool(bool isOn)
        {
            IsDashing = isOn;
        }

        [Command]
        private void CmdSetDashBool(bool isOn)
        {
            SetDashBool(isOn);
        }

        [Server]
        private void SetAddScore()
        {
            Score++;
        }

        [Command]
        private void CmdAddScore()
        {
            SetAddScore();
        }

        [Command]
        private void CmdSetPlayerName(string name)
        {
            PlayerName = name;
        }

        [Client]
        private void UpdatePlayerName(string oldName, string newName)
        {
            _playerNameText.text = newName;
        }

        [Client]
        private void UpdateColor(Color oldColor, Color newColor)
        {
            _meshRenderer.material.color = newColor;
            _playerNameText.color = newColor;
            _playerScoreText.color = newColor;
        }

        [Client]
        private void UpdateWinStatus(string oldName, string newName)
        {
            IsWinerFinded = true;
            _sceneObserver.Winn(newName);
        }

        [Client]
        private void UpdateHitCount(int oldScore, int newScore)
        {
            _playerScoreText.text = $"Score: {newScore}";

            if (newScore >= _winsToWin)
            {
                SetWinner();
            }
        }

        [Server]
        private void SetWinner()
        {
            Winner = PlayerName;
        }

        public void SetInvincibilityStatusOn()
        {
            StartCoroutine(InvincibilityTimer());
            StartCoroutine(PushAway());
        }

        private IEnumerator InvincibilityTimer()
        {
            SetNewStatusInvincibility(true);
            yield return new WaitForSeconds(_durationInvincibilityMode);
            SetNewStatusInvincibility(false);
        }

        private void SetNewStatusInvincibility(bool isOn)
        {
            if (isServer)
                SetInvincibilityMode(isOn);
            else
                CmdSetInvincibilityMode(isOn);

            IsInvincibilityMode = isOn;
        }

        [Command]
        private void CmdSetInvincibilityMode(bool isOn)
        {
            SetInvincibilityMode(isOn);
        }

        [Server]
        private void SetInvincibilityMode(bool isOn)
        {
            if (isOn)
                SetColor(Color.red);
            else
                SetColor(Color.white);
        }

        private IEnumerator PushAway()
        {
            if (_isPushAway)
                yield break;

            var time = Time.time;
            _isPushAway = true;

            while ((Time.time - time) > _durationInvincibilityMode)
            {
                _characterController.Move(_direction);
                yield return null;
            }

            _isPushAway = false;
        }

        public void Move(Controls controls, bool isOn)
        {
            if (IsWinerFinded)
            {
                _verticalMoved = _horizontalMoved = 0f;
                return;
            }

            if (!isLocalPlayer)
                return;

            switch (controls)
            {
                case Controls.Up:
                    _verticalMoved = isOn ? 1f : 0f;
                    break;
                case Controls.Down:
                    _verticalMoved = isOn ? -1f : 0f;
                    break;
                case Controls.Left:
                    _horizontalMoved = isOn ? -1f : 0f;
                    break;
                case Controls.Right:
                    _horizontalMoved = isOn ? 1f : 0f;
                    break;
            }
        }

        public void Rotate(Vector2 vector)
        {
            if (!isLocalPlayer || !isOwned || IsWinerFinded)
                return;

            RotateBody(vector);
            RotateCamera(vector);
        }

        private void RotateCamera(Vector2 vector)
        {
            _mousePosition = new Vector3(-vector.y * (_sensitivityCamera / 5f), vector.x * (_sensitivityCamera / 5f), 0);
            _mousePosition = new Vector3(_mousePosition.x, 0, 0);
            _camera.transform.localEulerAngles = _mousePosition;
        }

        private void RotateBody(Vector2 vector)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, vector.x * _sensitivityCamera, 0f), _rotationSpeed * Time.deltaTime);
        }

        public void Bounce()
        {
            if (IsWinerFinded || !isLocalPlayer)
                return;

            StartCoroutine(Dash());
        }
    }
}