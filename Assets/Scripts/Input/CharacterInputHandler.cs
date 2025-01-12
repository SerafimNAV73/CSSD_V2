using UnityEngine;

namespace CSSD
{
    public class CharacterInputHandler : MonoBehaviour
    {
        private Vector2 _moveVector = Vector2.zero;
        private Vector2 _viewVector = Vector2.zero;
        private bool _isFireButtonPressed = false;
        private bool _isReloadButtonPressed = false;
        private bool _isMenuButtonPressed = false;
        private bool _isFollowCallButtonPressed = false;
        private bool _isHelpCallButtonPressed = false;
        private bool _isAgreeCallButtonPressed = false;
        private bool _isDisagreeCallButtonPressed = false;

        private Vector2 _moveInputVector = Vector2.zero;
        private Vector2 _viewInputVector = Vector2.zero;
        private float _shootInput;
        private float _reloadInput;
        private float _pauseInput;
        private float _followCallInput;
        private float _helpCallInput;
        private float _agreeCallInput;
        private float _disagreeCallInput;

        //Other components
        private LocalCameraHandler _localCameraHandler;
        private NetworkPlayer _player;

        private PlayerInputActionPC _playerInput;

        private void Awake()
        {
            _localCameraHandler = GetComponentInChildren<LocalCameraHandler>();
            _player = GetComponent<NetworkPlayer>();
        }

        private void OnEnable()
        {
            _playerInput = new PlayerInputActionPC();

            _playerInput.Enable();
            _playerInput.Player.Movement.performed += context => { _moveInputVector = context.ReadValue<Vector2>(); };
            _playerInput.Player.Movement.started += context => { _moveInputVector = context.ReadValue<Vector2>(); };
            _playerInput.Player.Movement.canceled += context => { _moveInputVector = context.ReadValue<Vector2>(); };

            _playerInput.Player.Aim.performed += context => { _viewInputVector = _localCameraHandler.LocalCamera.ScreenToWorldPoint(context.ReadValue<Vector2>()); };
            _playerInput.Player.Aim.started += context => { _viewInputVector = _localCameraHandler.LocalCamera.ScreenToWorldPoint(context.ReadValue<Vector2>()); };
            _playerInput.Player.Aim.canceled += context => { _viewInputVector = _localCameraHandler.LocalCamera.ScreenToWorldPoint(context.ReadValue<Vector2>()); };

            _playerInput.Player.Fire.performed += context => { _shootInput = context.ReadValue<float>(); };
            _playerInput.Player.Fire.started += context => { _shootInput = context.ReadValue<float>(); };
            _playerInput.Player.Fire.canceled += context => { _shootInput = context.ReadValue<float>(); };

            _playerInput.Player.Reload.performed += context => { _reloadInput = context.ReadValue<float>(); };
            _playerInput.Player.Reload.started += context => { _reloadInput = context.ReadValue<float>(); };
            _playerInput.Player.Reload.canceled += context => { _reloadInput = context.ReadValue<float>(); };

            _playerInput.Player.Pause.performed += context => { _pauseInput = context.ReadValue<float>(); };
            _playerInput.Player.Pause.started += context => { _pauseInput = context.ReadValue<float>(); };
            _playerInput.Player.Pause.canceled += context => { _pauseInput = context.ReadValue<float>(); };

            _playerInput.Player.FirstCall.performed += context => { _followCallInput = context.ReadValue<float>(); };
            _playerInput.Player.FirstCall.started += context => { _followCallInput = context.ReadValue<float>(); };
            _playerInput.Player.FirstCall.canceled += context => { _followCallInput = context.ReadValue<float>(); };

            _playerInput.Player.SecondCall.performed += context => { _helpCallInput = context.ReadValue<float>(); };
            _playerInput.Player.SecondCall.started += context => { _helpCallInput = context.ReadValue<float>(); };
            _playerInput.Player.SecondCall.canceled += context => { _helpCallInput = context.ReadValue<float>(); };

            _playerInput.Player.ThirdCall.performed += context => { _agreeCallInput = context.ReadValue<float>(); };
            _playerInput.Player.ThirdCall.started += context => { _agreeCallInput = context.ReadValue<float>(); };
            _playerInput.Player.ThirdCall.canceled += context => { _agreeCallInput = context.ReadValue<float>(); };

            _playerInput.Player.FourthCall.performed += context => { _disagreeCallInput = context.ReadValue<float>(); };
            _playerInput.Player.FourthCall.started += context => { _disagreeCallInput = context.ReadValue<float>(); };
            _playerInput.Player.FourthCall.canceled += context => { _disagreeCallInput = context.ReadValue<float>(); };
        }

        private void OnDisable()
        {
            _playerInput.Disable();
        }

        private void Update()
        {
            if (!_player.Object.HasInputAuthority)             
                return;

            //Move and rotate
            _moveVector = _moveInputVector;
            _viewVector = _viewInputVector;

            //Fire
            if (_shootInput > 0f)
            {
                Debug.Log("FireInput");
                _isFireButtonPressed = true;
            }

            //Reload
            if (_reloadInput > 0f)
            {
                Debug.Log("ReloadInput");
                _isReloadButtonPressed = true;
            }

            //Pause
            if (_pauseInput > 0f)
            {
                Debug.Log("PauseInput");
                _isMenuButtonPressed = true;
            }

            //Follow call
            if (_followCallInput > 0f)
            {
                Debug.Log("FollowCallInput");
                _isFollowCallButtonPressed = true;
            }

            //Help call
            if (_helpCallInput > 0f)
            {
                Debug.Log("HelpCallInput");
                _isHelpCallButtonPressed = true;
            }

            //Follow call
            if (_agreeCallInput > 0f)
            {
                Debug.Log("AgreeCallInput");
                _isAgreeCallButtonPressed = true;
            }

            //Follow call
            if (_disagreeCallInput > 0f)
            {
                Debug.Log("DisagreeCallInput");
                _isDisagreeCallButtonPressed = true;
            }
        }

        public NetworkInputData GetNetworkInput()
        {
            NetworkInputData networkInputData = new NetworkInputData();

            //Aim data
            networkInputData.aimForwardVector = _viewVector;

            //Move data
            networkInputData.movementInput = _moveVector;

            //Fire data
            networkInputData.isFirePressed = _isFireButtonPressed;

            //Reload data
            networkInputData.isReloadButtonPressed = _isReloadButtonPressed;

            //Menu data
            networkInputData.isMenuButtonPressed = _isMenuButtonPressed;

            //Calls data
            networkInputData.isFollowCallButtonPressed = _isFollowCallButtonPressed;
            networkInputData.isHelpCallButtonPressed = _isHelpCallButtonPressed;
            networkInputData.isAgreeCallButtonPressed = _isAgreeCallButtonPressed;
            networkInputData.isDisagreeCallButtonPressed = _isDisagreeCallButtonPressed;

            //Reset variables now that we have read their states
            _isReloadButtonPressed = false;
            _isFireButtonPressed = false;
            _isMenuButtonPressed = false;
            _isFollowCallButtonPressed = false;
            _isHelpCallButtonPressed = false;
            _isAgreeCallButtonPressed = false;
            _isDisagreeCallButtonPressed = false;


            return networkInputData;
        }
    }
}