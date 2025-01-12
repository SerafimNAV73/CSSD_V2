using UnityEngine;
using Fusion;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;
using System.Collections;

namespace CSSD
{
    public class PauseMenuHandler : NetworkBehaviour
    {
        [SerializeField] private GameObject _pausePanel;
        [SerializeField] private Button _restartButton;

        [SerializeField] private SliderHandler _masterSlider;
        [SerializeField] private SliderHandler _sfxSlider;
        [SerializeField] private SliderHandler _ambientSlider;

        private SoundSettings _soundSettings;

        private bool _isInputOnCooldown;

        public static event Action restartDel;

        public override void Spawned()
        {
            LoadSoundSettings();
            Time.timeScale = 1;

            _pausePanel.SetActive(false);
            if (Runner.GameMode != GameMode.Single)
                _restartButton.interactable = false;
        }

        public override void FixedUpdateNetwork()
        {
            if (_pausePanel == null) return;

            if (_isInputOnCooldown) return;

            if (GetInput(out NetworkInputData networkInputData))
            {
                if (networkInputData.isMenuButtonPressed)
                {
                    if (!_pausePanel.activeInHierarchy)
                    {
                        _pausePanel.SetActive(true);
                        if (Runner.GameMode == GameMode.Single)
                            Time.timeScale = 0;
                    }
                    else
                    {
                        OnBackToGame();
                    }                    
                    StartCoroutine(CooldownCO());
                }
            }
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

        private void SaveSoundSettings()
        {
            SoundSettings settings = new SoundSettings();
            settings.masterVolume = _masterSlider.CurSlider.value;
            settings.sfxVolume = _sfxSlider.CurSlider.value;
            settings.ambienceVolume = _ambientSlider.CurSlider.value;
            AudioManager._instance.SaveSoundSettings(settings);
        }

        private IEnumerator CooldownCO()
        {
            _isInputOnCooldown = true;
            yield return new WaitForSeconds(2);
            _isInputOnCooldown = false;
        }

        public void OnBackToMainMenu()
        {
            SaveSoundSettings();
            if (Object.HasStateAuthority)
                RPC_BackToMenu();
            else
            {
                GameManager._instance.SetGameStatus(GameStatus.GameStart);
                Runner.Shutdown();
            }
        }

        public void OnRestart()
        {
            SaveSoundSettings();
            restartDel?.Invoke();
        }

        public void OnBackToGame()
        {
            SaveSoundSettings();
            _pausePanel.SetActive(false);
            if (Runner.GameMode == GameMode.Single)
                Time.timeScale = 1;
        }

        [Rpc(RpcSources.StateAuthority,RpcTargets.All)]
        private void RPC_BackToMenu()
        {
            GameManager._instance.SetGameStatus(GameStatus.GameStart);
            Runner.Shutdown();
        }
    }
}
