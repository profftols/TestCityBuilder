namespace Applications
{
    public struct EconomyStateChanged
    {
        public readonly int CurrentGold;

        public EconomyStateChanged(int currentGold)
        {
            CurrentGold = currentGold;
        }
    }
}