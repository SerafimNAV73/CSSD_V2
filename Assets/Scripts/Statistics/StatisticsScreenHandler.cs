using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CSSD {
    public class StatisticsScreenHandler : MonoBehaviour
    {
        [SerializeField] private Text _scoreCount;
        [SerializeField] private Text _shotsCount;
        [SerializeField] private Text _killsCount;
        [SerializeField] private Text _bonusesCount;
        [SerializeField] private Text _levelCup;

        private StatisticsHandler _statisticsHandler;

        private void Start()
        {
            AudioManager._instance.EndPlayerReloadEvent();
            AudioManager._instance.PlayStatisticsTheme();
            _statisticsHandler = new StatisticsHandler();
            _statisticsHandler.GetStatistics();
            _scoreCount.text = _statisticsHandler.PlayerStats.scoreCount.ToString();
            _shotsCount.text = _statisticsHandler.PlayerStats.shotsCount.ToString();
            _killsCount.text = _statisticsHandler.PlayerStats.killCount.ToString();
            _bonusesCount.text = _statisticsHandler.PlayerStats.bonusesCount.ToString();
            _levelCup.text = _statisticsHandler.PlayerStats.levelCup.ToString();
        }

        public void OnBackToMainMenu()
        {
            AudioManager._instance.PlayMainMenuTheme();
            GameManager._instance.SetGameStatus(GameStatus.GameStart);
            SceneManager.LoadScene(0);
        }
    }
}