using System;

namespace FightGame
{
    [Serializable]
    public struct AttackMove
    {
        public string animTrigger;
        public float damage;
        public float duration;

        public AttackMove(string animTrigger, float damage, float duration)
        {
            this.animTrigger = animTrigger;
            this.damage = damage;
            this.duration = duration;
        }
    }
}
