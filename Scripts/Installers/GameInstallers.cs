using Applications;
using Applications.Buildings;
using Applications.SaveLoad;
using Applications.Services;
using MessagePipe;
using Presentation.Controllers;
using Presentation.Grid;
using Presentation.UI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Installers
{
    public class GameInstallers : LifetimeScope
    {
        [SerializeField] private GridView _gridView;
        [SerializeField] private GridCursor _gridCursor;
        [SerializeField] private BuildingPlacementController _placementController;
        [SerializeField] private UIController _uiController;
        
        protected override void Configure(IContainerBuilder builder)
        {
            // Регистрация MessagePipe
            builder.RegisterMessagePipe();
        
            // Регистрация Use Cases и Services (логика игры)
            builder.Register<GridService>(Lifetime.Singleton);
            builder.Register<BuildingService>(Lifetime.Singleton);
            builder.Register<EconomyService>(Lifetime.Singleton);
        
            // Use Cases
            builder.Register<BuildBuildingUseCase>(Lifetime.Singleton);
            builder.Register<SaveGameUseCase>(Lifetime.Singleton);
            builder.Register<LoadGameUseCase>(Lifetime.Singleton);
            builder.Register<DeleteBuildingUseCase>(Lifetime.Singleton);
            builder.Register<MoveBuildingUseCase>(Lifetime.Singleton);
            builder.Register<UpgradeBuildingUseCase>(Lifetime.Singleton);
        
            // Регистрация компонентов на сцене (View)
            builder.RegisterComponent(_gridView);
            builder.RegisterComponent(_gridCursor);
            builder.RegisterComponent(_placementController);
            builder.RegisterComponent(_uiController);
        }
    }
}