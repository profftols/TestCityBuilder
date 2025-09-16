using UnityEngine;

namespace Applications
{
    /// <summary>
    /// DTO для передачи данных о здании.
    /// </summary>
    public class BuildingData
    {
        public string Id { get; set; } 
        public BuildType Type { get; set; } 
        public Vector2Int Position { get; set; } 
        public int Level { get; set; } 
        public int Rotation { get; set; } 
    }
}