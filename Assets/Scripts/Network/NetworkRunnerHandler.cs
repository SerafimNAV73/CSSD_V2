using Fusion;
using Fusion.Sockets;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CSSD
{
    public class NetworkRunnerHandler : MonoBehaviour
    {
        [SerializeField] private NetworkRunner _networkRunnerPrefab;

        private NetworkRunner _networkRunner;

        private void Awake()
        {
            NetworkRunner networkRunnerInScene = FindAnyObjectByType<NetworkRunner>();

            //If we already have a network runner in the scene then we should not create another one but rather use the existing one
            if (networkRunnerInScene != null)
                _networkRunner = networkRunnerInScene;
        }

        private void Start()
        {
            if (_networkRunner == null)
            {
                _networkRunner = Instantiate(_networkRunnerPrefab);
                _networkRunner.name = "Network runner";
            }
        }

        private INetworkSceneManager GetSceneManager(NetworkRunner runner)
        {
            var sceneManager = runner.GetComponents(typeof(MonoBehaviour)).OfType<INetworkSceneManager>().FirstOrDefault();

            if (sceneManager == null)
            {
                //Handle networked objects that already exists in the scene
                sceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
            }

            return sceneManager;
        }

        protected virtual Task InitializeNetworkRunner(NetworkRunner runner, GameMode gameMode, string sessionName, NetAddress address, SceneRef scene, Action<NetworkRunner> onGameStarted)
        {
            var sceneManager = GetSceneManager(runner);

            runner.ProvideInput = true;

            return runner.StartGame(new StartGameArgs
            {
                GameMode = gameMode,
                Address = address,
                Scene = scene,
                SessionName = sessionName,
                CustomLobbyName = "Game",
                OnGameStarted = onGameStarted,
                SceneManager = sceneManager,
                PlayerCount = 4,
            });
        }

        public void OnSingleGame()
        {
            var clientTask = InitializeNetworkRunner(_networkRunner, GameMode.Single, "Game",
                NetAddress.Any(), SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex), null);
            AudioManager._instance.PlayClickEvent();
        }

        public void OnCreateGame()
        {
            //Join existing game as a host
            var clientTask = InitializeNetworkRunner(_networkRunner, GameMode.Host, "Game", 
                NetAddress.Any(), SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex), null);
            AudioManager._instance.PlayClickEvent();
        }

        public void OnJoinGame()
        {
            //Join existing game as a client
            var clientTask = InitializeNetworkRunner(_networkRunner, GameMode.Client, "Game",
                NetAddress.Any(), SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex), null);
            AudioManager._instance.PlayClickEvent();
        }
    }
}