using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using System;
using FMODUnity;

namespace CSSD
{
    public class NetworkPlayer : NetworkBehaviour
    {
        [SerializeField] private LocalCameraHandler _localCameraHandler;
        [SerializeField] private NicknameHandler _nicknameHandler;
        [SerializeField] private GameObject _localUI;
        [SerializeField] private GameObject _bonusesUIRoot;
        [SerializeField] private StudioListener _studioListener;
        [SerializeField] private ViewModel _viewModel;

        //Level System
        [SerializeField] private int _currentLevel = 1;
        [SerializeField] private int _scoreToNextLevel = 20;
        [SerializeField] private List<MonoBehaviour> levelUpActions;
        
        //Other Components
        private WeaponHandler _weaponHandler;
        private HPHandler _hpHandler;
        private CharacterMovementHandler _characterMoveHandler;
        private CharacterAnimatorHandler _characterAnimatorHandler;
        private NetworkPlayerMessangerHandler _networkMessangerHandler;

        private StatisticsHandler _statisticsHandler;
        private ChangeDetector _changes;

        private int _currentScore = 0;

        public static Action<NetworkPlayer> playerDeathDel;
        public static Action<NetworkPlayer> playerReviveDel;

        [Networked, HideInInspector] public string _name { get; private set; }
        [Networked, HideInInspector] public NetworkString<_16> _nickname { get; set; }

        public NicknameHandler NicknameHandler => _nicknameHandler;
        public LocalCameraHandler LocalCamera => _localCameraHandler;
        public GameObject BonusesUIRoot => _bonusesUIRoot;
        public WeaponHandler Weapon => _weaponHandler;
        public HPHandler Health => _hpHandler;
        public CharacterMovementHandler Movement => _characterMoveHandler;
        public CharacterAnimatorHandler CharacterAnimatorHandler => _characterAnimatorHandler;
        public int CurrentLevel => _currentLevel;
        public int SelfID => gameObject.GetInstanceID();

        public static NetworkPlayer Local { get; set; }

        private void Awake()
        {
            _weaponHandler = GetComponent<WeaponHandler>();
            _hpHandler = GetComponent<HPHandler>();
            _characterMoveHandler = GetComponent<CharacterMovementHandler>();
            _characterAnimatorHandler = GetComponent<CharacterAnimatorHandler>();
            _networkMessangerHandler = GetComponent<NetworkPlayerMessangerHandler>();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            ScoreDealer.scoreDel += Score;
            Health.SetDeath(OnPlayerDeath);
            Health.SetRevive(OnPlayerRevive);
            Health.SetInitialize(OnPlayerInitialize);
            Weapon.SetShootDel(OnPlayerShoot);
            GameManager.writeStatisticsDel += WriteStatistics;
            GameManager.gameOverDel += GameOver;
            EnemyWaves.waveCountDel += SetWaveCount;
            PauseMenuHandler.restartDel += Restart;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            ScoreDealer.scoreDel -= Score;
            Health.UnSetDeath(OnPlayerDeath);
            Health.UnSetRevive(OnPlayerRevive);
            Health.UnSetInitialize(OnPlayerInitialize);
            Weapon.UnSetShootDel(OnPlayerShoot);
            GameManager.writeStatisticsDel -= WriteStatistics;
            GameManager.gameOverDel -= GameOver;
            EnemyWaves.waveCountDel -= SetWaveCount;
            PauseMenuHandler.restartDel -= Restart;
        }

        public override void Spawned()
        {
            _changes = GetChangeDetector(ChangeDetector.Source.SimulationState);
            _statisticsHandler = new StatisticsHandler();

            if (Object.HasInputAuthority)
            {
                Local = this;                              

                //Disable main camera
                if (Camera.main != null)
                    Camera.main.gameObject.SetActive(false);

                //Enable the local camera 
                _localCameraHandler.LocalCamera.enabled = true;
                _localCameraHandler.gameObject.SetActive(true);

                //Detach camera if enabled
                _localCameraHandler.transform.parent = null;

                //Enable UI for local player
                _localUI.SetActive(true);                

                _name = $"P_{Runner.LocalPlayer.PlayerId}";
                RPC_ChangeName(_name);

                _nickname = GameManager._instance.PlayerNickname;
                RPC_SetNickname(_nickname.ToString());

                //Disable the nick name for the local player.
                NicknameHandler.gameObject.SetActive(false);

                //Enable sound listener for local player
                _studioListener.enabled = true;

                var jsonString = GameManager._instance.GetPlayerStatsJsonString();
                if (jsonString.Length > 0)
                {
                    PlayerStats stats = JsonUtility.FromJson<PlayerStats>(jsonString);
                    Health.SetStartMaxHP(stats.MaxHP);
                    Weapon.SetDefaultModifiers(stats.damageModifier, stats.reloadModifier);
                    Movement.SetDefaultWalkSpeed(stats.walkSpeed);
                    RPC_ChangeStats(stats.MaxHP, stats.walkSpeed, stats.damageModifier, stats.reloadModifier);
                }

                jsonString = GameManager._instance.GetPlayerWeaponSettingsJsonString();
                if (jsonString.Length > 0)
                {
                    WeaponHandler.Settings settings = JsonUtility.FromJson<WeaponHandler.Settings>(jsonString);
                    Weapon.SetSettings(settings);
                    RPC_ChangeSettings(settings.magazineFullCapacity, settings.fireRate, settings.reloadTime, settings.damageAmount);
                }

                if (CharacterAnimatorHandler != null)
                    CharacterAnimatorHandler.OnSetRAC();

                Debug.Log("Spawned local player");
            }
            else
            {
                //Disable the camera if we are not the local player
                _localCameraHandler.LocalCamera.enabled = false;
                _localCameraHandler.gameObject.SetActive(false);

                //Disable soun listener for remote player
                _studioListener.enabled = true;

                //Disable UI for remote player
                _localUI.SetActive(false);

                //Disable UI for remote player
                _localUI.SetActive(false);

                Debug.Log("Spawned remote player");
            }
            //Set the player object 
            Runner.SetPlayerObject(Object.InputAuthority, Object);
            transform.name = _name;
            NicknameHandler.transform.parent = null;
            NicknameHandler.Nickname.text = _nickname.ToString();
            _networkMessangerHandler.SetNickname(_nickname.ToString());
        }

        public override void Render()
        {
            foreach (var change in _changes.DetectChanges(this, out var previousBuffer, out var currentBuffer))
            {
                switch (change)
                {
                    case nameof(_nickname):
                        var reader = GetPropertyReader<NetworkString<_16>>(nameof(_nickname));
                        var (previous, current) = reader.Read(previousBuffer, currentBuffer);
                        OnNickNameChanged(previous, current);
                        break;
                }
            }
        }

        private void LevelUp()
        {
            _currentLevel++;
            _statisticsHandler.SetLevelCup(_currentLevel);
            _scoreToNextLevel += _scoreToNextLevel * 2;
            foreach (var action in levelUpActions)
            {
                if (action == null) return;
                if (!(action is ILevelUp levelUp)) return;
                levelUp.LevelUp(this, _currentLevel);
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"{Time.time} OnSceneLoaded: {scene.name}");

            //Tell the host that we need to perform the spawned code manually.
            if (Object.HasStateAuthority && Object.HasInputAuthority)
                Spawned();
        }

        private void OnPlayerInitialize()
        {
            Debug.Log($"Player {transform.name} Initialize.");

            if (Object.HasStateAuthority)
            {
                if (GameManager._instance.CurStatus == GameStatus.WaveStart || GameManager._instance.CurStatus == GameStatus.GameOver)
                {
                    Health.OnTakeDamage(int.MaxValue);
                }
                else
                    _characterAnimatorHandler.RespawnAnim();
            }
        }

        private void OnPlayerDeath()
        {
            Debug.Log($"Player {transform.name} Death.");

            _characterMoveHandler.RequestRespawn();
            if (Object.HasStateAuthority)
            {
                Debug.Log($"Player {transform.name} Death delegate invokation");
                playerDeathDel?.Invoke(this);
            }
        }

        private void OnPlayerRevive()
        {
            Debug.Log($"Player {transform.name} Revive.");

            if (Object.HasStateAuthority)
            {
                Debug.Log($"Player {transform.name} Revive delegate invokation");
                playerReviveDel?.Invoke(this);
            }
        }

        private void WriteStatistics()
        {
            if (Object.HasInputAuthority)
                _statisticsHandler.WriteStatistics();
        }

        private void GameOver()
        {
            if (Runner != null)
                Runner.Shutdown();
        }

        private void OnPlayerShoot()
        {
            _statisticsHandler.UpShotsCount();
        }

        private void Restart()
        {
            _statisticsHandler.Reset();

            var jsonString = GameManager._instance.GetPlayerStatsJsonString();
            if (jsonString.Length > 0)
            {
                PlayerStats stats = JsonUtility.FromJson<PlayerStats>(jsonString);
                Health.SetStartMaxHP(stats.MaxHP);
                Weapon.SetDefaultModifiers(stats.damageModifier, stats.reloadModifier);
                Movement.SetDefaultWalkSpeed(stats.walkSpeed);
            }

            jsonString = GameManager._instance.GetPlayerWeaponSettingsJsonString();
            if (jsonString.Length > 0)
            {
                WeaponHandler.Settings settings = JsonUtility.FromJson<WeaponHandler.Settings>(jsonString);
                Weapon.SetSettings(settings);
                RPC_ChangeSettings(settings.magazineFullCapacity, settings.fireRate, settings.reloadTime, settings.damageAmount);
            }
        }

        private void OnNickNameChanged(NetworkString<_16> oldValue, NetworkString<_16> value)
        {
            Debug.Log($"Nickname changed for player to {value} for player {gameObject.name}");

            NicknameHandler.Nickname.text = value.ToString();
        }

        private void SetWaveCount(int count)
        {
            if (Object.HasStateAuthority && _viewModel != null)
                RPC_SetWaveCount(count);
        }

        public void UpKillCount()
        {
            if (Object.HasStateAuthority)
                RPC_UpKillCount();
        }

        public void UpBonusCount()
        {
            _statisticsHandler.UpBonusesCount();
        }

        public void Score(int scoreAmount)
        {
            if (GameManager._instance.CurStatus == GameStatus.GameOver)
                return;

            scoreAmount = (int)Mathf.Floor((float)scoreAmount / Mathf.Sqrt(Runner.ActivePlayers.Count()));
            _currentScore += scoreAmount;
            _statisticsHandler.UpScoreCount(scoreAmount);
            if (_currentScore >= _scoreToNextLevel) LevelUp();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_SetWaveCount(int count)
        {
            _viewModel.WaveCount = count.ToString();
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RPC_SetNickname(string nickName, RpcInfo info = default)
        {
            Debug.Log($"[RPC] SetNickName {nickName}");
            _nickname = nickName;
        }


        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_ChangeName(string name)
        {
            _name = name;
            transform.name = _name;
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_ChangeStats(int hp, float walkSpeed, float damageModifier, float reloadModifier, RpcInfo info = default)
        {
            Health.SetStartMaxHP(hp);
            Weapon.SetDefaultModifiers(damageModifier, reloadModifier);
            Movement.SetDefaultWalkSpeed(walkSpeed);
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_ChangeSettings(int magazineFullCapacity, float fireRate, float reloadTime, int damageAmount, RpcInfo info = default)
        {
            WeaponHandler.Settings settings = new WeaponHandler.Settings();
            settings.magazineFullCapacity= magazineFullCapacity;
            settings.fireRate = fireRate;
            settings.reloadTime = reloadTime;
            settings.damageAmount= damageAmount;
            Weapon.SetSettings(settings);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_UpKillCount()
        {
            _statisticsHandler.UpKillCount();
        }
    }

    [Serializable]
    public class PlayerStats
    {
        public int MaxHP;
        public float walkSpeed;
        public float damageModifier;
        public float reloadModifier;
    }
}
