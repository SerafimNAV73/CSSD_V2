using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CSSD 
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private GameObject _mainMenuPanel;
        [SerializeField] private GameObject _settingsPanel;
        [SerializeField] private GameObject _characterChoosePanel;

        [SerializeField] private SliderHandler _masterSlider;
        [SerializeField] private SliderHandler _sfxSlider;
        [SerializeField] private SliderHandler _ambientSlider;

        [SerializeField] private Button _startButton;
        [SerializeField] private InputField _nicknameInputField;

        [SerializeField] private List<PlayerStatsHandler> _playerCharButtonsList = new List<PlayerStatsHandler>();

        [SerializeField] private Color _defaultButtonColor;
        [SerializeField] private Color _choosenButtonColor;

        private PlayerStats _stats;
        private WeaponHandler.Settings _settings;
        private SoundSettings _soundSettings;

        private bool _hasChoosenCharacter;
        private bool _hasNickname;

        private void Start()
        {
            AudioManager._instance.PlayMainMenuTheme();
            LoadSoundSettings();

            _mainMenuPanel?.SetActive(true);
            _settingsPanel?.SetActive(false);
            _characterChoosePanel?.SetActive(false);

            if (_startButton!= null)
                _startButton.interactable = false;            
        }


        private void LoadSoundSettings()
        {
            _soundSettings = AudioManager._instance.LoadSoundSettings();
            if (_soundSettings == null)
            {
                _masterSlider.SetDefaultValue();
                _sfxSlider.SetDefaultValue();
                _ambientSlider.SetDefaultValue();
            }
            else
            {
                _masterSlider.CurSlider.value = _soundSettings.masterVolume;
                _sfxSlider.CurSlider.value = _soundSettings.sfxVolume;
                _ambientSlider.CurSlider.value = _soundSettings.ambienceVolume;
                _masterSlider.UpdateSliderOutput();
                _sfxSlider.UpdateSliderOutput();
                _ambientSlider.UpdateSliderOutput();
            }
        }

        private void CheckStart()
        {
            if (_startButton == null) return;

            if (_hasChoosenCharacter && _hasNickname) 
                _startButton.interactable = true;
            else 
                _startButton.interactable = false;
        }

        public void OnSetBoolCharacterChoose(bool hasChoosenCharacter)
        {
            _hasChoosenCharacter = hasChoosenCharacter;
            CheckStart();
        }

        public void OnSetNickname()
        {
            if (_nicknameInputField.text == "")            
                _hasNickname = false;
            else
                _hasNickname = true;

            GameManager._instance.PlayerNickname = _nicknameInputField.text;
            CheckStart();
        }

        public void OnGameScene()
        {
            AudioManager._instance.PlayClickEvent();
            GameManager._instance.SetPlayerStatsJsonString(JsonUtility.ToJson(_stats), JsonUtility.ToJson(_settings));

            SceneManager.LoadScene(1);
        }

        public void SetPlayerStats(PlayerStats stats, WeaponHandler.Settings settings)
        {
            _stats = stats;
            _settings = settings;
        }

        public void SetPlayerControllerID(int id)
        {
            GameManager._instance.SetPlayerRACid(id);
        }

        public void OnExit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        }

        public void OnSaveSoundSettings()
        {
            SoundSettings settings = new SoundSettings();
            settings.masterVolume = _masterSlider.CurSlider.value;
            settings.sfxVolume = _sfxSlider.CurSlider.value;
            settings.ambienceVolume = _ambientSlider.CurSlider.value;
            AudioManager._instance.SaveSoundSettings(settings);
        }

        public void SetDefaultColorsToButton()
        {
            foreach(PlayerStatsHandler button in _playerCharButtonsList)
            {
                button.SetColor(_defaultButtonColor);
            }
        }

        public void SetChoosenCharacterColor(PlayerStatsHandler handler)
        {
            handler.SetColor(_choosenButtonColor);
        }
    }
}