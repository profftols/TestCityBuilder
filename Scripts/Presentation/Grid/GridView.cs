using System;
using Applications;
using MessagePipe;
using UnityEngine;
using VContainer;

namespace Presentation.Grid
{
    public class GridView : MonoBehaviour
    {
        // Префаб одной клетки сетки.
        [SerializeField] private GameObject cellPrefab;

        // Ссылки на материалы для разных состояний клетки.
        [SerializeField] private Material freeCellMaterial;
        [SerializeField] private Material occupiedCellMaterial;

        private GameObject[,] _cells;
        private IDisposable _cellStateDisposable;

        // Внедрение зависимостей с помощью VContainer
        [Inject]
        public void Construct(IAsyncSubscriber<CellStateChanged> cellStateSubscriber)
        {
            // Подписываемся на события изменения состояния клетки.
            _cellStateDisposable = cellStateSubscriber.Subscribe(
                async (cellState, cancellationToken) =>
                {
                    // Обновляем визуальное состояние клетки в главном потоке.
                    UpdateCellVisual(cellState.Position, cellState.IsOccupied);
                });
        }

        // Инициализация сетки при запуске
        private void Start()
        {
            // Для простоты жестко зададим размеры, можно брать из GridService.
            int width = 32;
            int height = 32;

            _cells = new GameObject[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var cell = Instantiate(cellPrefab, new Vector3(x, 0, y), Quaternion.identity, transform);
                    _cells[x, y] = cell;
                    // Изначально все клетки свободны
                    _cells[x, y].GetComponent<Renderer>().material = freeCellMaterial;
                }
            }
        }

        // Отписываемся от событий при уничтожении объекта
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