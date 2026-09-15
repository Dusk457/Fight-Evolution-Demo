using UnityEngine;

namespace FightGame
{
    public class FighterController : MonoBehaviour
    {
        [Header("属性")]
        public float moveSpeed = 3f;
        public float maxHp = 100f;

        [Header("对手")]
        public FighterController Opponent;

        [Header("输入来源")]
        [SerializeField] 
        private MonoBehaviour inputSourceBehaviour;

        [Header("攻击 / 受击")]
        public float hitStun = 0.3f;
        public Hitbox hitbox;

        [Header("招式")]
        public AttackMove punchMove = new AttackMove("Punch", 6f, 0.3f);
        public AttackMove kickMove = new AttackMove("Kick", 12f, 0.45f);

        [Header("格挡")]
        [Range(0f, 1f)] public float blockDamageMul = 0.2f;

        public float HitStun => hitStun;

        public float CurrentHp 
        {
            get; 
            private set; 
        }
        public bool IsBlocking 
        { 
            get; 
            private set; 
        }
        public bool IsDead 
        { 
            get; 
            private set; 
        }
        public bool DeathPending 
        { 
            get; 
            private set;
        }
        public AttackMove CurrentMove 
        { 
            get; 
            private set;
        }
        public void SetCurrentMove(AttackMove move) 
        { 
            CurrentMove = move; 
        }
        private IInputSource _input;
        private FighterStateMachine _sm;
        private CharacterController _cc;
        private Animator _anim;
        private Transform _self;
        public float gravity = -20f;
        public float jumpHeight = 1.5f;
        private float _verticalVelocity;

        void Awake()
        {
            _sm = GetComponent<FighterStateMachine>();
            _cc = GetComponent<CharacterController>();
            _anim = GetComponent<Animator>();
            if (_anim == null)
            {
                _anim = GetComponentInChildren<Animator>();
            }
            _self = transform;
            CurrentHp = maxHp;
            _input = inputSourceBehaviour as IInputSource;
            if (_input == null)
            {
                _input = GetComponent<IInputSource>();
            }
            if (_input == null)
            {
                Debug.LogWarning("[Fighter] 没有输入来源(IInputSource)，不响应操作", this);
            }
        }

        void Update()
        {
            FaceOpponent();

            FighterInput input = _input != null ? _input.Read() : default;

            ApplyMove(input);

        }

        private void FaceOpponent()
        {
            if (Opponent == null) return;
            Vector3 toOpp = Opponent.transform.position - _self.position;
            toOpp.y = 0f;
            if (toOpp.sqrMagnitude > 0.01f)
            {
                _self.rotation = Quaternion.LookRotation(toOpp.normalized);
            }
        }

        private void ApplyMove(FighterInput input)
        {
            bool canMove = !(_sm.Current is AttackState) && !(_sm.Current is HitState)
                           && !(_sm.Current is BlockState) && !(_sm.Current is DeadState);

            if (canMove)
            {
                _sm.ChangeState(input.HasMove ? new WalkState() : new IdleState());
            }

            Vector3 horizontal = new Vector3(input.x, 0f, input.z);
            if (horizontal.sqrMagnitude > 0.01f)
            {
                horizontal = horizontal.normalized;
            }
            horizontal *= moveSpeed;
            if (!canMove) horizontal = Vector3.zero;

            if (_cc.isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = -2f;
            }
            else
            {
                _verticalVelocity += gravity * Time.deltaTime;
            }
               
            if (input.jump && _cc.isGrounded && canMove)
            {
                _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
            if (_cc != null)
            {
                _cc.Move((horizontal + Vector3.up * _verticalVelocity) * Time.deltaTime);
            }
            else
            {
                Debug.LogWarning("[Fighter] 缺少 CharacterController 组件，无法移动", this);
            }
        }

        public void AnimSpeed(float s) 
        { 
            if (_anim != null)
            {
                _anim.SetFloat("Speed", s);
            }
        }

        public void TryAttack(AttackMove move)
        {
            if (_sm == null) return;
            if (CurrentHp <= 0f) return;
            if (_sm.Current is AttackState) return;
            if (_sm.Current is HitState) return;
            _sm.ChangeState(new AttackState(move));
        }

        private Hitbox FindHitbox(string name)
        {
            Hitbox[] all = GetComponentsInChildren<Hitbox>(true);
            if (all == null || all.Length == 0) return null;

            if (!string.IsNullOrEmpty(name))
            {
                foreach (Hitbox hb in all)
                {
                    if (hb != null && hb.gameObject.name == name) return hb;
                }   
            }
            if (hitbox != null) return hitbox;
            hitbox = all[0];
            return hitbox;
        }

        public void EnableHitbox(float damage)
        {
            Hitbox hb = FindHitbox(CurrentMove.hitboxName);
            if (hb == null) return;
            if (hb.gameObject == gameObject) return;
            hb.owner = this;
            hb.damage = damage;
            hb.gameObject.SetActive(true);
        }

        public void DisableHitbox()
        {
            Hitbox[] all = GetComponentsInChildren<Hitbox>(true);
            foreach (Hitbox hb in all)
            {
                if (hb == null) continue;
                if (hb.gameObject == gameObject) continue;
                hb.gameObject.SetActive(false);
            }
        }

        public void ChangeToIdle() => _sm.ChangeState(new IdleState());

        public void TickStateMachine()
        {
            if (_sm != null) _sm.Tick();
        }
        public void PlayTrigger(string trigger) 
        { 
            if (_anim != null)
            {
                _anim.SetTrigger(trigger);
            }
        }
        public void PlayHitAnim() 
        { 
            if (_anim != null)
            {
                _anim.SetTrigger("Hit");
            }
        }

        public void TakeDamage(float dmg)
        {
            if (CurrentHp <= 0f) return;

            float final = IsBlocking ? dmg * blockDamageMul : dmg;
            CurrentHp = Mathf.Max(0f, CurrentHp - final);
            Debug.Log($"[Fighter] {name} 受到 {final} 伤害{(IsBlocking ? "（格挡）" : "")}，剩余 HP {CurrentHp}");

            if (CurrentHp <= 0f)
            {
                DeathPending = true;
                _sm.ChangeState(new HitState());
            }
            else if (!IsBlocking)
            {
                _sm.ChangeState(new HitState());
            }
        }

        public void RequestBlock(bool on)
        {
            if (_sm == null) return;
            if (IsDead) return;

            if (on)
            {
                if (_sm.Current is AttackState) return;
                if (_sm.Current is HitState) return;
                if (_sm.Current is BlockState) return;
                _sm.ChangeState(new BlockState());
            }
            else
            {
                if (_sm.Current is BlockState) ChangeToIdle();
            }
        }

        public void SetBlockingFlag(bool on)
        {
            IsBlocking = on;
            if (_anim != null)
            {
                _anim.SetBool("Blocking", on);
            }
        }

        public void SetDeadFlag(bool on)
        {
            IsDead = on;
            if (_anim != null)
            {
                _anim.SetBool("Dead", on);
            }
        }    

        public void OnHitStunEnd()
        {
            if (DeathPending) ChangeToDead();
            else ChangeToIdle();
        }

        public void ChangeToDead() => _sm.ChangeState(new DeadState());

        public void ResetHitboxHits()
        {
            Hitbox[] all = GetComponentsInChildren<Hitbox>(true);
            foreach (Hitbox hb in all)
            {
                if (hb != null) hb.ResetHits();
            }
        }

        public void EndAttack()
        {
            if (_sm == null) return;
            if (!(_sm.Current is AttackState)) return;
            DisableHitbox();
            ChangeToIdle();
        }

        public float GetClipLength(string clipName)
        {
            if (_anim == null)
            {
                Debug.LogWarning("[GetClipLength] _anim 为空");
                return 0f;
            }
            if (_anim.runtimeAnimatorController == null)
            {
                Debug.LogWarning("[GetClipLength] runtimeAnimatorController 为空");
                return 0f;
            }
            AnimationClip[] clips = _anim.runtimeAnimatorController.animationClips;
            if (clips == null || clips.Length == 0)
            {
                Debug.LogWarning("[GetClipLength] animationClips 为空");
                return 0f;
            }
            string names = "";
            foreach (AnimationClip c in clips)
            {
                names += (c != null ? c.name : "null") + " | ";
            }
            Debug.Log($"[GetClipLength] 要找='{clipName}'，现有 clips: {names}");

            foreach (AnimationClip c in clips)
            {
                if (c != null && c.name == clipName) return c.length;
            }

            Debug.LogWarning($"[GetClipLength] 没找到 '{clipName}'");
            return 0f;
        }
    }
}
