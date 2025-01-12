using System;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.AI;

namespace CSSD
{
    public class EnemyBehaviour : NetworkBehaviour
    {
        [SerializeField] private NavMeshAgent _agent;
        [SerializeField] private List<NetworkBehaviour> behaviours = new List<NetworkBehaviour>();

        [SerializeField] private float _targetTime = 5;

        private IBehaviour activeBehaviour;

        private float _waveCount;

        public static Func<NetworkPlayer> getRandomPlayerDel;
        public static Func<Transform, NetworkPlayer> getClosestPlayerDel;
        public static Action<EnemyBehaviour> removeEnemyDel;

        //Target
        private NetworkPlayer _target;

        [Networked] private TickTimer _targetTimer { get; set; }

        private void OnValidate()
        {
            if (_targetTime < 0) _targetTime = 0;
        }

        public override void Spawned()
        {
            if (Object.HasStateAuthority)
            {
                SetTarget();
                SetAgent();
            }

            AudioManager._instance.EnemySpawnEvent(transform.position);
        }

        public override void FixedUpdateNetwork()
        {
            if (!Object.HasStateAuthority)
                return;

            EvaluateAndBehave();
            ResetTarget();
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            removeEnemyDel?.Invoke(this);
        }

        private void EvaluateAndBehave()
        {
            float highScore = float.MinValue;

            activeBehaviour = null;

            foreach (var behaviour in behaviours)
            {
                if (behaviour is IBehaviour ai)
                {
                    var currentScore = ai.Evaluate();
                    if (currentScore > highScore)
                    {
                        highScore = currentScore;
                        activeBehaviour = ai;
                    }
                }
                else
                {
                    Debug.LogError("Behaviour must derive from IBehaviour!!!");
                }
            }
            activeBehaviour?.Behave();
        }

        private void ResetTarget()
        {
            if (_targetTimer.ExpiredOrNotRunning(Runner))
            {
                SetTarget();
            }
        }

        private void SetTarget()
        {
            _target = getClosestPlayerDel?.Invoke(transform);
            foreach (var behaviour in behaviours)
            {
                if (behaviour is IBehaviour ai)
                {
                    ai.Target = _target;
                }
            }
            _targetTimer = TickTimer.CreateFromSeconds(Runner, _targetTime);            
        }

        private void SetAgent()
        {
            foreach (var behaviour in behaviours)
            {
                if (behaviour is IBehaviour ai)
                {
                    ai.Agent = _agent;
                }
            }            
        }

        private void SetWaveModifier()
        {
            foreach (var behaviour in behaviours)
            {
                if (behaviour is IBehaviour ai)
                {
                    ai.WaveModifier = (float)Math.Pow((double)_waveCount, 1.0/4.0);
                }
            }
        }

        public void OnSetWaveCount(int count)
        {
            _waveCount = count;
            SetWaveModifier();
        }
    }
}
