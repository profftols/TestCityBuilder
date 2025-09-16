namespace Applications.DTO
{
    public struct BuildingUpgraded
    {
        public readonly string Id;
        public readonly int NewLevel;

        public BuildingUpgraded(string id, int newLevel)
        {
            Id = id;
            NewLevel = newLevel;
        }
    }
}