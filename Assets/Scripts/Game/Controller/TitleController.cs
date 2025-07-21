using UnityEngine;
using UnityEngine.UIElements;
using Cysharp.Threading.Tasks;
using VContainer;
using BossSlayingTourney.Network;
using BossSlayingTourney.Core;
using BossSlayingTourney.Game.Model;
using BossSlayingTourney.Game.View;
using R3;

namespace BossSlayingTourney.Game.Controllers
{
    public class TitleController : MonoBehaviour
    {
        private TitleTextData _titleTextData;

        #region Dependencies
        private MainController _mainController;
        private StateController _stateController;
        private TitleModel _model;
        private TitleView _view;
        #endregion

        [Inject]
        public void Construct(MainController mainController, StateController stateController)
        {
            _mainController = mainController;
            _stateController = stateController;
            // Model と View の初期化
            _model = new TitleModel(NetworkManager.Instance);
            _view = new TitleView();
            _titleTextData = new TitleTextData();
        }

        private void Start()
        {
            InitializeController();
        }

        private void Update()
        {
            _model.CheckMatchingProgress();
        }

        private void InitializeController()
        {
            // View の初期化
            _view.Initialize(GetComponent<UIDocument>(), _titleTextData);

            // Model のイベント購読
            SubscribeToModelEvents();

            // View のイベント購読
            SubscribeToViewEvents();

            // 初期状態の設定
            _model.SetPlayerCount(_mainController.PlayerCount);
        }

        private void SubscribeToModelEvents()
        {
            _model.OnMatchingStarted.Subscribe(_ => _view.SetMatchingState());
            _model.OnMatchingStopped.Subscribe(_ => _view.SetPreMatchmakingState());
            _model.OnMatchingCompleted.Subscribe(_ => StartOnlineGame());
            _model.OnMatchingError.Subscribe(error => _view.SetPreMatchmakingState());
            _model.OnPlayerCountChanged.Subscribe(count =>
            {
                _mainController.PlayerCount = count;
                _view.UpdatePlayerCountDisplay(count);
            });
        }

        private void SubscribeToViewEvents()
        {
            _view.OnLocalGameRequested.Subscribe(_ => StartLocalGame());
            _view.OnMatchmakingStartRequested.Subscribe(data =>
                _model.StartMatchmakingAsync(data.useRoomName, data.roomName).Forget());
            _view.OnStopMatchmakingRequested.Subscribe(_ => _model.StopMatchmaking());
            _view.OnPlayerCountChangeRequested.Subscribe(delta =>
            {
                int newCount = _model.PlayerCount + delta;
                _model.SetPlayerCount(newCount);
            });
        }

        private void StartLocalGame()
        {
            _mainController.GameMode = BossSlayingTourney.Core.GameMode.Local;
            _model.StartLocalGameAsync().Forget();
            _stateController.ChangeState(State.Field);
        }

        private void StartOnlineGame()
        {
            _mainController.GameMode = BossSlayingTourney.Core.GameMode.Online;
            _stateController.ChangeState(State.Field);
        }

        private void OnDestroy()
        {
            _model?.Dispose();
            _view?.Dispose();
        }
    }
}