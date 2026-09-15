using UnityEngine;

namespace FightGame
{
    public interface IFighterState
    {
        void Enter(FighterController f);
        void Update(FighterController f);
        void Exit(FighterController f);
    }

    public class FighterStateMachine : MonoBehaviour
    {
        private IFighterState _current;
        public IFighterState Current => _current;

        private FighterController _f;
        void Awake() => _f = GetComponent<FighterController>();

        public void ChangeState(IFighterState s)
        {
            if (_current != null && _current.GetType() == s.GetType()) return; 
            _current?.Exit(_f);
            _current = s;
            _current?.Enter(_f);
        }

        public void Tick() => _current?.Update(_f);
    }


    public class IdleState : IFighterState
    {
        public void Enter(FighterController f) { f.AnimSpeed(0f); }
        public void Update(FighterController f) { }
        public void Exit(FighterController f) { }
    }

    public class WalkState : IFighterState
    {
        public void Enter(FighterController f) { f.AnimSpeed(1f); }
        public void Update(FighterController f) { }
        public void Exit(FighterController f) { }
    }
    public class AttackState : IFighterState
    {
        private readonly AttackMove _move;
        private float _t;
        private float _duration;

        public AttackState(AttackMove move) 
        { 
            _move = move;
        }

        public void Enter(FighterController f) 
        { 
            _t = 0f;
            _duration = _move.duration > 0f ? _move.duration : f.GetClipLength(_move.animTrigger);
            if (_duration <= 0f)
            {
                _duration = 0.3f;
            }

            f.SetCurrentMove(_move);
            f.ResetHitboxHits();
            f.PlayTrigger(_move.animTrigger);

            Debug.Log($"[Attack] {f.name} 进入攻击({_move.animTrigger}) 时长={_duration:F2}s 命中框=AnimationEvent");
        }
        public void Update(FighterController f)
        {
            _t += Time.deltaTime;
            if (_t >= _duration) 
            { 
                f.DisableHitbox(); 
                f.ChangeToIdle();
                Debug.Log($"[Attack] {f.name} 攻击结束 → Idle");
            }
        }
        public void Exit(FighterController f) 
        { 
            f.DisableHitbox(); 
        }
    }

    public class HitState : IFighterState
    {
        private float _t;
        public void Enter(FighterController f) 
        { 
            _t = 0f; f.PlayHitAnim(); Debug.Log($"[Hit] {f.name} 进入硬直");
        }
        public void Update(FighterController f)
        {
            _t += Time.deltaTime;
            if (_t >= f.HitStun)
            {
                 f.OnHitStunEnd();
                 Debug.Log($"[Hit] {f.name} 硬直结束");
            }
        }
        public void Exit(FighterController f) { }
    }

    public class BlockState : IFighterState
    {
        public void Enter(FighterController f)
        {
            f.SetBlockingFlag(true);
            Debug.Log($"[Block] {f.name} 进入格挡");
        }
        public void Update(FighterController f) { }
        public void Exit(FighterController f)
        {
            f.SetBlockingFlag(false);
            Debug.Log($"[Block] {f.name} 解除格挡");
        }
    }

    public class DeadState : IFighterState
    {
        public void Enter(FighterController f)
        {
            f.SetDeadFlag(true);
            Debug.Log($"[Dead] {f.name} 死亡");
        }
        public void Update(FighterController f) { }
        public void Exit(FighterController f) { }
    }
}
