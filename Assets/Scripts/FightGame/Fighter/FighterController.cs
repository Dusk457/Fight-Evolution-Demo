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

        public float HitStun => hitStun;

        public float CurrentHp 
        {
            get; 
            private set; 
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
            bool canMove = !(_sm.Current is AttackState) && !(_sm.Current is HitState);

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

        public void EnableHitbox(float damage)
        {
            if (hitbox == null) hitbox = GetComponentInChildren<Hitbox>(true);
            if (hitbox == null) return;
            if (hitbox.gameObject == gameObject) return;
            hitbox.owner = this;
            hitbox.damage = damage;
            hitbox.gameObject.SetActive(true);
        }

        public void DisableHitbox()
        {
            if (hitbox == null) return;
            if (hitbox.gameObject == gameObject) return;
            hitbox.gameObject.SetActive(false);
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

            CurrentHp = Mathf.Max(0f, CurrentHp - dmg);
            Debug.Log($"[Fighter] {name} 受到 {dmg} 伤害，剩余 HP {CurrentHp}");

            _sm.ChangeState(new HitState());

            if (CurrentHp <= 0f)
            {
                Debug.Log($"[Fighter] {name} 死亡");
            }
        }
    }
}
