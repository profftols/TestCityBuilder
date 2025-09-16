using System.Linq;
using Applications.DTO;
using Applications.Services;
using Cysharp.Threading.Tasks;
using Domain.Data;
using Infrastructure;
using MessagePipe;
using UnityEngine;
using VContainer;

namespace Applications.Buildings
{
    public class BuildBuildingUseCase
    {
        private readonly GridService _gridService;
        private readonly EconomyService _economyService;
        private readonly IAsyncPublisher<BuildingBuilt> _buildingPublisher;
        private readonly IAsyncPublisher<NotificationEvent> _notificationPublisher;
        private readonly BuildingTypeSO[] _buildingTypes;

        [Inject]
        public BuildBuildingUseCase(
            GridService gridService,
            EconomyService economyService,
            IAsyncPublisher<BuildingBuilt> buildingPublisher,
            IAsyncPublisher<NotificationEvent> notificationPublisher,
            IObjectResolver container)
        {
            _gridService = gridService;
            _economyService = economyService;
            _buildingPublisher = buildingPublisher;
            _notificationPublisher = notificationPublisher;

            _buildingTypes = Resources.LoadAll<BuildingTypeSO>(AssetPath.buildingsTypePath);
        }

        /// <summary>
        /// Выполняет логику строительства здания.
        /// </summary>
        public async UniTask Execute(BuildBuildingRequest request)
        {
            BuildingTypeSO buildingType = _buildingTypes.FirstOrDefault(bt => (int)bt.buildTypeId == (int)request.BuildingTypeId);
            
            if (buildingType == null)
            {
                await _notificationPublisher.PublishAsync(new NotificationEvent("Ошибка: Неизвестный тип здания."));
                return;
            }

            if (!_gridService.CanPlace(request.Position, buildingType.width, buildingType.height))
            {
                await _notificationPublisher.PublishAsync(
                    new NotificationEvent("Невозможно разместить здесь. Клетки заняты."));
                return;
            }

            int cost = buildingType.levels[0].GoldCost;
            
            if (!_economyService.TryChangeGold(-cost))
            {
                await _notificationPublisher.PublishAsync(new NotificationEvent("Недостаточно золота!"));
                return;
            }

            await _gridService.OccupyCells(request.Position, buildingType.width, buildingType.height, true);

            var newBuildingData = new BuildingData
            {
                Id = System.Guid.NewGuid().ToString(),
                Type = (BuildType)buildingType.buildTypeId,
                Position = request.Position,
                Level = 1,
                Rotation = 0
            };

            await _buildingPublisher.PublishAsync(new BuildingBuilt(newBuildingData));
            await _notificationPublisher.PublishAsync(new NotificationEvent("Здание успешно построено!"));
        }
    }

}
