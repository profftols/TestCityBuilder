using System;
using System.Threading;
using Applications.Buildings;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UniRx;
using VContainer;

namespace Applications.Services
{
    public class EconomyService : IDisposable
    {
        private readonly ReactiveProperty<int> _gold = new ReactiveProperty<int>(100);
        private readonly IAsyncPublisher<EconomyStateChanged> _economyPublisher;
        private readonly BuildingService _buildingService;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public IReadOnlyReactiveProperty<int> Gold => _gold;

        [Inject]
        public EconomyService(IAsyncPublisher<EconomyStateChanged> economyPublisher, BuildingService buildingService)
        {
            _economyPublisher = economyPublisher;
            _buildingService = buildingService;

            StartIncomeLoop().Forget();
        }

        private async UniTaskVoid StartIncomeLoop()
        {
            try
            {
                while (!_cts.IsCancellationRequested)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(5), cancellationToken: _cts.Token);

                    int income = _buildingService.CalculateTotalIncome();
                    if (income > 0)
                    {
                        TryChangeGold(income);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Ожидаемый выход при отмене
            }
        }

        public bool TryChangeGold(int amount)
        {
            if (_gold.Value + amount < 0)
            {
                return false;
            }

            _gold.Value += amount;
            _economyPublisher.PublishAsync(new EconomyStateChanged(_gold.Value)).Forget();
            return true;
        }

        public void Dispose()
        {
            _cts.Cancel();
        }
        
        public void SetGold(int amount)
        {
            _gold.Value = amount;
            _economyPublisher.PublishAsync(new EconomyStateChanged(_gold.Value)).Forget();
        }
    }

}
