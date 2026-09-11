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

        void Update() => _current?.Update(_f);
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
}
