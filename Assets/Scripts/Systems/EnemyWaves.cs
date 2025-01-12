using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Fusion;

namespace CSSD
{
    public class EnemyWaves : NetworkBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private EnemyBehaviour _meleeEnemyPrefab;
        [SerializeField] private EnemyBehaviour _rangedEnemyPrefab;
        [SerializeField] private EnemyBehaviour _bigEnemyPrefab;

        [Header("Delays")]
        [SerializeField] private float _waveSpawnDelay = 10;

        [SerializeField] private int _rangedEnemyWaveDelay = 1;
        [SerializeField] private int _bigEnemyWaveDelay = 2;

        [Header("Start enemy counts")]
        [SerializeField] private int _startMeleeEnemyCount = 5;
        [SerializeField] private int _startRangedEnemyCount = 3;
        [SerializeField] private int _startBigEnemyCount = 1;

        [Header("Enemy spawn delays")]
        [SerializeField] private int _meleeEnemySpawnDelay = 1;
        [SerializeField] private int _rangedEnemySpawnDelay = 2;
        [SerializeField] private int _bigEnemySpawnDelay = 5;

        [Header("Enemy spawn point tag")]
        [SerializeField] private string _enemySpawnPointTag = "EnemySpawn";
        //private List<Transform> _spawnPoints = new List<Transform>();

        private GameObject[] _enemySpawnPoints;
        private List<EnemyBehaviour> _spawnedEnemies = new List<EnemyBehaviour>();

        private int _waveCount = 0;

        private int _meleeEnemyCount = 0;
        private int _rangedEnemyCount = 0;
        private int _bigEnemyCount = 0;

        private int _enemyTypesWavingCount = 0;

        private bool _isWaving = false;
        private bool _isNewWaveStarted = false;

        private int _playersCount = 0;

        public bool IsWaving => _isWaving;
        public bool IsNewWaveStarted => _isNewWaveStarted;
        public int PlayersCount
        {
            get { return _playersCount; }
            set { _playersCount = value; }
        }

        private CancellationTokenSource _cancellationTokenSource;

        public static Action<int> waveCountDel;

        private void OnValidate()
        {
            if (_waveSpawnDelay < 0) _waveSpawnDelay = 0;
            if (_rangedEnemyWaveDelay < 0) _rangedEnemyWaveDelay = 0;
            if (_bigEnemyWaveDelay < 0) _bigEnemyWaveDelay = 0;
            if (_startMeleeEnemyCount < 0) _startMeleeEnemyCount = 0;
            if (_startRangedEnemyCount < 0) _startRangedEnemyCount = 0;
            if (_startBigEnemyCount < 0) _startBigEnemyCount = 0;
        }

        private void Start()
        {
            _enemySpawnPoints = GameObject.FindGameObjectsWithTag(_enemySpawnPointTag);
            AudioManager._instance.PlayWaveEndTheme();

#if UNITY_EDITOR
            if (_enemySpawnPoints.Length == 0)
                Debug.LogError("Lost spawn points, check the tags");
#endif
        }

        private void OnEnable()
        {
            RetreatBehaviour.getRetreatPointDel += SetRetreatPoint;
            GameManager.waveStartDel += NewWaveStart;
            EnemyBehaviour.removeEnemyDel += RemoveEnemy;

            PauseMenuHandler.restartDel += Restart;
        }

        private void OnDisable()
        {
            RetreatBehaviour.getRetreatPointDel -= SetRetreatPoint;
            GameManager.waveStartDel -= NewWaveStart;
            EnemyBehaviour.removeEnemyDel -= RemoveEnemy;

            PauseMenuHandler.restartDel -= Restart;
        }

        private void OnDestroy()
        {
            TokenCancellation();
        }

        public override void FixedUpdateNetwork()
        {
            if (!Object.HasStateAuthority)
                return;

            CheckWaving();
        }

        private void CheckWaving()
        {
            if (_isWaving)
            {
                if (_enemyTypesWavingCount == 0 && _spawnedEnemies.Count == 0)
                {
                    _isWaving = false;
                    _isNewWaveStarted = false;
                    GameManager._instance.SetGameStatus(GameStatus.WaveEnd);
                    AudioManager._instance.PlayWaveEndTheme();
                    RPC_EndWave();
                }
            }
        }

        private void TokenCancellation()
        {
            _cancellationTokenSource?.Cancel();
        }

        private void NewWaveStart()
        {
            if (!Object.HasStateAuthority)
                return;

            if (!_isWaving)
            {
                _waveCount++;
                waveCountDel?.Invoke(_waveCount);

                int rangedMultiplier = _waveCount - _rangedEnemyWaveDelay;
                int bigMultiplier = _waveCount - _bigEnemyWaveDelay;
                if (rangedMultiplier < 0) rangedMultiplier = 0;
                if (bigMultiplier < 0) bigMultiplier = 0;

                _meleeEnemyCount = Mathf.CeilToInt((float)_startMeleeEnemyCount * (float)_waveCount * (float)Math.Sqrt((float)_playersCount));
                _rangedEnemyCount = Mathf.CeilToInt((float)_startRangedEnemyCount * (float)rangedMultiplier * (float)Math.Sqrt((float)_playersCount));
                _bigEnemyCount = Mathf.CeilToInt((float)_startBigEnemyCount * (float)bigMultiplier * (float)Math.Sqrt((float)_playersCount));

                _isWaving = true;
                GameManager._instance.SetGameStatus(GameStatus.WaveStart);
                AudioManager._instance.PlayWaveStartTheme();
                RPC_StartWave();

                _cancellationTokenSource = _cancellationTokenSource ?? new CancellationTokenSource();

                _enemyTypesWavingCount = 3;
                SpawnAsync(_meleeEnemyPrefab, _meleeEnemyCount, _meleeEnemySpawnDelay, _cancellationTokenSource);
                SpawnAsync(_rangedEnemyPrefab, _rangedEnemyCount, _rangedEnemySpawnDelay, _cancellationTokenSource);
                SpawnAsync(_bigEnemyPrefab, _bigEnemyCount, _bigEnemySpawnDelay, _cancellationTokenSource);
            }
        }

        private async void SpawnAsync(EnemyBehaviour prefab, int count, int spawnDelay, CancellationTokenSource token)
        {
            if (!Object.HasStateAuthority)
                return;

            try
            {
                while (count > 0)
                {
                    await Task.Delay(spawnDelay * 1000, token.Token);
                    await CustomInstantiateAsync(prefab);
                    count--;
                }
                _enemyTypesWavingCount--;
            }
            catch (OperationCanceledException e)
            {
                Debug.Log(e);
                token?.Dispose();
                return;
            }
        }

        private async Task CustomInstantiateAsync(EnemyBehaviour prefab)
        {
            Vector3 curSpawn = _enemySpawnPoints[UnityEngine.Random.Range(0, _enemySpawnPoints.Length)].transform.position;
            EnemyBehaviour newEnemy = Runner.Spawn(prefab, curSpawn, Quaternion.identity);
            _spawnedEnemies.Add(newEnemy);
            newEnemy.OnSetWaveCount(_waveCount);
            await Task.Yield();
        }

        private Transform SetRetreatPoint()
        {
            return _enemySpawnPoints[UnityEngine.Random.Range(0, _enemySpawnPoints.Length)].transform;
        }

        private IEnumerator NewWaveTimer()
        {
            yield return new WaitForSeconds(_waveSpawnDelay);
            NewWaveStart();
        }

        private void RemoveEnemy(EnemyBehaviour enemy)
        {
            _spawnedEnemies.Remove(enemy);
        }

        private void Restart()
        {
            GameManager._instance.SetGameStatus(GameStatus.GameStart);
            TokenCancellation();

            _isWaving = false;
            _isNewWaveStarted = false;
            _waveCount = 0;

            _meleeEnemyCount = 0;
            _rangedEnemyCount = 0;
            _bigEnemyCount = 0;

            _enemyTypesWavingCount = 0;

            AudioManager._instance.PlayWaveEndTheme();

            foreach (EnemyBehaviour enemy in _spawnedEnemies)
                Runner.Despawn(enemy.GetComponent<NetworkObject>());


            StartNewWaveTimer();
        }


        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_StartWave()
        {
            GameManager._instance.SetGameStatus(GameStatus.WaveStart);
            AudioManager._instance.PlayWaveStartTheme();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_EndWave()
        {
            GameManager._instance.SetGameStatus(GameStatus.WaveEnd);
            AudioManager._instance.PlayWaveEndTheme();
        }

        public void StartNewWaveTimer()
        {
            if (!_isNewWaveStarted)
            {
                StartCoroutine(NewWaveTimer());
                _isNewWaveStarted = true;
            }
        }
    }
}