using UnityEngine;

namespace FightGame
{
    public class FighterAnimEvents : MonoBehaviour
    {
        private FighterController _controller;

        void Awake()
        {
            _controller = GetComponent<FighterController>();
            if (_controller == null) _controller = GetComponentInParent<FighterController>();
            if (_controller == null) _controller = GetComponentInChildren<FighterController>();
        }
        // 命中帧开始开命中框
        public void OnHitStart()
        {
            if (_controller != null)
            {
                _controller.EnableHitbox(_controller.CurrentMove.damage);
            }
        }

        // 命中帧结束关命中框
        public void OnHitEnd()
        {
            if (_controller != null)
            {
                _controller.DisableHitbox();
            }
        }

        public void OnAttackEnd()
        {
            if (_controller != null)
            {
                _controller.EndAttack();
            }
        }
    }
}
