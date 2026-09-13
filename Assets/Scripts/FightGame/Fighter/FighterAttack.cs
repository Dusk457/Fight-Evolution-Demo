using UnityEngine;

namespace FightGame
{
    public class FighterAttack : MonoBehaviour
    {
        private FighterController _controller;
        private IInputSource _input;

        void Awake()
        {
            _controller = GetComponent<FighterController>();
            _input = GetComponent<IInputSource>();

            Hitbox[] all = GetComponentsInChildren<Hitbox>(true);
            foreach (Hitbox hb in all)
            {
                hb.owner = _controller;
                hb.gameObject.SetActive(false);
            }
            if (_controller != null && _controller.hitbox == null && all.Length > 0)
            {
                _controller.hitbox = all[0];
            }
        }

        void Update()
        {
            if (_input == null || _controller == null) return;

            FighterInput input = _input.Read();

            if (input.punch)
            {
                Debug.Log($"[FighterAttack] {name} 拳");
                _controller.TryAttack(_controller.punchMove);
            }
            if (input.kick)
            {
                Debug.Log($"[FighterAttack] {name} 踢");
                _controller.TryAttack(_controller.kickMove);
            }

            _controller.TickStateMachine();
        }
    }
}
