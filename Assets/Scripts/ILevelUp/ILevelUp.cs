namespace CSSD
{
    public interface ILevelUp
    {
        void LevelUp(NetworkPlayer player, int level);
        public int MinLevel { get; }
    }
}