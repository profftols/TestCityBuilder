using System;
using System.Collections.Generic;
using Applications;
using Applications.Buildings;
using Applications.DTO;
using Cysharp.Threading.Tasks;
using Domain.Data;
using MessagePipe;
using Presentation.Buildings;
using Presentation.Grid;
using Presentation.UI;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using BuildType = Applications.BuildType;

namespace Presentation.Controllers
{
    public class BuildingPlacementController : MonoBehaviour
    {
        [SerializeField] private GridCursor _gridCursor;

        [SerializeField] private BuildingTypeSO houseType;
        [SerializeField] private BuildingTypeSO farmType;
        [SerializeField] private BuildingTypeSO mineType;

        private bool _isDeleteMode;
        private string _selectedBuildingId;
        
        private Dictionary<string, GameObject> _buildingObjects = new();
        private BuildBuildingUseCase _buildBuildingUseCase;
        private DeleteBuildingUseCase _deleteBuildingUseCase;
        private BuildingTypeSO _selectedBuildingType;
        private IAsyncSubscriber<BuildingBuilt> _buildingBuiltSubscriber;
        private IAsyncSubscriber<NotificationEvent> _notificationSubscriber;
        private IAsyncSubscriber<BuildingDeleted> _buildingDeletedSubscriber;
        private UpgradeBuildingUseCase _upgradeBuildingUseCase;
        private IAsyncSubscriber<BuildingUpgraded> _buildingUpgradedSubscriber;
        private UIController _uiController;
        private BuildingService _buildingService;

        private IDisposable _buildingBuiltDisposable;
        private IDisposable _notificationDisposable;

        [Inject]
        public void Construct(BuildBuildingUseCase buildBuildingUseCase,
            IAsyncSubscriber<BuildingBuilt> buildingBuiltSubscriber,
            IAsyncSubscriber<NotificationEvent> notificationSubscriber,
            DeleteBuildingUseCase deleteBuildingUseCase,
            IAsyncSubscriber<BuildingDeleted> buildingDeletedSubscriber,
            UpgradeBuildingUseCase upgradeBuildingUseCase,
            IAsyncSubscriber<BuildingUpgraded> buildingUpgradedSubscriber,
            UIController uiController,
            BuildingService buildingService)
        {
            _buildBuildingUseCase = buildBuildingUseCase;
            _buildingBuiltSubscriber = buildingBuiltSubscriber;
            _notificationSubscriber = notificationSubscriber;
            _deleteBuildingUseCase = deleteBuildingUseCase;
            _buildingDeletedSubscriber = buildingDeletedSubscriber;
            _upgradeBuildingUseCase = upgradeBuildingUseCase;
            _buildingUpgradedSubscriber = buildingUpgradedSubscriber;
            _uiController = uiController;
            _buildingService = buildingService;
        }

        private void Start()
        {
            _buildingBuiltDisposable = _buildingBuiltSubscriber.Subscribe(
                (buildingBuilt, cancellationToken) => OnBuildingBuilt(buildingBuilt));

            _notificationDisposable = _notificationSubscriber.Subscribe(
                (notification, cancellationToken) => OnNotificationReceived(notification));
            
            _buildingDeletedSubscriber.Subscribe(async (deleted, token) => OnBuildingDeleted(deleted)).AddTo(this);
            _buildingUpgradedSubscriber.Subscribe(async (upgraded, token) => OnBuildingUpgraded(upgraded)).AddTo(this);
        }

        private void OnDestroy()
        {
            _buildingBuiltDisposable?.Dispose();
            _notificationDisposable?.Dispose();
        }
        
        private async UniTask OnBuildingBuilt(BuildingBuilt buildingBuilt)
        {
            BuildingTypeSO buildingType = houseType; // TODO: заменить на универсальный поиск по `buildingBuilt.BuildingData.Type`
    
            if (buildingType != null)
            {
                GameObject buildingObject = Instantiate(buildingType.prefab, new Vector3(buildingBuilt.BuildingData.Position.x, 0, buildingBuilt.BuildingData.Position.y), Quaternion.identity);
                var buildingView = buildingObject.AddComponent<BuildingView>();
                buildingView.Initialize(buildingBuilt.BuildingData.Id);
    
                _buildingObjects[buildingBuilt.BuildingData.Id] = buildingObject;
            }
            
            
    
            Debug.Log($"Здание типа {buildingBuilt.BuildingData.Type} построено на {buildingBuilt.BuildingData.Position}.");
        }

        private async UniTask OnNotificationReceived(NotificationEvent notification)
        {
            Debug.Log($"Уведомление: {notification.Message}");
        }

        private void Update()
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                SelectBuilding(houseType);
            }
            
            if (Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                SelectBuilding(farmType);
            }
            
            if (Keyboard.current.digit3Key.wasPressedThisFrame)
            {
                SelectBuilding(mineType);
            }
            
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                ToggleDeleteMode();
            }
        
            if (_isDeleteMode && Mouse.current.leftButton.wasPressedThisFrame)
            {
                HandleDeleteClick();
            }
            
            if (Mouse.current.leftButton.wasPressedThisFrame && _selectedBuildingType == null)
            {
                HandleBuildingSelectionClick();
            }

            if (_selectedBuildingType != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                PlaceBuilding();
            }
            
            if (_selectedBuildingType == null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                HandleBuildingSelectionClick();
            }
        }
        
        private void HandleBuildingSelectionClick()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                var buildingView = hit.collider.GetComponent<BuildingView>();
                if (buildingView != null)
                {
                    var buildingData = _buildingService.GetBuildingById(buildingView.BuildingId);
                    
                    if (buildingData != null)
                    {
                        _uiController.ShowBuildingInfo(buildingData);
                    }
                }
            }
        }
        
        private async UniTask OnBuildingUpgraded(BuildingUpgraded upgraded)
        {
            // TODO: Обновить 3D-модель, если она меняется при улучшении
            Debug.Log($"Здание с ID {upgraded.Id} улучшено до уровня {upgraded.NewLevel}.");
        }
        
        private void SelectBuilding(BuildingTypeSO buildingType)
        {
            _selectedBuildingType = buildingType;
            _gridCursor.UpdateCursor(_selectedBuildingType);
            Debug.Log($"Выбрано здание: {_selectedBuildingType.displayName}");
        }

        private void DeselectBuilding()
        {
            _selectedBuildingType = null;
            _gridCursor.HideCursor();
    
            _selectedBuildingId = null;
            _uiController.HideBuildingInfo();
        }

        private async UniTask PlaceBuilding()
        {
            if (_selectedBuildingType == null) return;

            var request = new BuildBuildingRequest
            {
                BuildingTypeId = (BuildType)_selectedBuildingType.buildTypeId,
                Position = _gridCursor.CurrentPosition
            };

            await _buildBuildingUseCase.Execute(request);

            DeselectBuilding();
        }

        private void ToggleDeleteMode()
        {
            _isDeleteMode = !_isDeleteMode;
            if (_isDeleteMode)
            {
                DeselectBuilding();
                _gridCursor.HideCursor(); 
                Debug.Log("Режим удаления активирован. Выберите здание для удаления.");
            }
            else
            {
                Debug.Log("Режим удаления деактивирован.");
            }
        }
        
        private void HandleDeleteClick()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                var buildingView = hit.collider.GetComponent<BuildingView>();
                if (buildingView != null)
                {
                    _deleteBuildingUseCase.Execute(buildingView.BuildingId).Forget();
                    _isDeleteMode = false;
                }
            }
        }
        
        private async UniTask OnBuildingDeleted(BuildingDeleted buildingDeleted)
        {
            if (_buildingObjects.TryGetValue(buildingDeleted.Id, out GameObject buildingObject))
            {
                Destroy(buildingObject);
                _buildingObjects.Remove(buildingDeleted.Id);
            }
        }
    }

}
