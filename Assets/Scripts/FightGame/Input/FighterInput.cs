namespace FightGame
{

    public struct FighterInput
    {
        public float x;
        public float z;
        public bool punch;
        public bool kick;
        public bool block;
        public bool jump;

        public bool HasMove => (x != 0f || z != 0f);
    }

    public interface IInputSource
    {
        FighterInput Read();
    }
}
