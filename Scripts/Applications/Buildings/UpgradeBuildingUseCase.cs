using Applications.DTO;
using Applications.Services;
using Cysharp.Threading.Tasks;
using Domain.Data;
using MessagePipe;
using VContainer;

namespace Applications.Buildings
{
    public class UpgradeBuildingUseCase
    {
        private readonly BuildingService _buildingService;
    private readonly EconomyService _economyService;
    private readonly IAsyncPublisher<BuildingUpgraded> _publisher;
    private readonly IAsyncPublisher<NotificationEvent> _notificationPublisher;

    [Inject]
    public UpgradeBuildingUseCase(
        BuildingService buildingService,
        EconomyService economyService,
        IAsyncPublisher<BuildingUpgraded> publisher,
        IAsyncPublisher<NotificationEvent> notificationPublisher)
    {
        _buildingService = buildingService;
        _economyService = economyService;
        _publisher = publisher;
        _notificationPublisher = notificationPublisher;
    }

    /// <summary>
    /// Выполняет логику улучшения здания.
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

        BuildingTypeSO buildingType = _buildingService.GetBuildingType(buildingData.Type);
        if (buildingType == null)
        {
            await _notificationPublisher.PublishAsync(new NotificationEvent("Ошибка: Тип здания не найден."));
            return;
        }

        int nextLevel = buildingData.Level + 1;
        if (nextLevel > buildingType.levels.Length)
        {
            await _notificationPublisher.PublishAsync(new NotificationEvent("Это здание достигло максимального уровня!"));
            return;
        }

        BuildingLevelData nextLevelData = buildingType.levels[nextLevel - 1];
        if (!_economyService.TryChangeGold(-nextLevelData.GoldCost))
        {
            await _notificationPublisher.PublishAsync(new NotificationEvent("Недостаточно золота для улучшения!"));
            return;
        }

        buildingData.Level = nextLevel;
        _buildingService.UpdateBuildingData(buildingData);

        await _publisher.PublishAsync(new BuildingUpgraded(buildingData.Id, nextLevel));
        await _notificationPublisher.PublishAsync(new NotificationEvent("Здание успешно улучшено!"));
    }
    }

}
