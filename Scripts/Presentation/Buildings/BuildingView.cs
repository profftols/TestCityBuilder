using UnityEngine;

namespace Presentation.Buildings
{
    public class BuildingView : MonoBehaviour
    {
        public string BuildingId { get; private set; }
    
        public void Initialize(string id)
        {
            BuildingId = id;
        }
    }
}