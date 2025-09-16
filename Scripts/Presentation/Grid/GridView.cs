using System;
using Applications;
using MessagePipe;
using UnityEngine;
using VContainer;

namespace Presentation.Grid
{
    public class GridView : MonoBehaviour
    {
        [SerializeField] private GameObject cellPrefab;

        [SerializeField] private Material freeCellMaterial;
        [SerializeField] private Material occupiedCellMaterial;

        private GameObject[,] _cells;
        private IDisposable _cellStateDisposable;

        [Inject]
        public void Construct(IAsyncSubscriber<CellStateChanged> cellStateSubscriber)
        {
            _cellStateDisposable = cellStateSubscriber.Subscribe(
                async (cellState, cancellationToken) =>
                {
                    UpdateCellVisual(cellState.Position, cellState.IsOccupied);
                });
        }

        private void Start()
        {
            int width = 32;
            int height = 32;

            _cells = new GameObject[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var cell = Instantiate(cellPrefab, new Vector3(x, 0, y), Quaternion.identity, transform);
                    _cells[x, y] = cell;
                    _cells[x, y].GetComponent<Renderer>().material = freeCellMaterial;
                }
            }
        }

        private void OnDestroy()
        {
            _cellStateDisposable?.Dispose();
        }

        private void UpdateCellVisual(Vector2Int position, bool isOccupied)
        {
            if (position.x >= 0 && position.x < _cells.GetLength(0) && position.y >= 0 &&
                position.y < _cells.GetLength(1))
            {
                var cellRenderer = _cells[position.x, position.y].GetComponent<Renderer>();
                cellRenderer.material = isOccupied ? occupiedCellMaterial : freeCellMaterial;
            }
        }
    }

}

