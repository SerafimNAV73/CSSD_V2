using UnityEngine;
using Fusion;

namespace CSSD
{
    [RequireComponent(typeof(Animator))]
    public class EnemyAnimatorHandler : NetworkBehaviour
    {
        [SerializeField] private string _attackAnimHash = "isAttacking";
        [SerializeField] private string _attackSpeedParamHash = "AttackSpeed";
        [SerializeField] private string _deathTriggerAnimHash = "Death";
        [SerializeField] private string _spawnTriggerAnimHash = "Spawn";
        [SerializeField] private string _deathBoolAnimHash = "isDead";

        private Animator _anim;

        public override void Spawned()
        {
            _anim = GetComponent<Animator>();
        }

        public void AttackAnim(bool isAttacking, float attackSpeed)
        {
            _anim?.SetBool(_attackAnimHash, isAttacking);
            _anim?.SetFloat(_attackSpeedParamHash, attackSpeed);
        }

        public void DeathAnim()
        {
            _anim?.SetTrigger(_deathTriggerAnimHash);
            _anim?.SetBool(_deathBoolAnimHash, true);
        }

        public void RespawnAnim()
        {
            _anim?.SetBool(_deathBoolAnimHash, false);
            _anim?.SetTrigger(_spawnTriggerAnimHash);
        }
    }
}