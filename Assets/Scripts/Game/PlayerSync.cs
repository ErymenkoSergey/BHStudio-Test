using UnityEngine;
using Mirror;
using System.Collections;
using TMPro;
using HBStudio.Test.Mechanics.NetWork;
using HBStudio.Test.Data;

namespace HBStudio.Test.Mechanics.Character
{
    public class PlayerSync : BaseCharacter
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

        [SerializeField] private float _turnSmoothTime = 0.01f;
        [SerializeField] private bool _isPushAway;

        [SerializeField] private float _rotationSpeed;
        [SerializeField] private float _jumpSpeed;
        [SerializeField] private float _moveSpeed;
        [SerializeField] private float _turnSensitivity;

        private bool _enemyDetected;
        private float _dashTimeDetected = 1f;
        private bool _isGrounded = true;
        private bool isFail;
        private float _horizontal;
        private float _vertical;
        private Vector3 _direction;

        public override void OnStartLocalPlayer()
        {
            _characterController.enabled = true;

            var players = SetReferences();

            if (isClient && isOwned)
                CmdSetPlayerName(players.GetName());

            if (isOwned)
            {
                _cinemachine.Follow = transform;
                _cinemachine.LookAt = transform;
            }
        }

        private GameNetConfigurator SetReferences()
        {
            SceneObserver observer = FindObjectOfType<SceneObserver>(); //I would fix it with an injection (Zenject)
            _sceneObserver = observer;
            _cinemachine = observer.GetCinemachine();
            var _netConfigurator = observer.GetGameNetConfigurator();

            return _netConfigurator;
        }

        public void SetData(Configuration configuration)
        {
            _dashDistance = configuration.JerkDistance;
            _durationInvincibilityMode = configuration.DurationInvincibilityMode;
            _winsToWin = configuration.CountWinsToWin;
        }

        private void Update()
        {
            if (!isOwned)
                return;

            if (!isLocalPlayer)
                return;

            if (Input.GetMouseButtonDown(0))
                StartCoroutine(Dash());

            _horizontal = Input.GetAxis("Horizontal");
            _vertical = Input.GetAxis("Vertical");

            _isGrounded = _characterController.isGrounded;

            if (_isGrounded)
                isFail = false;

            if ((_isGrounded || !isFail) && _jumpSpeed < 1f && Input.GetKey(KeyCode.Space))
            {
                _jumpSpeed = Mathf.Lerp(_jumpSpeed, 4f, 0.5f);
            }
            else if (!_isGrounded)
            {
                isFail = true;
                _jumpSpeed = 0;
            }
        }

        private void FixedUpdate()
        {
            if (!isOwned)
                return;

            if (_cinemachine != null)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, _cinemachine.m_XAxis.Value, 0f), _rotationSpeed * Time.deltaTime);

            if (!isLocalPlayer || _characterController == null || !_characterController.enabled)
                return;

            Vector3 direction = new Vector3(_horizontal, _jumpSpeed, _vertical);
            direction = Vector3.ClampMagnitude(direction, 1f);
            direction = transform.TransformDirection(direction);
            direction *= _moveSpeed;
            _direction = direction;

            if (_jumpSpeed > 0)
                _characterController.Move(direction * Time.fixedDeltaTime);
            else
                _characterController.SimpleMove(direction);
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
                else if (hit.gameObject.GetComponent<PlayerSync>().IsDashing)
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

            while (Vector3.Distance(transform.position, currentPosition) < _dashDistance && IsDashing)
            {
                _characterController.Move(_moveSpeed * 2f * Time.deltaTime * _direction);
                yield return null;
            }

            yield return new WaitForSeconds(_dashTimeDetected / 2);

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

            while (Time.time - time < 0.05f)
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
            SetInvincibilityMode(isOn);
        }

        [Server]
        private void SetInvincibilityMode(bool isOn)
        {
            IsInvincibilityMode = isOn;
            SetColor(isOn ? Color.red : Color.blue);
        }

        private IEnumerator SetDamageStatus()
        {
            if (isServer)
                SetInvincibilityMode(true);
            else
                CmdSetInvincibilityMode(true);

            yield return new WaitForSeconds(_durationInvincibilityMode);

            if (isServer)
                SetInvincibilityMode(false);
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

            if (_winsToWin == newScore)
            {
                _sceneObserver.Winn(this);
            }
        }
    }
}