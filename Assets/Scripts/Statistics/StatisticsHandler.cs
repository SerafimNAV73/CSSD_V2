using System;
using UnityEngine;

namespace CSSD
{
    public class StatisticsHandler
    {
        private PlayerStatistics _playerStatistics;

        public PlayerStatistics PlayerStats => _playerStatistics;

        public StatisticsHandler()
        {
            _playerStatistics = new PlayerStatistics();
            _playerStatistics.scoreCount = 0;
            _playerStatistics.shotsCount = 0;
            _playerStatistics.killCount = 0;
            _playerStatistics.bonusesCount = 0;
            _playerStatistics.levelCup = 1;
        }

        public void UpScoreCount(int count)
        {
            _playerStatistics.scoreCount += count;
        }

        public void UpShotsCount()
        {
            _playerStatistics.shotsCount++;
        }

        public void UpKillCount()
        {
            _playerStatistics.killCount++;
        }

        public void UpBonusesCount()
        {
            _playerStatistics.bonusesCount++;
        }

        public void SetLevelCup(int cup)
        {
            _playerStatistics.levelCup = cup;
        }

        public void WriteStatistics()
        {
            var jsonString = JsonUtility.ToJson(_playerStatistics);
            GameManager._instance.SetPlayerStatisticsJsonString(jsonString);
        }

        public void GetStatistics()
        {
            var jsonString = GameManager._instance.GetPlayerStatisticsJsonString();
            _playerStatistics = JsonUtility.FromJson<PlayerStatistics>(jsonString);
        }

        public void Reset()
        {
            if (_playerStatistics != null)
            {
                _playerStatistics.scoreCount = 0;
                _playerStatistics.shotsCount = 0;
                _playerStatistics.killCount = 0;
                _playerStatistics.bonusesCount = 0;
                _playerStatistics.levelCup = 1;
            }
        }
    }

    [Serializable]
    public class PlayerStatistics
    {
        public int scoreCount;
        public int shotsCount;
        public int killCount;
        public int bonusesCount;
        public int levelCup;
    }
}