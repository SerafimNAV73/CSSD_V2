using Fusion;
using System;
using System.Collections;
using UnityEngine;
using FMODUnity;

namespace CSSD
{
    [RequireComponent(typeof(Collider2D))]
    public class HPHandler : NetworkBehaviour
    {
        [Header("Animator")]
        [SerializeField] private CharacterAnimatorHandler _anim;

        [SerializeField] private int _startMaxHealth = 100;

        [Header("Sound events")]
        [SerializeField] private EventReference _hitEvent;
        [SerializeField] private EventReference _deathEvent;

        [Networked] public int CurHP { get; set; }
        [Networked] public int MaxHP { get; set; }
        [Networked] public bool IsDead { get; set; }

        private bool _isInitialized = false;

        private Action initializeDel;
        private Action deathDel;
        private Action reviveDel;

        [Header("Damage Reduction Multiplier")]
        [SerializeField] private int _defaultDRM = 1;
        [SerializeField] private int _currentDRM = 1;
        private bool _hasDamageReduction = false;
        private GameObject _damageReductionUIIcon;
        private IEnumerator _damageReductionCO;

        //Other components
        private HitboxRoot _hitboxRoot;
        private ViewModel _viewModel;
        private Collider2D _collider;

        private ChangeDetector _changes;

        private void Awake()
        {
            _hitboxRoot = GetComponentInChildren<HitboxRoot>();
            _viewModel = GetComponentInChildren<ViewModel>();
            _collider = GetComponentInChildren<Collider2D>();
        }

        private void OnEnable()
        {
            PauseMenuHandler.restartDel += Restart;
        }

        private void OnDisable()
        {
            PauseMenuHandler.restartDel -= Restart;
        }

        public override void Spawned()
        {
            _changes = GetChangeDetector(ChangeDetector.Source.SimulationState);

            MaxHP = _startMaxHealth;

            _isInitialized = true;
            initializeDel?.Invoke();

            ChangeCurHealthView();
            ChangeMaxHealthView();

            if (IsDead) return;

            CurHP = MaxHP;
        }

        public override void Render()
        {
            foreach (var change in _changes.DetectChanges(this, out var previousBuffer, out var currentBuffer))
            {
                switch (change)
                {
                    case nameof(CurHP):
                        var reader1 = GetPropertyReader<int>(nameof(CurHP));
                        var (previous1, current1) = reader1.Read(previousBuffer, currentBuffer);
                        OnCurHPChanged(previous1, current1);
                        break;
                    case nameof(IsDead):
                        var reader2 = GetPropertyReader<bool>(nameof(IsDead));
                        var (previous2, current2) = reader2.Read(previousBuffer, currentBuffer);
                        OnStateChanged(previous2, current2);
                        break;
                    case nameof(MaxHP):
                        var reader3 = GetPropertyReader<int>(nameof(MaxHP));
                        var (previous3, current3) = reader3.Read(previousBuffer, currentBuffer);
                        OnMaxHPChanged(previous3, current3);
                        break;
                }
            }
        }

        private void OnCurHPChanged(int oldValue, int newValue)
        {
            //Debug.Log($"{Time.time} OnCurHPChanged value {newValue}");

            if (newValue < oldValue)
                OnCurHPReduced();

            if (newValue > oldValue)
                OnCurHPRaised();

            ChangeCurHealthView();
        }

        private void OnMaxHPChanged(int oldValue, int newValue)
        {
            //Debug.Log($"{Time.time} OnMaxHPChanged value {newValue}");

            if (newValue != oldValue)
                OnMaxHPChange(oldValue, newValue);

            ChangeMaxHealthView();
        }

        private void OnCurHPReduced()
        {
            if (!_isInitialized) return;

            _anim.HitAnim();
        }

        private void OnCurHPRaised()
        {
            if (!_isInitialized) return;

            //StartCoroutine(OnHitCO());
        }

        private void OnMaxHPChange(int oldMHP, int newMHP)
        {
            if (oldMHP != 0)
                CurHP = Mathf.CeilToInt((float)CurHP / (float)oldMHP * (float)newMHP);
        }

        private void OnStateChanged(bool oldValue, bool newValue)
        {
            //Debug.Log($"{Time.time} OnStateChanged value {newValue}");

            if (newValue)
                OnDeath();
            else if (!newValue && oldValue)
                OnRevive();
        }

        private void OnDeath()
        {
            //Debug.Log($"{Time.time} {transform.name} OnDeath");

            if(!_deathEvent.IsNull)
                AudioManager._instance.PlayEvent(_deathEvent, transform.position);

            if (_anim != null)
                _anim.DeathAnim();
            else
                Debug.LogWarning("Lost health animator handler");

            deathDel?.Invoke();
            _hitboxRoot.HitboxRootActive = false;
            _collider.enabled = false;
        }

        private void OnRevive()
        {
            //Debug.Log($"{Time.time} {transform.name} OnRevive");

            if (_anim != null)
                _anim.RespawnAnim();
            else
                Debug.LogWarning("Lost health animator handler");

            reviveDel?.Invoke();
            _hitboxRoot.HitboxRootActive = true;
            _collider.enabled = true;
        }

        private void ChangeCurHealthView()
        {
            if (_viewModel == null) return;

            _viewModel.CurHealth = CurHP.ToString();
            _viewModel.RatioHealth = (float)CurHP / (float)MaxHP;
        }

        private void ChangeMaxHealthView()
        {
            if (_viewModel == null) return;

            _viewModel.MaxHealth = MaxHP.ToString();
            _viewModel.RatioHealth = (float)CurHP / (float)MaxHP;
        }

        private IEnumerator DamageReductionCO(float time)
        {
            yield return new WaitForSeconds(time);
            _hasDamageReduction = false;
            _currentDRM = _defaultDRM;
        }

        private void Restart()
        {
            if (_hasDamageReduction)
            {
                StopCoroutine(_damageReductionCO);
                if (_damageReductionUIIcon != null) Destroy(_damageReductionUIIcon);
                else Debug.LogError("UIIcon lost");
            }
        }

        public void DamageReductionUpgrade(NetworkPlayer player, int value, float time, GameObject UIIcon)
        {
            if (_hasDamageReduction)
            {
                StopCoroutine(_damageReductionCO);
                if (_damageReductionUIIcon != null) Destroy(_damageReductionUIIcon);
                else Debug.LogError("UIIcon lost");
            }
            else
            {
                _hasDamageReduction = true;
            }

            _currentDRM = _defaultDRM * value;

            _damageReductionCO = DamageReductionCO(time);
            StartCoroutine(_damageReductionCO);
            _damageReductionUIIcon = Instantiate(UIIcon, player.BonusesUIRoot.transform, false);

            if (_damageReductionUIIcon.TryGetComponent<UIBonusIcon>(out var icon))
            {
                icon.SetTime(time);
            }
            else
            {
                Debug.LogError("Lost icon bonus script");
            }
        }

        //Function only called on the server
        public void OnTakeDamage(int damageAmount)
        {
            //Only take damage while alive
            if (IsDead) return;

            damageAmount /= _currentDRM;

            if (damageAmount > CurHP)
                damageAmount = CurHP;

            CurHP -= damageAmount;
                
            if (!_hitEvent.IsNull)
                AudioManager._instance.PlayEvent(_hitEvent, transform.position);

            //Debug.Log($"{Time.time} {transform.name} took {damageAmount} damage and got {CurHP} left.");

            //Death
            if (CurHP <= 0)
            {
                //Debug.Log($"{Time.time} {transform.name} died.");
                IsDead = true;
            }
        }

        public void OnTakeDamage(int damageAmount, NetworkPlayer player)
        {
            //Only take damage while alive
            if (IsDead) return;

            damageAmount /= _currentDRM;

            if (damageAmount > CurHP)
                damageAmount = CurHP;

            CurHP -= damageAmount;
            //Debug.Log($"{Time.time} {transform.name} took {damageAmount} damage and got {CurHP} left.");
            
            if (!_hitEvent.IsNull)
                AudioManager._instance.PlayEvent(_hitEvent, transform.position);

            //Death
            if (CurHP <= 0)
            {
                //Debug.Log($"{Time.time} {transform.name} died.");
                player.UpKillCount();
                IsDead = true;
            }
        }

        public void OnHeal(int healAmount)
        {
            if (IsDead) return;

            if (CurHP + healAmount <= CurHP) return;

            if (CurHP + healAmount > MaxHP)
                healAmount = MaxHP - CurHP;

            CurHP += healAmount;
        }

        public void OnRespawned()
        {
            //Reset variables
            CurHP = MaxHP;
            IsDead = false;
        }

        public void OnMaxHPRaise(int count)
        {
            if (IsDead) return;

            if (MaxHP + _startMaxHealth * count / 100 <= MaxHP)
                return;
            else
                MaxHP += _startMaxHealth * count / 100;
        }

        public void SetStartMaxHP(int maxHP)
        {
            _startMaxHealth = maxHP;
            MaxHP = maxHP;
        }

        public void SetDeath(Action deathMethod)
        {
            deathDel += deathMethod;
        }

        public void UnSetDeath(Action deathmethod)
        {
            deathDel -= deathmethod;
        }

        public void SetRevive(Action reviveMethod)
        {
            reviveDel += reviveMethod;
        }

        public void UnSetRevive(Action reviveMethod)
        {
            reviveDel -= reviveMethod;
        }

        public void SetInitialize(Action initializeMethod)
        {
            initializeDel += initializeMethod;
        }

        public void UnSetInitialize(Action initializeMethod)
        {
            initializeDel -= initializeMethod;
        }
    }
}