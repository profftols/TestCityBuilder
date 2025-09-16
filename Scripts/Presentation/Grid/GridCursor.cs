using Applications;
using Domain.Data;
using UnityEngine;
using VContainer;

namespace Presentation.Grid
{
    public class GridCursor : MonoBehaviour
    {
        [SerializeField] private MeshRenderer cursorRenderer;
        [SerializeField] private Material canPlaceMaterial;
        [SerializeField] private Material cannotPlaceMaterial;

        private Camera _mainCamera;
        private GridService _gridService;
        private int _currentWidth;
        private int _currentHeight;

        public Vector2Int CurrentPosition { get; private set; }

        [Inject]
        public void Construct(GridService gridService)
        {
            _gridService = gridService;
        }

        private void Awake()
        {
            _mainCamera = Camera.main;
            UpdateCursorVisual(false);
        }

        public void UpdateCursor(BuildingTypeSO buildingType)
        {
            _currentWidth = buildingType.width;
            _currentHeight = buildingType.height;

            UpdateCursorVisual(true);
            Update();
        }

        public void HideCursor()
        {
            UpdateCursorVisual(false);
        }

        private void UpdateCursorVisual(bool isVisible)
        {
            cursorRenderer.gameObject.SetActive(isVisible);
        }
    
        private void Update()
        {
            if (!cursorRenderer.gameObject.activeSelf) return;

            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                int x = Mathf.RoundToInt(hit.point.x);
                int y = Mathf.RoundToInt(hit.point.z);
                CurrentPosition = new Vector2Int(x, y);

                transform.position = new Vector3(x, 0.01f, y);

                bool canPlace = _gridService.CanPlace(CurrentPosition, _currentWidth, _currentHeight);
                cursorRenderer.material = canPlace ? canPlaceMaterial : cannotPlaceMaterial;
            }
        }
    }

}
