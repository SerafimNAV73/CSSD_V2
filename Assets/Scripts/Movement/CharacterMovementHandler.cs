using Fusion;
using UnityEngine;
using System.Collections;
using System;

namespace CSSD
{
    public class CharacterMovementHandler : NetworkBehaviour
    {
        [Header("Animator")]
        [SerializeField] private CharacterAnimatorHandler _anim;

        [Header("Components")]
        //[SerializeField] private CharacterAnimator _anim;
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private HPHandler _hpHandler;

        [SerializeField] private float _defaultWalkSpeed;
        [SerializeField] private float _currentWalkSpeed;
        [SerializeField] private AnimationCurve _curve;

        private bool _isSpawnRequested = false;
        private Vector2 _oldAim;
        private Vector3 _spawnPos;

        //Speed upgrade
        private GameObject _speedUIIcon;
        private bool _hasSpeedUpgrade = false;
        private IEnumerator _speedUpgradeCoroutine;

        public static Func<int, Vector3> getRespawnPointDel;

        private void OnValidate()
        {
            if (_defaultWalkSpeed < 0) _defaultWalkSpeed = 0;
            if (_currentWalkSpeed < 0) _currentWalkSpeed = 0;            
        }

        private void Awake()
        {
            _currentWalkSpeed = _defaultWalkSpeed;
        }

        private void OnEnable()
        {
            PauseMenuHandler.restartDel += Restart;
        }

        private void OnDisable()
        {
            PauseMenuHandler.restartDel -= Restart;
        }

        private void Start()
        {
            _spawnPos = transform.position;
        }

        public override void FixedUpdateNetwork()
        {
            if (Object.HasStateAuthority)
            {
                if (_isSpawnRequested && GameManager._instance.CurStatus == GameStatus.WaveEnd)
                {
                    Respawn();
                    return;
                }

                //Don't update the clients'position when they are dead
                if (_hpHandler.IsDead)
                    return;
            }

            Vector2 movementInput = Vector2.zero;
            Vector2 aimForward = Vector2.zero;

            //Get the input from the network
            if (GetInput(out NetworkInputData networkInputData))
            {
                aimForward = networkInputData.aimForwardVector;
                movementInput = networkInputData.movementInput;
            }

            if (Object.HasStateAuthority)
            {
                Moving(movementInput);
                Rotating(aimForward);
                RPC_ChangeTransform(transform.position, transform.rotation);

                if (_anim != null)
                    _anim.MoveAnim(movementInput, _currentWalkSpeed);
                else
                    Debug.LogWarning("Lost movement animator handler");
            }
        }

        private void Moving(Vector2 direction)
        {
            _rb.velocity = new Vector2(
                _curve.Evaluate(direction.x) * _currentWalkSpeed,
                _curve.Evaluate(direction.y) * _currentWalkSpeed);
        }

        private void Rotating(Vector2 aim)
        {
            if (aim != _oldAim)
            {
                _oldAim = aim;
                Vector2 dir = aim - _rb.position;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                _rb.MoveRotation(angle);
            }
        }

        private void Respawn()
        {
            _hpHandler.OnRespawned();
            transform.position = _spawnPos;
            AudioManager._instance.PlayerRespawnEvent(transform.position);
            RPC_RespawnSound();
            _isSpawnRequested = false;
        }

        private IEnumerator UpgradeTimer(float time, float multiplier)
        {
            yield return new WaitForSeconds(time);
            _currentWalkSpeed = _defaultWalkSpeed;
            _hasSpeedUpgrade = false;
        }

        public void SpeedUpgrade(NetworkPlayer player, float multiplier, float time, GameObject UIIcon)
        {
            if (_hasSpeedUpgrade)
            {
                StopCoroutine(_speedUpgradeCoroutine);
                if (_speedUIIcon != null) Destroy(_speedUIIcon);
                else Debug.LogError("UIIcon lost");
            }
            else
            {
                _currentWalkSpeed = _defaultWalkSpeed * multiplier;
                _hasSpeedUpgrade = true;
            }

            _speedUpgradeCoroutine = UpgradeTimer(time, multiplier);
            StartCoroutine(_speedUpgradeCoroutine);
            _speedUIIcon = Instantiate(UIIcon, player.BonusesUIRoot.transform, false);

            if (_speedUIIcon.TryGetComponent<UIBonusIcon>(out var icon))
            {
                icon.SetTime(time);
            }
            else
            {
                Debug.LogError("Lost icon bonus script");
            }
        }

        public void RequestRespawn()
        {
            _isSpawnRequested = true;
        }

        public void SetDefaultWalkSpeed(float speed)
        {
            _defaultWalkSpeed = speed;
        }

        public void Restart()
        {
            if (Runner.GameMode == GameMode.Single)
            {
                transform.position = _spawnPos;

                if (_hasSpeedUpgrade)
                {
                    _currentWalkSpeed = _defaultWalkSpeed;
                    StopCoroutine(_speedUpgradeCoroutine);
                    if (_speedUIIcon != null) Destroy(_speedUIIcon);
                    else Debug.LogError("UIIcon lost");
                }                
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_ChangeTransform(Vector3 position, Quaternion rotation)
        {
            transform.position = position;
            transform.rotation = rotation;
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_RespawnSound()
        {
            AudioManager._instance.PlayerRespawnEvent(transform.position);
        }
    }
}