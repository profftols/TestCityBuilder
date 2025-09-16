using Cysharp.Threading.Tasks;
using MessagePipe;
using UnityEngine;
using VContainer;

namespace Applications
{
    public class GridService
    {
        private readonly bool[,] _grid;
        private readonly int _width;
        private readonly int _height;
        private readonly IAsyncPublisher<CellStateChanged> _publisher;

        [Inject]
        public GridService(IAsyncPublisher<CellStateChanged> publisher)
        {
            _publisher = publisher;
            _width = 32; // Например, 10x10
            _height = 32;
            _grid = new bool[_width, _height];
        }
    
        /// <summary>
        /// Проверяет, можно ли разместить строение заданного размера в указанной позиции.
        /// </summary>
        /// <param name="startPosition">Левая верхняя координата</param>
        /// <param name="width">Ширина строения</param>
        /// <param name="height">Высота строения</param>
        /// <returns>True, если все ячейки свободны, иначе False</returns>
        public bool CanPlace(Vector2Int startPosition, int width, int height)
        {
            if (startPosition.x < 0 || startPosition.y < 0 || 
                startPosition.x + width > _width || startPosition.y + height > _height)
            {
                return false;
            }

            for (int y = startPosition.y; y < startPosition.y + height; y++)
            {
                for (int x = startPosition.x; x < startPosition.x + width; x++)
                {
                    if (_grid[x, y])
                    {
                        return false;
                    }
                }
            }
        
            return true; 
        }
    
        /// <summary>
        /// Занимает или освобождает ячейки для строения и публикует события.
        /// </summary>
        /// <param name="startPosition">Левая верхняя координата</param>
        /// <param name="width">Ширина строения</param>
        /// <param name="height">Высота строения</param>
        /// <param name="isOccupied">True для занятия, False для освобождения</param>
        public async UniTask OccupyCells(Vector2Int startPosition, int width, int height, bool isOccupied)
        {
            for (int y = startPosition.y; y < startPosition.y + height; y++)
            {
                for (int x = startPosition.x; x < startPosition.x + width; x++)
                {
                    _grid[x, y] = isOccupied;
                
                    await _publisher.PublishAsync(new CellStateChanged(new Vector2Int(x, y), isOccupied));
                }
            }
        }
    }
}
