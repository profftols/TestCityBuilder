using UnityEngine;

namespace Applications
{
    public struct CellStateChanged
    {
        public readonly Vector2Int Position;
        public readonly bool IsOccupied;

        public CellStateChanged(Vector2Int position, bool isOccupied)
        {
            Position = position;
            IsOccupied = isOccupied;
        }
    }
}