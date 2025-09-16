using Applications;
using UnityEngine;

namespace Domain.Data
{
    [CreateAssetMenu(fileName = "NewBuildingType", menuName = "City Builder/Building Type", order = 0)]
    public class BuildingTypeSO : ScriptableObject
    {
        public string title;
        public BuildType buildTypeId;
        public string displayName;
        public int width;
        public int height;
        public GameObject prefab;
        public Sprite icon;

        public BuildingLevelData[] levels;
    }

}
