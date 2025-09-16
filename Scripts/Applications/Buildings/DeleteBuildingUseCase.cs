using Applications.DTO;
using Cysharp.Threading.Tasks;
using Domain.Data;
using MessagePipe;
using VContainer;

namespace Applications.Buildings
{
    public class DeleteBuildingUseCase
    {
        private readonly GridService _gridService;
        private readonly BuildingService _buildingService;
        private readonly IAsyncPublisher<BuildingDeleted> _publisher;
        private readonly IAsyncPublisher<NotificationEvent> _notificationPublisher;

        [Inject]
        public DeleteBuildingUseCase(
            GridService gridService,
            BuildingService buildingService,
            IAsyncPublisher<BuildingDeleted> publisher,
            IAsyncPublisher<NotificationEvent> notificationPublisher)
        {
            _gridService = gridService;
            _buildingService = buildingService;
            _publisher = publisher;
            _notificationPublisher = notificationPublisher;
        }

        /// <summary>
        /// Выполняет логику удаления здания.
        /// </summary>
        /// <param name="buildingId">Уникальный ID здания.</param>
        public async UniTask Execute(string buildingId)
        {
            BuildingData buildingData = _buildingService.GetBuildingById(buildingId);
            
            if (buildingData == null)
            {
                await _notificationPublisher.PublishAsync(new NotificationEvent("Ошибка: Здание не найдено."));
                return;
            }

            // Освобождаем клетки на сетке
            BuildingTypeSO buildingType = _buildingService.GetBuildingType(buildingData.Type);
            if (buildingType != null)
            {
                await _gridService.OccupyCells(buildingData.Position, buildingType.width, buildingType.height, false);
            }

            // Удаляем здание из списка
            _buildingService.RemoveBuilding(buildingData.Id);

            // Публикуем событие об удалении
            await _publisher.PublishAsync(new BuildingDeleted(buildingData.Id));
            await _notificationPublisher.PublishAsync(new NotificationEvent("Здание удалено."));
        }
    }
}