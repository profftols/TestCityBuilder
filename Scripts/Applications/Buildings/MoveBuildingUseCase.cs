using Applications.DTO;
using Cysharp.Threading.Tasks;
using Domain.Data;
using MessagePipe;
using UnityEngine;
using VContainer;

namespace Applications.Buildings
{
    public class MoveBuildingUseCase
    {
        private readonly GridService _gridService;
        private readonly BuildingService _buildingService;
        private readonly IAsyncPublisher<BuildingMoved> _publisher;
        private readonly IAsyncPublisher<NotificationEvent> _notificationPublisher;

        [Inject]
        public MoveBuildingUseCase(
            GridService gridService,
            BuildingService buildingService,
            IAsyncPublisher<BuildingMoved> publisher,
            IAsyncPublisher<NotificationEvent> notificationPublisher)
        {
            _gridService = gridService;
            _buildingService = buildingService;
            _publisher = publisher;
            _notificationPublisher = notificationPublisher;
        }

        /// <summary>
        /// Выполняет логику перемещения здания.
        /// </summary>
        /// <param name="buildingId">Уникальный ID здания.</param>
        /// <param name="newPosition">Новая позиция на сетке.</param>
        public async UniTask Execute(string buildingId, Vector2Int newPosition)
        {
            BuildingData buildingData = _buildingService.GetBuildingById(buildingId);
            if (buildingData == null)
            {
                await _notificationPublisher.PublishAsync(new NotificationEvent("Ошибка: Здание не найдено."));
                return;
            }

            BuildingTypeSO buildingType = _buildingService.GetBuildingType(buildingData.Type);
            if (buildingType == null)
            {
                await _notificationPublisher.PublishAsync(new NotificationEvent("Ошибка: Тип здания не найден."));
                return;
            }

            await _gridService.OccupyCells(buildingData.Position, buildingType.width, buildingType.height, false);

            if (!_gridService.CanPlace(newPosition, buildingType.width, buildingType.height))
            {
                await _gridService.OccupyCells(buildingData.Position, buildingType.width, buildingType.height, true);
                await _notificationPublisher.PublishAsync(
                    new NotificationEvent("Невозможно переместить сюда. Клетки заняты."));
                return;
            }

            await _gridService.OccupyCells(newPosition, buildingType.width, buildingType.height, true);

            buildingData.Position = newPosition;
            _buildingService.UpdateBuildingData(buildingData);

            await _publisher.PublishAsync(new BuildingMoved(buildingData.Id, newPosition));
            await _notificationPublisher.PublishAsync(new NotificationEvent("Здание перемещено."));
        }
    }

}
