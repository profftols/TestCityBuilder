using UnityEngine;

namespace Applications.DTO
{
    public struct BuildingMoved
    {
        public readonly string Id;
        public readonly Vector2Int NewPosition;
        public BuildingMoved(string id, Vector2Int newPosition)
        {
            Id = id;
            NewPosition = newPosition;
        }
    }
}