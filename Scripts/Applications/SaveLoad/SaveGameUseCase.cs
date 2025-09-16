using Applications.Buildings;
using Applications.DTO;
using Applications.Services;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UnityEngine;
using VContainer;

namespace Applications.SaveLoad
{
    public class SaveGameUseCase
    {
        private readonly BuildingService _buildingService;
        private readonly EconomyService _economyService;
        private readonly IAsyncPublisher<NotificationEvent> _notificationPublisher;

        [Inject]
        public SaveGameUseCase(
            BuildingService buildingService, 
            EconomyService economyService,
            IAsyncPublisher<NotificationEvent> notificationPublisher)
        {
            _buildingService = buildingService;
            _economyService = economyService;
            _notificationPublisher = notificationPublisher;
        }

        public async UniTask Execute()
        {
            var saveData = new GameSaveData
            {
                Gold = _economyService.Gold.Value,
                Buildings = _buildingService.GetBuildings()
            };

            string json = JsonUtility.ToJson(saveData);
            PlayerPrefs.SetString("GameSave", json);
            PlayerPrefs.Save();

            Debug.Log("Game saved!");
            await _notificationPublisher.PublishAsync(new NotificationEvent("Игра сохранена!"));
        }
    }
}