using UnityEngine;

namespace FightGame
{
    public class FightManager : MonoBehaviour
    {
        public FighterController player1;
        public FighterController player2;

        void Start()
        {
            if (player1 != null)
            {
                player1.Opponent = player2;
            }
            if (player2 != null)
            {
                player2.Opponent = player1;
            }
        }
    }
}
