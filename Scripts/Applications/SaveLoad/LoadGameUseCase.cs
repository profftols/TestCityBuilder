using Applications.Buildings;
using Applications.DTO;
using Applications.Services;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UnityEngine;
using VContainer;

namespace Applications.SaveLoad
{
    public class LoadGameUseCase
    {
        private readonly BuildingService _buildingService;
        private readonly EconomyService _economyService;
        private readonly IAsyncPublisher<NotificationEvent> _notificationPublisher;

        [Inject]
        public LoadGameUseCase(
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
            if (!PlayerPrefs.HasKey("GameSave"))
            {
                await _notificationPublisher.PublishAsync(new NotificationEvent("Нет сохранённых игр."));
                return;
            }

            string json = PlayerPrefs.GetString("GameSave");
            GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);

            // Восстанавливаем состояние
            _economyService.SetGold(saveData.Gold);
            _buildingService.LoadBuildings(saveData.Buildings);

            Debug.Log("Game loaded!");
            await _notificationPublisher.PublishAsync(new NotificationEvent("Игра загружена!"));
        }
    }
}