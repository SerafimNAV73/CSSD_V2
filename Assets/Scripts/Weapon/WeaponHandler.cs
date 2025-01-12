using System.Collections;
using UnityEngine;
using Fusion;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CSSD
{
    public class WeaponHandler : NetworkBehaviour
    {
        [Header("Animator")]
        [SerializeField] private CharacterAnimatorHandler _anim;
        [Header("Collision")]
        [SerializeField] private LayerMask _collisionLayers;

        private Settings _settings;

        //Operative
        private int _magazineCurrentCapacity = 0;
        private float _lastTimeFired = 0;

        //Statuses
        private bool _isReloading;

        private CancellationTokenSource clipReloadCancellationTokenSource;

        private bool _hasDamageUpgrade = false;
        private bool _hasReloadUpgrade = false;

        private IEnumerator _damageUpgradeCoroutine;
        private IEnumerator _reloadUpgradeCoroutine;

        private GameObject _damageUIIcon;
        private GameObject _reloadUIIcon;

        private float _defaultDamageModifier = 1;
        private float _defaultReloadModifier = 1;

        private float _currentDamageModifier;
        private float _currentReloadModifier;

        private float _maxHitDistance = 200;

        private Action shootDel;

        //Other components
        private HPHandler _hpHandler;
        private NetworkPlayer _networkPlayer;
        private ViewModel _viewModel;

        private ChangeDetector _changes;

        [Networked] public bool IsFiring { get; set; }

        private void Awake()
        {
            _hpHandler = GetComponent<HPHandler>();
            _networkPlayer = GetBehaviour<NetworkPlayer>();
            _viewModel = GetComponentInChildren<ViewModel>();
            clipReloadCancellationTokenSource = new CancellationTokenSource();
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
            _currentDamageModifier = _defaultDamageModifier;
            _currentReloadModifier = _defaultReloadModifier;

            if (Object.HasInputAuthority)
                _lastTimeFired = Time.time - 60 / (_settings.fireRate * _currentReloadModifier);
        }


        private void OnDestroy()
        {
            Dispose();
        }

        public override void FixedUpdateNetwork()
        {
            if (_hpHandler.IsDead) return;

            //Get the input from the network
            if (GetInput(out NetworkInputData networkInputData))
            {
                if (networkInputData.isFirePressed)
                {
                    Debug.Log("Fire");
                    Shoot(_networkPlayer, transform.position, transform.right, _currentDamageModifier);
                }
                if (networkInputData.isReloadButtonPressed)
                {
                    Debug.Log("Reload");
                    OnReload(_currentReloadModifier);
                }
            }
        }

        public override void Spawned()
        {
            _changes = GetChangeDetector(ChangeDetector.Source.SimulationState);
        }

        public override void Render()
        {
            foreach (var change in _changes.DetectChanges(this, out var previousBuffer, out var currentBuffer))
            {
                switch (change)
                {
                    case nameof(IsFiring):
                        var reader = GetPropertyReader<bool>(nameof(IsFiring));
                        var (previous, current) = reader.Read(previousBuffer, currentBuffer);
                        OnFireChanged(previous, current);
                        break;
                }
            }
        }

        private void OnFireChanged(bool oldValue, bool value)
        {
            //Debug.Log($"{Time.time} OnFireChanged value {IsFiring}");

            if (value && !oldValue)
                OnFireRemote();
        }

        private void OnFireRemote()
        {
            if (!Object.HasInputAuthority)
            {
                SetFiringAnim();

                AudioManager._instance.PlayerShotEvent(transform.position);
            }
        }

        private void Shoot(NetworkPlayer player, Vector2 aimPointPosition, Vector2 aimForwardVector, float damageModifier)
        {
            if (!CanShoot()) return;

            StartCoroutine(FireEffectCO());

            shootDel?.Invoke();

            HPHandler hitHPHandler = CalculateFireDirection(aimPointPosition, aimForwardVector, out Vector2 fireDirection);
            Debug.Log($"Spotted HPhandler: {hitHPHandler}");

            if (hitHPHandler != null && Object.HasStateAuthority)
                hitHPHandler.OnTakeDamage((int)(_settings.damageAmount * damageModifier), player);
        }

        private HPHandler CalculateFireDirection(Vector2 aimPointPosition, Vector2 aimForwardVector, out Vector2 fireDirection)
        {
            LagCompensatedHit hitInfo = new LagCompensatedHit();

            fireDirection = aimForwardVector;
            float hitDistance = _maxHitDistance;

            //Check if we hit anything with the fire
            Runner.LagCompensation.Raycast(aimPointPosition, aimForwardVector, _maxHitDistance, Object.InputAuthority, out hitInfo,
                _collisionLayers, HitOptions.IgnoreInputAuthority | HitOptions.IncludePhysX);

            //Check against targets
            if (hitInfo.Hitbox != null)
            {
                hitDistance = hitInfo.Distance;
                HPHandler hitHPHandler = null;

                if (Object.HasStateAuthority)
                {
                    if (hitInfo.Hitbox.transform.root.TryGetComponent(out hitHPHandler))
                    {
                        Debug.DrawRay(aimPointPosition, fireDirection * hitDistance, Color.red, 1);

                        return hitHPHandler;
                    }
                }

            }
            else if (hitInfo.Collider != null)
            {
                hitDistance = hitInfo.Distance;

                Debug.DrawRay(aimPointPosition, fireDirection * hitDistance, Color.green, 1);
            }
            else Debug.DrawRay(aimPointPosition, fireDirection * hitDistance, Color.black, 1);

            return null;
        }

        private IEnumerator FireEffectCO()
        {
            IsFiring = true;
            SetFiringAnim();

            AudioManager._instance.PlayerShotEvent(transform.position);

            yield return new WaitForSeconds((_settings.fireRate * _currentReloadModifier) / 60);

            IsFiring = false;
        }

        private void SetFiringAnim()
        {
            if (_anim != null)
                _anim.AttackAnim();
            else
                Debug.LogWarning("Lost weapon animator handler");
        }

        private void OnReload(float reloadModifier)
        {
            if (_isReloading)
                return;

            if (_magazineCurrentCapacity == _settings.magazineFullCapacity)
                return;

            _isReloading = true;
            AudioManager._instance.StartPlayerReloadEvent();
            Reload(reloadModifier);
        }

        private async void Reload(float reloadModifier)
        {
            try
            {
                await Task.Delay((int)(_settings.reloadTime * 1000 / reloadModifier), clipReloadCancellationTokenSource.Token);
                _magazineCurrentCapacity = _settings.magazineFullCapacity;
                _viewModel.AmmoCount = _magazineCurrentCapacity.ToString();
                _isReloading = false;
                AudioManager._instance.EndPlayerReloadEvent();
            }
            catch (OperationCanceledException e)
            {
                clipReloadCancellationTokenSource.Dispose();
                return;
            }
        }

        private void Dispose()
        {
            clipReloadCancellationTokenSource?.Cancel();
        }

        private bool CanShoot()
        {
            if (_isReloading)
                return false;

            if (Time.time - _lastTimeFired < 60 / (_settings.fireRate * _currentReloadModifier))
                return false;

            if (_magazineCurrentCapacity > 0)
            {
                _lastTimeFired = Time.time;
                _magazineCurrentCapacity--;

                if (_viewModel != null)
                    _viewModel.AmmoCount = _magazineCurrentCapacity.ToString();

                return true;
            }
            else
            {
                Debug.Log("Empty");
                AudioManager._instance.PlayerOutOfAmmoEvent();
                return false;
            }
        }

        private void Restart()
        {
            if (_hasDamageUpgrade)
            {
                StopCoroutine(_damageUpgradeCoroutine);
                _currentDamageModifier = _defaultDamageModifier;
                _hasDamageUpgrade = false;
                if (_damageUIIcon != null) Destroy(_damageUIIcon);
                else Debug.LogError("UIIcon lost");
            }

            if (_hasReloadUpgrade)
            {
                StopCoroutine(_reloadUpgradeCoroutine);
                _currentReloadModifier = _defaultReloadModifier;
                _hasReloadUpgrade = false;
                if (_reloadUIIcon != null) Destroy(_reloadUIIcon);
                else Debug.LogError("UIIcon lost");
            }
        }

        private IEnumerator DamageUpgradeCO(float time, float damageMultiplier)
        {
            _currentDamageModifier = _defaultDamageModifier * damageMultiplier;
            yield return new WaitForSeconds(time);
            _currentDamageModifier = _defaultDamageModifier;
            _hasDamageUpgrade = false;
        }

        private IEnumerator ReloadUpgradeCO(float time, float reloadMultiplier)
        {
            _currentReloadModifier = _defaultReloadModifier * reloadMultiplier;
            yield return new WaitForSeconds(time);
            _currentReloadModifier = _defaultReloadModifier;
            _hasReloadUpgrade = false;
        }

        public void OnDamageUpgrade(NetworkPlayer player, float damageMultiplier, float time, GameObject UIIcon)
        {
            if (_hasDamageUpgrade)
            {
                StopCoroutine(_damageUpgradeCoroutine);
                if (_damageUIIcon != null) Destroy(_damageUIIcon);
                else Debug.LogError("UIIcon lost");
            }
            else
            {
                _hasDamageUpgrade = true;
            }

            _damageUpgradeCoroutine = DamageUpgradeCO(time, damageMultiplier);
            StartCoroutine(_damageUpgradeCoroutine);
            _damageUIIcon = Instantiate(UIIcon, player.BonusesUIRoot.transform, false);

            if (_damageUIIcon.TryGetComponent<UIBonusIcon>(out var icon))
            {
                icon.SetTime(time);
            }
            else
            {
                Debug.LogError("Lost icon bonus script");
            }
        }

        public void OnReloadUpgrade(NetworkPlayer player, float reloadMultiplier, float time, GameObject UIIcon)
        {
            if (_hasReloadUpgrade)
            {
                StopCoroutine(_reloadUpgradeCoroutine);
                if (_reloadUIIcon != null) Destroy(_reloadUIIcon);
                else Debug.LogError("UIIcon lost");
            }
            else
            {
                _hasReloadUpgrade = true;
            }

            _reloadUpgradeCoroutine = ReloadUpgradeCO(time, reloadMultiplier);
            StartCoroutine(_reloadUpgradeCoroutine);
            _reloadUIIcon = Instantiate(UIIcon, player.BonusesUIRoot.transform, false);

            if (_reloadUIIcon.TryGetComponent<UIBonusIcon>(out var icon))
            {
                icon.SetTime(time);
            }
            else
            {
                Debug.LogError("Lost icon bonus script");
            }
        }

        public void SetShootDel(Action shootMethod)
        {
            shootDel += shootMethod;
        }

        public void UnSetShootDel(Action shootMethod)
        {
            shootDel -= shootMethod;
        }

        public void SetDefaultModifiers(float damageModifier, float reloadModifier)
        {
            _defaultDamageModifier = damageModifier;
            _defaultReloadModifier = reloadModifier;
        }

        public void SetSettings(Settings settings)
        {
            _settings = new Settings();
            _settings.magazineFullCapacity = settings.magazineFullCapacity;
            _settings.fireRate = settings.fireRate;
            _settings.reloadTime = settings.reloadTime;
            _settings.damageAmount = settings.damageAmount;

            _magazineCurrentCapacity = _settings.magazineFullCapacity;
            _viewModel.AmmoCount = _magazineCurrentCapacity.ToString();
        }

        [Serializable]
        public class Settings
        {
            public int magazineFullCapacity;
            public float fireRate;
            public float reloadTime;
            public int damageAmount;
        }
    }
}