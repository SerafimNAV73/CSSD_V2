using System;
using UnityEngine;
using Fusion;

namespace CSSD
{
    [RequireComponent(typeof(Animator))]
    public class CharacterAnimatorHandler : NetworkBehaviour, IPlayerJoined
    {
        [SerializeField] private string _moveAnimHash = "isWalking";
        [SerializeField] private string _deathTriggerAnimHash = "Death";
        [SerializeField] private string _spawnTriggerAnimHash = "Spawn";
        [SerializeField] private string _deathBoolAnimHash = "isDead";
        [SerializeField] private string _moveSpeedParamHash = "WalkSpeed";
        [SerializeField] private string _attackTriggerHash = "Attack";
        [SerializeField] private string _attackParamHash = "AttackSpeed";
        [SerializeField] private string _hitTriggerAnimHash = "Hitted";

        private Animator _anim;
        private int _racID;

        public override void Spawned()
        {
            if (_anim == null)
                _anim = GetComponent<Animator>();
        }

        private void SetRAC(int id)
        {
            if (_anim == null)
                _anim = GetComponent<Animator>();

            RuntimeAnimatorController[] animators = Resources.LoadAll<RuntimeAnimatorController>("RuntimeAnimatorControllers/");
            Array.Sort(animators, (a, b) => { return a.name.CompareTo(b.name); });

            _anim.runtimeAnimatorController = animators[id];
        }

        public void OnSetRAC()
        {
            if (Object.HasInputAuthority)
            {
                _racID = GameManager._instance.GetPlayerRACid();
                if (Object.HasStateAuthority)
                {
                    SetRAC(_racID);
                }
                else
                {
                    RPC_SetAnimatorID(_racID);
                }
            }
        }

        public void MoveAnim(Vector2 direction, float moveSpeed)
        {
            _anim?.SetBool(_moveAnimHash, Math.Abs(direction.x) > 0.05f || Math.Abs(direction.y) > 0.05f);
            _anim?.SetFloat(_moveSpeedParamHash, moveSpeed);
        }

        public void AttackAnim()
        {
            _anim?.SetTrigger(_attackTriggerHash);
        }

        public void AttackAnim(float attackSpeed)
        {
            _anim?.SetTrigger(_attackTriggerHash);
            _anim?.SetFloat(_attackParamHash, attackSpeed);
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

        public void HitAnim()
        {
            _anim?.SetTrigger(_hitTriggerAnimHash);
        }

        public void PlayerJoined(PlayerRef player)
        {
            if (Runner.LocalPlayer != player && Object.HasStateAuthority)
                RPC_SetAnimator(_racID);            
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_SetAnimatorID(int id, RpcInfo info = default)
        {
            _racID = id;
            RPC_SetAnimator(id);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_SetAnimator(int id, RpcInfo info = default)
        {
            _racID = id;
            SetRAC(id);
        }
    }
}