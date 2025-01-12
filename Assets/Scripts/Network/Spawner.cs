using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using UnityEngine.SceneManagement;

namespace CSSD
{
    public class Spawner : SimulationBehaviour, INetworkRunnerCallbacks
    {
        [Header("Prefabs")]
        [SerializeField] private NetworkObject _playerPrefab;
        [SerializeField] private EnemyWaves _enemyWaverPrefab;
        [SerializeField] private PlayerLifeMonitor _playerLifeMonitorPrefab;
        [SerializeField] private BonusSpawner _bonusSpawnerPrefab;

        [Header("Player Spawn Tag")]
        [SerializeField] private string _playerSpawnTag = "PlayerSpawn";

        [Header("First wave timer")]
        [SerializeField] private float _time = 60;

        private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();
        private List<NetworkPlayer> _playerList = new List<NetworkPlayer>();

        private EnemyWaves _enemyWaver;
        private PlayerLifeMonitor _playerLifeMonitor;
        private BonusSpawner _bonusSpawner;

        private GameObject[] _playerSpawnPoints;

        //Other components
        private CharacterInputHandler _characterInputHandler;

        private void Start()
        {
            _playerSpawnPoints = GameObject.FindGameObjectsWithTag(_playerSpawnTag);

#if UNITY_EDITOR
            if (_playerSpawnPoints.Length == 0)
                Debug.LogError("Lost spawn points, check the tags");
#endif
        }

        private IEnumerator WaveStartCO(NetworkRunner runner)
        {
            yield return new WaitForSeconds(_time);
            
            _enemyWaver?.StartNewWaveTimer();
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            if (_characterInputHandler == null && NetworkPlayer.Local != null)
                _characterInputHandler = NetworkPlayer.Local.GetComponent<CharacterInputHandler>();

            if (_characterInputHandler != null)
                input.Set(_characterInputHandler.GetNetworkInput());
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (runner.IsServer)
            {
                Vector3 spawnPosition = new Vector3();

                if (_playerSpawnPoints.Length > 0)
                    spawnPosition = _playerSpawnPoints[player.RawEncoded % runner.Config.Simulation.PlayerCount].transform.position;
                else
                    spawnPosition = Vector3.zero;

                NetworkObject spawnedNetworkPlayerObject = runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity, player);
                spawnedNetworkPlayerObject.transform.position = spawnPosition;
                _spawnedCharacters.Add(player, spawnedNetworkPlayerObject);

                NetworkPlayer spawnedNetworkPlayer = spawnedNetworkPlayerObject.GetComponent<NetworkPlayer>();
                _playerList.Add(spawnedNetworkPlayer);
                _playerLifeMonitor.AddPlayer(spawnedNetworkPlayer);

                _enemyWaver.PlayersCount = _playerList.Count;
            }

            AudioManager._instance.PlayerJoinEvent();
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (GameManager._instance.CurStatus == GameStatus.GameOver)
                return;

            if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
            {
                runner.Despawn(networkObject);
                _spawnedCharacters.Remove(player);

                NetworkPlayer networkPlayer = networkObject.GetComponent<NetworkPlayer>();
                _playerList.Remove(networkPlayer);
                _playerLifeMonitor.RemovePlayer(networkPlayer);

                _enemyWaver.PlayersCount = _playerList.Count;
            }

            AudioManager._instance.PlayerLeftEvent();
        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {
            if (runner.IsServer)
            {
                _enemyWaver = runner.Spawn(_enemyWaverPrefab, Vector3.zero, Quaternion.identity);
                _playerLifeMonitor = runner.Spawn(_playerLifeMonitorPrefab, Vector3.zero, Quaternion.identity);                
                _bonusSpawner = runner.Spawn(_bonusSpawnerPrefab, Vector3.zero, Quaternion.identity);

                foreach (var player in _playerList)
                {
                    _playerLifeMonitor.AddPlayer(player);
                }

                StartCoroutine(WaveStartCO(runner));
            }
        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

        public void OnHostMigrationCleanUp() { }

        public void OnConnectedToServer(NetworkRunner runner) { }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }

        public void OnSceneLoadStart(NetworkRunner runner) { }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            if (GameManager._instance.CurStatus == GameStatus.GameOver)
                SceneManager.LoadScene("Statistics");
            else
                SceneManager.LoadScene("MainMenu");
        }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    }
}