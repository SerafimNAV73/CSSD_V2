using UnityEngine;
using FMODUnity;

namespace CSSD
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager _instance = null;

        //UI Events
        [Header("UI Events")]
        [SerializeField]
        private EventReference _clickEvent;

        [SerializeField]
        private EventReference _sliderEvent;

        private FMOD.Studio.EventInstance _sliderEventInstance;

        //Themes
        [Header("Themes")]
        [SerializeField]
        private EventReference _mainMenuEvent;

        [SerializeField]
        private EventReference _waveStartEvent;

        [SerializeField]
        private EventReference _waveEndEvent;

        [SerializeField]
        private EventReference _statisticsEvent;

        private FMOD.Studio.EventInstance _mainMenuEventInstance;
        private FMOD.Studio.EventInstance _waveEndEventInstance;
        private FMOD.Studio.EventInstance _waveStartEventInstance;
        private FMOD.Studio.EventInstance _statisticsEventInstance;
        private FMOD.Studio.EventInstance _currentThemeEventInstance;

        //Player events
        [Header("Player events")]
        [SerializeField]
        private EventReference _playerJoinEvent;

        [SerializeField]
        private EventReference _playerLeftEvent;

        [SerializeField]
        private EventReference _playerShotEvent;

        [SerializeField]
        private EventReference _playerReloadEvent;

        [SerializeField]
        private EventReference _playerOutOfAmmoEvent;

        [SerializeField]
        private EventReference _playerRespawnEvent;

        private FMOD.Studio.EventInstance _playerReloadEventInstance;

        //PickUp events
        [Header("PickUp events")]
        [SerializeField]
        private EventReference _damagePickUpEvent;

        [SerializeField]
        private EventReference _walkSpeedPickUpEvent;

        [SerializeField]
        private EventReference _reloadSpeedPickUpEvent;

        [SerializeField]
        private EventReference _armorPickUpEvent;

        [SerializeField]
        private EventReference _healPickUpEvent;

        //Enemy events
        [Header("Enemy events")]
        [SerializeField]
        private EventReference _enemySpawnEvent;

        [SerializeField]
        private EventReference _enemyMeleeAttackEvent;

        [SerializeField]
        private EventReference _enemyRangeAttackEvent;

        private void Awake()
        {
            if (_instance == null)
                _instance = this;
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            _mainMenuEventInstance = RuntimeManager.CreateInstance(_mainMenuEvent);
            _waveStartEventInstance = RuntimeManager.CreateInstance(_waveStartEvent);
            _waveEndEventInstance = RuntimeManager.CreateInstance(_waveEndEvent);
            _statisticsEventInstance = RuntimeManager.CreateInstance(_statisticsEvent);
        }
                    
        #region UI event methods
        public void PlayClickEvent()
        {
            if (_clickEvent.IsNull) return;

            RuntimeManager.PlayOneShot(_clickEvent);
        }

        public void PlaySliderEvent()
        {
            if (_sliderEvent.IsNull) return;

            if (_sliderEventInstance.handle == null)
            {
                _sliderEventInstance = RuntimeManager.CreateInstance(_sliderEvent);
                _sliderEventInstance.start();
            }
            else
            {
                _sliderEventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                _sliderEventInstance.release();
                _sliderEventInstance = RuntimeManager.CreateInstance(_sliderEvent);
                _sliderEventInstance.start();
            }
        }
        #endregion
            
        #region Player event methods
        public void PlayerJoinEvent()
        {
            if (_playerJoinEvent.IsNull) return;

            RuntimeManager.PlayOneShot(_playerJoinEvent);
        }

        public void PlayerLeftEvent()
        {
            if (_playerLeftEvent.IsNull) return;

            RuntimeManager.PlayOneShot(_playerLeftEvent);
        }

        public void PlayerShotEvent(Vector3 pos)
        {
            if (_playerShotEvent.IsNull) return;

            RuntimeManager.PlayOneShot(_playerShotEvent, pos);
        }

        public void StartPlayerReloadEvent()
        {
            if (_playerReloadEvent.IsNull) return;

            _playerReloadEventInstance = RuntimeManager.CreateInstance(_playerReloadEvent);
            _playerReloadEventInstance.start();            
        }

        public void EndPlayerReloadEvent()
        {
            if (_playerReloadEvent.IsNull) return;

            _playerReloadEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            _playerReloadEventInstance.release();            
        }

        public void PlayerOutOfAmmoEvent()
        {
            if (_playerOutOfAmmoEvent.IsNull) return;

            RuntimeManager.PlayOneShot(_playerOutOfAmmoEvent);
        }

        public void PlayerRespawnEvent(Vector3 pos)
        {
            if (_playerRespawnEvent.IsNull) return;

            RuntimeManager.PlayOneShot(_playerRespawnEvent, pos);
        }
        #endregion
            
        #region PickUp event methods
        public void DamagePickUpEvent()
        {
            if (_damagePickUpEvent.IsNull) return;

            RuntimeManager.PlayOneShot(_damagePickUpEvent);
        }

        public void WalkSpeedPickUpEvent()
        {
            if (_walkSpeedPickUpEvent.IsNull) return;

            RuntimeManager.PlayOneShot(_walkSpeedPickUpEvent);
        }

        public void ReloadPickUpEvent()
        {
            if (_reloadSpeedPickUpEvent.IsNull) return;

            RuntimeManager.PlayOneShot(_reloadSpeedPickUpEvent);
        }

        public void ArmorPickUpEvent()
        {
            if (_armorPickUpEvent.IsNull) return;

            RuntimeManager.PlayOneShot(_armorPickUpEvent);
        }

        public void HealPickUpEvent()
        {
            if (_healPickUpEvent.IsNull) return;

            RuntimeManager.PlayOneShot(_healPickUpEvent);
        }
        #endregion
            
        #region Enemy event methods
        public void EnemySpawnEvent(Vector3 pos)
        {
            if (_enemySpawnEvent.IsNull) return;

            RuntimeManager.PlayOneShot(_enemySpawnEvent, pos);
        }
        public void EnemyMeleeAttackEvent(Vector3 pos)
        {
            if (_enemyMeleeAttackEvent.IsNull) return;

            RuntimeManager.PlayOneShot(_enemyMeleeAttackEvent, pos);
        }
        public void EnemyRangeAttackEvent(Vector3 pos)
        {
            if (_enemyRangeAttackEvent.IsNull) return;

            RuntimeManager.PlayOneShot(_enemyRangeAttackEvent, pos);
        }
        #endregion

        #region Theme event methods
        public void PlayMainMenuTheme()
        {
            if (_currentThemeEventInstance.handle != null)
            {
                _currentThemeEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                _currentThemeEventInstance.release();
            }

            _currentThemeEventInstance = _mainMenuEventInstance;

            _currentThemeEventInstance.start();
        }

        public void PlayWaveStartTheme()
        {
            if (_currentThemeEventInstance.handle != null)
            {
                _currentThemeEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                _currentThemeEventInstance.release();
            }

            _currentThemeEventInstance = _waveStartEventInstance;

            _currentThemeEventInstance.start();
        }

        public void PlayWaveEndTheme()
        {
            if (_currentThemeEventInstance.handle != null)
            {
                _currentThemeEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                _currentThemeEventInstance.release();
            }

            _currentThemeEventInstance = _waveEndEventInstance;

            _currentThemeEventInstance.start();
        }
        public void PlayStatisticsTheme()
        {
            if (_currentThemeEventInstance.handle != null)
            {
                _currentThemeEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                _currentThemeEventInstance.release();
            }

            _currentThemeEventInstance = _statisticsEventInstance;

            _currentThemeEventInstance.start();
        }
        #endregion

        public void PlayEvent(EventReference eventRef, Vector3 position)
        {
            if (eventRef.IsNull) return;

            RuntimeManager.PlayOneShot(eventRef, position);
        }

        private bool IsPlaying(FMOD.Studio.EventInstance instance)
        {
            FMOD.Studio.PLAYBACK_STATE state;
            instance.getPlaybackState(out state);
            return state != FMOD.Studio.PLAYBACK_STATE.STOPPED;
        }

        public void SaveSoundSettings(SoundSettings soundSettings)
        {
            PlayerPrefs.SetString("SoundSettings",JsonUtility.ToJson(soundSettings));
            PlayerPrefs.Save();
        }

        public SoundSettings LoadSoundSettings()
        {
            var jsonString = PlayerPrefs.GetString("SoundSettings");
            return JsonUtility.FromJson<SoundSettings>(jsonString);
        }
    }

    public class SoundSettings
    {
        public float masterVolume;
        public float sfxVolume;
        public float ambienceVolume;
    }
}