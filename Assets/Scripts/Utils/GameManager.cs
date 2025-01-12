using System;
using UnityEngine;

namespace CSSD 
{
    public class GameManager : MonoBehaviour
    {
        //Static instance of GameManager so other scripts can access it
        public static GameManager _instance = null;

        private string _playerNickname = "";

        private GameStatus _curStatus = GameStatus.GameStart;

        private int _playerRACid;
        private string _playerStatsJsonString;
        private string _playerWeaponSettingsJsonString;
        private string _playerStatisticsJsonString;

        private bool _isPausedGame = false;

        public static event Action waveStartDel;
        public static event Action waveEndDel;
        public static event Action gameOverDel;

        public static event Action writeStatisticsDel;

        public GameStatus CurStatus => _curStatus;

        public string PlayerNickname 
        { 
            get { return _playerNickname; } 
            set { _playerNickname = value; } 
        }

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

        private void Update()
        {
            switch (_curStatus)
            {
                case GameStatus.GameStart:
                    {                    
                        Debug.Log("Game started");
                        break;
                    }
                case GameStatus.WaveStart:
                    {
                        waveStartDel?.Invoke();
                        Debug.Log("Wave started");
                        break;
                    }
                case GameStatus.WaveEnd:
                    {
                        //Respawn players
                        waveEndDel?.Invoke();
                        _curStatus = GameStatus.WaveStart;
                        Debug.Log("Wave ended");
                        break;
                    }
                case GameStatus.GameOver:
                    {
                        writeStatisticsDel?.Invoke();
                        gameOverDel?.Invoke();
                        Debug.Log("Game over");
                        break;
                    }
            }
        }

        public void SetGameStatus(GameStatus status)
        {
            _curStatus = status;
        }

        public void SetPlayerRACid(int id)
        {
            RuntimeAnimatorController[] animators = Resources.LoadAll<RuntimeAnimatorController>("RuntimeAnimatorControllers/");

            if (id > animators.Length - 1 || id < 0)
                Debug.LogError("Wrong id");

            _playerRACid = id;
        }

        public int GetPlayerRACid()
        {
            return _playerRACid;
        }

        public void SetPlayerStatsJsonString(string statsJsonString, string settingsJsonString)
        {
            _playerStatsJsonString = statsJsonString;
            _playerWeaponSettingsJsonString = settingsJsonString;
        }

        public string GetPlayerStatsJsonString()
        {
            return _playerStatsJsonString;
        }

        public string GetPlayerWeaponSettingsJsonString()
        {
            return _playerWeaponSettingsJsonString;
        }

        public void SetPlayerStatisticsJsonString(string jsonString)
        {
            _playerStatisticsJsonString = jsonString;
        }

        public string GetPlayerStatisticsJsonString()
        {
            return _playerStatisticsJsonString;
        }
    }

    public enum GameStatus { GameStart, WaveStart, WaveEnd, GameOver }
}
