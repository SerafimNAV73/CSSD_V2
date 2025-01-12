using System;
using UnityEngine;
using UnityEngine.AI;
using Fusion;

namespace CSSD
{
    public class RetreatBehaviour : NetworkBehaviour, IBehaviour
    {
        [SerializeField] private float _retreatDistance = 3;
        [SerializeField] private float _retreatSpeed = 1;
        [SerializeField] private float _retreatPointChangeCooldown = 2;
        [SerializeField] private float _evaluationPriority = 0.8f;

        private float _currentTime = 0;

        private Transform _retreatPoint;

        public static Func<Transform> getRetreatPointDel;

        private Vector3 SelfPos => transform.position;
        public NavMeshAgent Agent { get; set; }
        public NetworkPlayer Target { get; set; }
        public float WaveModifier { get; set; }

        private void Start()
        {
            if (getRetreatPointDel != null)
                _retreatPoint = getRetreatPointDel.Invoke();
            else
                Debug.Log("Lost retreat point!");
        }

        private void OnValidate()
        {
            if (_evaluationPriority < 0) _evaluationPriority = 0;
        }

        public override void FixedUpdateNetwork()
        {
            if (!Object.HasStateAuthority) return;

            CheckRetreatPoint();
        }

        private void CheckRetreatPoint()
        {
            if (_currentTime > _retreatPointChangeCooldown)
            {
                if (Vector3.Distance(SelfPos, _retreatPoint.position) < 0.1f)
                {
                    if (getRetreatPointDel != null)
                        _retreatPoint = getRetreatPointDel.Invoke();
                    else
                    {
                        Debug.LogWarning("Lost retreat point!");
                        return;
                    }
                    _currentTime = 0;
                }
                else return;
            }
            else
                _currentTime += Time.deltaTime;
        }

        public float Evaluate()
        {
            if (Target == null)
            {
                Agent.destination = SelfPos;
                return 0;
            }
            else
            {
                float dist = Vector2.Distance(transform.position, Target.transform.position);
                if (dist < _retreatDistance / WaveModifier)
                {
                    Agent.updatePosition = true;
                    return _evaluationPriority;
                }
                else
                    return 0;
            }
        }

        public void Behave()
        {
            Agent.speed = _retreatSpeed * WaveModifier;
            if (getRetreatPointDel != null)
            {
                Agent.destination = _retreatPoint.position;
                //Debug.Log("Retreat to: " + _retreatPoint.position);
            }
            else
                Debug.LogWarning("Lost retreat point!");
        }
    }
}