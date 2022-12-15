using BHStudio.Test.Interface;
using BHStudio.Test.Other;
using UnityEngine;
using UnityEngine.InputSystem;

public enum Controls
{
    None = 0,

    Bounce = 1,

    Up = 2,
    Down = 3,
    Left = 4,
    Right = 5
}

namespace BHStudio.Test.Mechanics.Inputs
{
    public class InputControl : CommonBehaviour
    {
        [SerializeField] private GameObject _player;
        private IMoveble _iMoveblePlayer;

        [SerializeField] private InputActionAsset _inputActions;
        private InputActionMap _playerActionMap;

        private InputAction _forward;
        private InputAction _back;
        private InputAction _left;
        private InputAction _right;
        private InputAction _mousePos;
        private InputAction _bounce;

        private bool _isPlayerOn;

        private void OnEnable()
        {
            SetLinks();
            Subscribe();
        }

        private void OnDisable()
        {
            UnSubscribe();
        }

        public void SetPlayer(GameObject player)
        {
            _player = player;
            SetPlayer();
        }

        private void SetPlayer()
        {
            if (_player.gameObject.TryGetComponent(out IMoveble moveble))
            {
                _iMoveblePlayer = moveble;
                _isPlayerOn = true;
            }
        }

        private void SetLinks()
        {
            _playerActionMap = _inputActions.FindActionMap("Player");

            _forward = _playerActionMap.FindAction("ForwardMove");
            _back = _playerActionMap.FindAction("BackMove");
            _left = _playerActionMap.FindAction("LeftMove");
            _right = _playerActionMap.FindAction("RightMove");

            _mousePos = _playerActionMap.FindAction("Mouse");
            _bounce = _playerActionMap.FindAction("Bounce");
        }

        private void Subscribe()
        {
            _forward.started += UpMove;
            _forward.canceled += UpMove;
            _back.started += DownMove;
            _back.canceled += DownMove;
            _left.started += LeftMove;
            _left.canceled += LeftMove;
            _right.started += RightMove;
            _right.canceled += RightMove;
            _mousePos.started += MousePosition;
            _mousePos.performed += MousePosition;
            _mousePos.canceled += MousePosition;
            _bounce.started += Bounce;

            _playerActionMap.Enable();
            _inputActions.Enable();
        }

        private void UnSubscribe()
        {
            _forward.started -= UpMove;
            _forward.canceled -= UpMove;
            _back.started -= DownMove;
            _back.canceled -= DownMove;
            _left.started -= LeftMove;
            _left.canceled -= LeftMove;
            _right.started -= RightMove;
            _right.canceled -= RightMove;
            _mousePos.started -= MousePosition;
            _mousePos.performed -= MousePosition;
            _mousePos.canceled -= MousePosition;
            _bounce.started -= Bounce;

            _playerActionMap.Disable();
            _inputActions.Disable();
        }

        private void UpMove(InputAction.CallbackContext Context)
        {
            if (Context.started)
            {
                _iMoveblePlayer.Move(Controls.Up, true);
            }
            if (Context.canceled)
            {
                _iMoveblePlayer.Move(Controls.Up, false);
            }
        }

        private void DownMove(InputAction.CallbackContext Context)
        {
            if (Context.started)
            {
                _iMoveblePlayer.Move(Controls.Down, true);
            }
            if (Context.canceled)
            {
                _iMoveblePlayer.Move(Controls.Down, false);
            }
        }

        private void LeftMove(InputAction.CallbackContext Context)
        {
            if (Context.started)
            {
                _iMoveblePlayer.Move(Controls.Left, true);
            }
            if (Context.canceled)
            {
                _iMoveblePlayer.Move(Controls.Left, false);
            }
        }

        private void RightMove(InputAction.CallbackContext Context)
        {
            if (Context.started)
            {
                _iMoveblePlayer.Move(Controls.Right, true);
            }
            if (Context.canceled)
            {
                _iMoveblePlayer.Move(Controls.Right, false);
            }
        }

        private void MousePosition(InputAction.CallbackContext Context)
        {
            if (!_isPlayerOn)
                return;

            var pos = Context.ReadValue<Vector2>();
            _iMoveblePlayer.Rotate(pos);
        }

        private void Bounce(InputAction.CallbackContext Context)
        {
            _iMoveblePlayer.Bounce();
        }
    }
}
