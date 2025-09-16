using UnityEngine;

namespace Applications.DTO
{
    /// <summary>
    /// DTO для запроса на строительство здания.
    /// </summary>
    public class BuildBuildingRequest
    {
        public BuildType BuildingTypeId;
        public Vector2Int Position;
    }
}