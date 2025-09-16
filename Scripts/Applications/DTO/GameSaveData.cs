using System.Collections.Generic;

namespace Applications.DTO
{
    [System.Serializable]
    public class GameSaveData
    {
        public int Gold;
        public List<BuildingData> Buildings;
    }
}