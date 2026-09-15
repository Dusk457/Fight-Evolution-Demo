using System.Collections.Generic;
using UnityEngine;

namespace FightGame
{
    public class Hitbox : MonoBehaviour
    {
        public FighterController owner;
        public float damage = 8f;

        private readonly HashSet<FighterController> _hitOnce = new HashSet<FighterController>();

        public void ResetHits() => _hitOnce.Clear();

        void OnTriggerEnter(Collider other)
        {
            Hurtbox hurt = other.GetComponentInParent<Hurtbox>();
            if (hurt == null || hurt.Owner == null) return;
            if (hurt.Owner == owner) return;
            if (_hitOnce.Contains(hurt.Owner)) return;

            _hitOnce.Add(hurt.Owner);
            Debug.Log($"[Hitbox] {(owner != null ? owner.name : "?")} 的命中框 → 命中 {hurt.Owner.name}");
            hurt.Owner.TakeDamage(damage);
        }
    }
}
