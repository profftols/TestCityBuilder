using System;
using System.Collections.Generic;
using System.Linq;
using Applications.DTO;
using Cysharp.Threading.Tasks;
using Domain.Data;
using Infrastructure;
using MessagePipe;
using UnityEngine;
using VContainer;

namespace Applications.Buildings
{
    public class BuildingService : IDisposable
    {
        private readonly List<BuildingData> _buildings = new();
        private readonly IAsyncSubscriber<BuildingBuilt> _buildingBuiltSubscriber;
        private readonly BuildingTypeSO[] _buildingTypes;
        private readonly IAsyncPublisher<NotificationEvent> _notificationPublisher;
        private readonly Dictionary<string, BuildingData> _buildingDictionary = new();
        
        private IDisposable _buildingBuiltDisposable;

        [Inject]
        public BuildingService(IAsyncSubscriber<BuildingBuilt> buildingBuiltSubscriber, IAsyncPublisher<NotificationEvent> notificationPublisher)
        {
            _buildingBuiltSubscriber = buildingBuiltSubscriber;
            _notificationPublisher = notificationPublisher;
        
            _buildingTypes = Resources.LoadAll<BuildingTypeSO>(AssetPath.buildingsTypePath);
        
            // Просто подписываемся и сохраняем IDisposable
            _buildingBuiltDisposable = _buildingBuiltSubscriber.Subscribe(
                async (buildingBuilt, cancellationToken) => await OnBuildingBuilt(buildingBuilt));
            
        }

        private async UniTask OnBuildingBuilt(BuildingBuilt buildingBuilt)
        {
            _buildingDictionary[buildingBuilt.BuildingData.Id] = buildingBuilt.BuildingData;
        }

        /// <summary>
        /// Рассчитывает общий доход в секунду от всех построенных зданий.
        /// </summary>
        public int CalculateTotalIncome()
        {
            int totalIncome = 0;
            foreach (var building in _buildings)
            {
                BuildingTypeSO type = _buildingTypes.FirstOrDefault(bt => (int)bt.buildTypeId == (int)building.Type);
                
                if (type != null)
                {
                    BuildingLevelData levelData = type.levels[building.Level - 1]; // Уровни начинаются с 1
                    totalIncome += levelData.IncomePerSecond;
                }
            }

            return totalIncome;
        }

        // Реализация IDisposable для корректной очистки
        public void Dispose()
        {
            _buildingBuiltDisposable?.Dispose();
        }
        
        public List<BuildingData> GetBuildings()
        {
            return new List<BuildingData>(_buildings);
        }

        public async UniTask LoadBuildings(List<BuildingData> buildings)
        {
            _buildingDictionary.Clear();

            foreach (var buildingData in buildings)
            {
                _buildingDictionary[buildingData.Id] = buildingData;
        
                await _notificationPublisher.PublishAsync(new NotificationEvent($"Загружено здание {buildingData.Type} на {buildingData.Position}"));
            }
        }
        
        public void RemoveBuilding(string buildingId)
        {
            _buildingDictionary.Remove(buildingId);
        }

        public void UpdateBuildingData(BuildingData buildingData)
        {
            if (_buildingDictionary.ContainsKey(buildingData.Id))
            {
                _buildingDictionary[buildingData.Id] = buildingData;
            }
        }
        
        public BuildingTypeSO GetBuildingType(BuildType type)
        {
            return _buildingTypes.FirstOrDefault(bt => (int)bt.buildTypeId == (int)type);
        }
        
        /// <summary>
        /// Находит здание по его уникальному ID.
        /// </summary>
        public BuildingData GetBuildingById(string buildingId)
        {
            return _buildingDictionary.TryGetValue(buildingId, out BuildingData data) ? data : null;
        }
    }
}