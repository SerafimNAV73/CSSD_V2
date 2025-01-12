using UnityEngine;
using UnityEngine.AI;
using Fusion;

namespace CSSD
{
    public class RangeAttackBehaviour : NetworkBehaviour, IBehaviour
    {
        [SerializeField] private AttackSphere _attackObject;
        [SerializeField] private Transform _attackPoint;
        [SerializeField] private float _cooldownTime = 1;
        [SerializeField] private float _attackDistance = 1;
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private float _evaluationPriority = 0.75f;

        private bool _isReadyToAttack;

        //Other components
        private NetworkObject _networkObject;

        [Networked] private TickTimer _cooldown { get; set; }
        public NavMeshAgent Agent { get; set; }
        public NetworkPlayer Target { get; set; }
        public float WaveModifier { get; set; }

        public override void Spawned()
        {
            _networkObject = GetComponent<NetworkObject>();
        }

        private void OnValidate()
        {
            if (_attackDistance < 0) _attackDistance = 0;
            if (_cooldownTime < 0) _cooldownTime = 0;
            if (_evaluationPriority < 0) _evaluationPriority = 0;
        }

        public override void FixedUpdateNetwork()
        {
            if (_cooldown.ExpiredOrNotRunning(Runner))
                _isReadyToAttack = true;
        }

        public float Evaluate()
        {
            if (Target != null)
            {
                Vector2 target = (Target.transform.position - transform.position).normalized;
                RaycastHit2D hit = Physics2D.Raycast(transform.position, target, _attackDistance, _layerMask);
                if (hit.collider != null)
                {
                    if (hit.collider.TryGetComponent<NetworkPlayer>(out var player)) 
                    {
                        Agent.nextPosition = transform.position;
                        Agent.updatePosition = false;
                        return _evaluationPriority; 
                    }                   
                }
            }

            Agent.nextPosition = transform.position;
            Agent.updatePosition = true;
            return 0;            
        }

        public void Behave()
        {
            Agent.destination = Target.transform.position;
            if (_isReadyToAttack)
            {
                _cooldown = TickTimer.CreateFromSeconds(Runner, _cooldownTime / WaveModifier);
                _isReadyToAttack = false;
                AttackSphere attackSphere = Runner.Spawn(_attackObject, _attackPoint.position, _attackPoint.rotation);
                attackSphere.Damage = Mathf.FloorToInt((float)attackSphere.Damage * WaveModifier);
                attackSphere.Fire(_networkObject);
                AudioManager._instance.EnemyRangeAttackEvent(transform.position);
                //Debug.Log("Distance Attack");
            }
        }
    }
}