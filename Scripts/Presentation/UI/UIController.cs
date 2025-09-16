using System;
using Applications;
using Applications.Buildings;
using Applications.DTO;
using Applications.SaveLoad;
using Applications.Services;
using Cysharp.Threading.Tasks;
using Domain.Data;
using MessagePipe;
using UniRx;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace Presentation.UI
{
    public class UIController : MonoBehaviour
    {
        private VisualElement _root;
        private VisualElement _notificationContainer;
        private VisualElement _buildingInfoPanel;
        
        private Label _goldLabel;
        private Label _notificationLabel;
        private Label _buildingTitleLabel;
        private Label _buildingLevelLabel;
        private Label _upgradeCostLabel;

        private Button _upgradeButton;
        private Button _deleteButton;
        private Button _moveButton;
        private Button _saveButton;
        private Button _loadButton;

        private IDisposable _economyDisposable;
        private IDisposable _notificationDisposable;

        private IReadOnlyReactiveProperty<int> _gold;
        private IAsyncSubscriber<NotificationEvent> _notificationSubscriber;
        private UpgradeBuildingUseCase _upgradeBuildingUseCase;
        
        private SaveGameUseCase _saveGameUseCase;
        private LoadGameUseCase _loadGameUseCase;
        private BuildingService _buildingService;
        private DeleteBuildingUseCase _deleteBuildingUseCase; 
        
        private string _currentSelectedBuildingId;

        [Inject]
        public void Construct(EconomyService economyService, IAsyncSubscriber<NotificationEvent> notificationSubscriber,
            SaveGameUseCase saveGameUseCase,
            LoadGameUseCase loadGameUseCase,
            UpgradeBuildingUseCase upgradeBuildingUseCase,
            DeleteBuildingUseCase deleteBuildingUseCase,
            BuildingService buildingService)
        {
            _gold = economyService.Gold;
            _notificationSubscriber = notificationSubscriber;
            _saveGameUseCase = saveGameUseCase;
            _loadGameUseCase = loadGameUseCase;
            _upgradeBuildingUseCase = upgradeBuildingUseCase;
            _buildingService = buildingService;
            _deleteBuildingUseCase = deleteBuildingUseCase;
        }

        void OnEnable()
        {
            // Получаем корневой элемент UI Document
            _root = GetComponent<UIDocument>().rootVisualElement;

            // Находим элементы по имени
            _goldLabel = _root.Q<Label>("gold-label");
            _upgradeButton = _root.Q<Button>("upgrade-button");
            _saveButton = _root.Q<Button>("save-button");
            _loadButton = _root.Q<Button>("load-button");
            _deleteButton = _root.Q<Button>("delete-button");
            _moveButton = _root.Q<Button>("move-button");
            _buildingInfoPanel = _root.Q<VisualElement>("building-info-panel");
            _buildingTitleLabel = _root.Q<Label>("building-title-label");
            _buildingLevelLabel = _root.Q<Label>("building-level-label");
            _upgradeCostLabel = _root.Q<Label>("upgrade-cost-label");
            _notificationContainer = _root.Q<VisualElement>("notification-container");
            _notificationLabel = _root.Q<Label>("notification-label");

            // Подписываемся на реактивное свойство золота
            _economyDisposable = _gold.Subscribe(OnGoldChanged).AddTo(this);

            // Подписываемся на уведомления
            _notificationDisposable = _notificationSubscriber.Subscribe(
                async (notification, cancellationToken) => OnNotificationReceived(notification)).AddTo(this);
            
            _saveButton.clicked += OnSaveClicked;
            _loadButton.clicked += OnLoadClicked;
            _upgradeButton.clicked += OnUpgradeClicked;
            
            _upgradeButton.clicked += OnUpgradeClicked;
            _deleteButton.clicked += OnDeleteClicked;
            _moveButton.clicked += OnMoveClicked;
        }

        void OnDisable()
        {
            // Отписываемся от событий
            _economyDisposable?.Dispose();
            _notificationDisposable?.Dispose();
            _saveButton.clicked -= OnSaveClicked;
            _loadButton.clicked -= OnLoadClicked;
            _upgradeButton.clicked -= OnUpgradeClicked;
        }
        
        public void HideBuildingInfo()
        {
            _currentSelectedBuildingId = null;
            _buildingInfoPanel.style.display = DisplayStyle.None;
        }

        private void OnGoldChanged(int newGoldValue)
        {
            _goldLabel.text = $"Gold: {newGoldValue}";
        }

        private async UniTask OnNotificationReceived(NotificationEvent notification)
        {
            // Показываем панель
            _notificationLabel.text = notification.Message;
            _notificationContainer.style.display = DisplayStyle.Flex;

            // Ждём 3 секунды
            await UniTask.Delay(TimeSpan.FromSeconds(3));

            // Скрываем панель
            _notificationContainer.style.display = DisplayStyle.None;
        }
        
        private void OnSaveClicked()
        {
            _saveGameUseCase.Execute().Forget();
        }
    
        private void OnLoadClicked()
        {
            _loadGameUseCase.Execute().Forget();
        }
        
        private void OnUpgradeClicked()
        {
            if (!string.IsNullOrEmpty(_currentSelectedBuildingId))
            {
                _upgradeBuildingUseCase.Execute(_currentSelectedBuildingId).Forget();
            }
        }
        
        public void ShowBuildingInfo(BuildingData buildingData)
        {
            _currentSelectedBuildingId = buildingData.Id;
    
            // Получаем ScriptableObject для доступа к названию и уровням
            BuildingTypeSO buildingType = _buildingService.GetBuildingType(buildingData.Type);
    
            if (buildingType != null)
            {
                _buildingInfoPanel.style.display = DisplayStyle.Flex;
                _buildingTitleLabel.text = buildingType.title;
                _buildingLevelLabel.text = $"Уровень: {buildingData.Level}";

                // Проверяем, есть ли следующий уровень
                if (buildingData.Level < buildingType.levels.Length)
                {
                    BuildingLevelData nextLevelData = buildingType.levels[buildingData.Level];
                    _upgradeCostLabel.text = $"Стоимость улучшения: {nextLevelData.GoldCost} Золота";
                    _upgradeButton.style.display = DisplayStyle.Flex;
                }
                else
                {
                    _upgradeCostLabel.text = "Макс. уровень";
                    _upgradeButton.style.display = DisplayStyle.None;
                }
            }
        }
        
        private void OnDeleteClicked()
        {
            if (!string.IsNullOrEmpty(_currentSelectedBuildingId))
            {
                _deleteBuildingUseCase.Execute(_currentSelectedBuildingId).Forget();
                HideBuildingInfo();
            }
        }
        
        private void OnMoveClicked()
        {
            if (!string.IsNullOrEmpty(_currentSelectedBuildingId))
            {
                // TODO: Реализовать логику перемещения
                // Временно, просто скрываем UI
                HideBuildingInfo();
            }
        }
    }
}