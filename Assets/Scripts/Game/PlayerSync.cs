using UnityEngine;
using Mirror;
using System.Collections;
using TMPro;
using HBStudio.Test.Mechanics.NetWork;
using HBStudio.Test.Data;
using HBStudio.Test.Interface;

namespace HBStudio.Test.Mechanics.Character
{
    public sealed class PlayerSync : BaseCharacter, IMoveble
    {
        [SyncVar(hook = nameof(UpdatePlayerName))] public string PlayerName;
        [SyncVar(hook = nameof(UpdateHitCount))] public int HitCount = 0;
        [SyncVar(hook = nameof(UpdateColor))] public Color PlayerColor;

        [SyncVar] public bool IsInvincibilityMode;
        [SyncVar] public bool IsDashing;

        [SerializeField] private TMP_Text _playerNameText;
        [SerializeField] private TMP_Text _playerScoreText;

        [SerializeField] private float _durationInvincibilityMode;
        [SerializeField] private float _dashDistance;
        [SerializeField] private int _winsToWin;

        [SerializeField] private bool _isPushAway;

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

            if (isClient && isOwned)
            {
                var players = SetReferences();
                CmdSetPlayerName(players.GetName());
            }
        }

        private GameNetConfigurator SetReferences()
        {
            SceneObserver observer = FindObjectOfType<SceneObserver>();

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

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.gameObject.name == "Plane")
                return;

            if (hit.gameObject.GetComponent<PlayerSync>() == null)
                return;

            if (_enemyDetected)
                return;

            if (IsDashing && isLocalPlayer && !_enemyDetected)
            {
                _enemyDetected = true;
                StartCoroutine(CollisionTimer());

                StartCoroutine(PushAway());

                if (IsDashing)
                {
                    if (isServer)
                        AddHitCount();
                    else
                        CmdAddHitCount();
                }

                if (hit.gameObject.GetComponent<PlayerSync>().IsDashing)
                {
                    StartCoroutine(SetDamageStatus());
                }
            }
        }

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

            IsDashing = true;

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

            IsDashing = false;
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

        private IEnumerator PushAway()
        {
            if (_isPushAway)
                yield break;

            var time = Time.time;
            _isPushAway = true;

            while (Time.time - time < 0.5f)
            {
                _characterController.Move(_direction);

                yield return null;
            }

            _isPushAway = false;
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
        private void AddHitCount()
        {
            HitCount++;
        }

        [Command]
        private void CmdAddHitCount()
        {
            AddHitCount();
        }

        [Command]
        private void CmdSetPlayerName(string name)
        {
            PlayerName = name;
        }

        [Command]
        private void CmdSetInvincibilityMode(bool isOn)
        {
            RcpSetInvincibilityMode(isOn);
        }

        [Server]
        private void RcpSetInvincibilityMode(bool isOn)
        {
            IsInvincibilityMode = isOn;
            SetColor(isOn ? Color.grey : Color.blue);
        }

        private IEnumerator SetDamageStatus()
        {
            if (isServer)
                RcpSetInvincibilityMode(true);
            else
                CmdSetInvincibilityMode(true);

            yield return new WaitForSeconds(_durationInvincibilityMode);

            if (isServer)
                RcpSetInvincibilityMode(false);
            else
                CmdSetInvincibilityMode(false);
        }

        private void UpdatePlayerName(string oldName, string newName)
        {
            _playerNameText.text = newName;
        }

        private void UpdateColor(Color oldColor, Color newColor)
        {
            _meshRenderer.material.color = newColor;
            _playerNameText.color = newColor;
            _playerScoreText.color = newColor;
        }

        private void UpdateHitCount(int oldScore, int newScore)
        {
            _playerScoreText.text = $"Score: {newScore}";

            if (_winsToWin >= newScore)
            {
                _sceneObserver.Winn(this);
            }
        }

        public void Move(Controls controls, bool isOn)
        {
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
            if (!isLocalPlayer || !isOwned)
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
            if (!isLocalPlayer)
                return;

            StartCoroutine(Dash());
        }
    }
}