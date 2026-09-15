using UnityEngine;

namespace FightGame
{
    public class KeyboardInputSource : MonoBehaviour, IInputSource
    {
        [Header("移动按键")]
        public KeyCode up = KeyCode.W, down = KeyCode.S, left = KeyCode.A, right = KeyCode.D;

        [Header("动作按键")]
        public KeyCode punch = KeyCode.J;
        public KeyCode kick = KeyCode.K;
        public KeyCode block = KeyCode.L;
        public KeyCode jump = KeyCode.Space;

        public FighterInput Read()
        {
            FighterInput i = default;
            if (Input.GetKey(left)) i.x = -1f;
            if (Input.GetKey(right)) i.x = 1f;
            if (Input.GetKey(up)) i.z = 1f;
            if (Input.GetKey(down)) i.z = -1f;

            i.punch = Input.GetKeyDown(punch);
            i.kick = Input.GetKeyDown(kick);
            i.block = Input.GetKey(block);
            i.jump = Input.GetKeyDown(jump);
            return i;
        }
    }
}
