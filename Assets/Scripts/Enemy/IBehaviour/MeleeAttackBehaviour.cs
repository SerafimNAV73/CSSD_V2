using UnityEngine;
using UnityEngine.AI;
using Fusion;

namespace CSSD
{
    public class MeleeAttackBehaviour : NetworkBehaviour, IBehaviour
    {
        [SerializeField] private CharacterAnimatorHandler _anim;

        [SerializeField] private AttackSphere _attackObject;
        [SerializeField] private Transform _attackPoint;
        [SerializeField] private float _cooldownTime = 1;
        [SerializeField] private float _attackDistance = 1;
        [SerializeField] private float _attackMoveSpeed = 0.01f;
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

        public override void FixedUpdateNetwork()
        {
            if (_cooldown.ExpiredOrNotRunning(Runner))
                _isReadyToAttack = true;
        }

        public float Evaluate()        
        {
            if (Target == null)
            {
                Agent.destination = gameObject.transform.position;
                return 0;
            }
            else
            {
                float dist = Vector2.Distance(transform.position, Target.transform.position);

                if (dist < _attackDistance || !_isReadyToAttack)
                {
                    return _evaluationPriority;
                }

                return 0;
            }            
        }

        public void Behave()
        {
            Agent.destination = Target.transform.position;
            Agent.speed = _attackMoveSpeed;
            if (_isReadyToAttack)
            {
                _anim.AttackAnim(_cooldownTime);
                _cooldown = TickTimer.CreateFromSeconds(Runner, _cooldownTime);
                _isReadyToAttack = false;
                AttackSphere attackSphere = Runner.Spawn(_attackObject, _attackPoint.position, _attackPoint.rotation);
                attackSphere.Damage = Mathf.FloorToInt((float)attackSphere.Damage * WaveModifier);
                attackSphere.Fire(_networkObject);
                AudioManager._instance.EnemyMeleeAttackEvent(transform.position);
                //Debug.Log("Attack");
            }            
        }
    }
}