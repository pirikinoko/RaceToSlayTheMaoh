using BossSlayingTourney.Core;
using BossSlayingTourney.Network;
using Cysharp.Threading.Tasks;
using Fusion;
using R3;
using UnityEngine;

namespace BossSlayingTourney.Game.Model
{
    public class TitleModel
    {
        #region Events
        public readonly Subject<Unit> OnMatchingStarted = new();
        public readonly Subject<Unit> OnMatchingStopped = new();
        public readonly Subject<Unit> OnMatchingCompleted = new();
        public readonly Subject<string> OnMatchingError = new();
        public readonly Subject<int> OnPlayerCountChanged = new();
        #endregion

        #region Properties
        public bool IsMatching { get; private set; }
        public int PlayerCount { get; private set; } = 2;
        public BossSlayingTourney.Core.GameMode GameMode { get; set; } = BossSlayingTourney.Core.GameMode.Local;
        #endregion

        #region Dependencies
        private readonly NetworkManager _networkManager;
        #endregion

        public TitleModel(NetworkManager networkManager)
        {
            _networkManager = networkManager;
        }

        public void SetPlayerCount(int playerCount)
        {
            if (playerCount < 1) playerCount = 1;
            if (playerCount > Constants.MaxPlayerCount) playerCount = Constants.MaxPlayerCount;

            PlayerCount = playerCount;
            OnPlayerCountChanged.OnNext(playerCount);
        }

        public async UniTask StartLocalGameAsync()
        {
            GameMode = BossSlayingTourney.Core.GameMode.Local;
            await _networkManager.CraeateLocalGameAsync();
        }

        public async UniTask StartMatchmakingAsync(bool useRoomName, string roomName = "")
        {
            if (_networkManager.GetNetworkRunner() != null && _networkManager.GetNetworkRunner().IsRunning)
            {
                return;
            }

            IsMatching = true;
            OnMatchingStarted.OnNext(Unit.Default);

            StartGameResult result = null;

            if (useRoomName && !string.IsNullOrEmpty(roomName))
            {
                result = await _networkManager.JoinOrCreateRoomByNameAsync(roomName);
                Debug.Log($"Room matchmaking result: {result}");
            }
            else
            {
                result = await _networkManager.JoinOrCreateOldestRoomAsync();
                Debug.Log($"Random matchmaking result: {result}");
            }

            if (!result.Ok)
            {
                Debug.LogError($"Failed to start matchmaking: {result.ErrorMessage}");
                IsMatching = false;
                OnMatchingError.OnNext(result.ErrorMessage);
            }
        }

        public void StopMatchmaking()
        {
            IsMatching = false;
            OnMatchingStopped.OnNext(Unit.Default);

            if (_networkManager.GetNetworkRunner() != null && _networkManager.GetNetworkRunner().IsRunning)
            {
                _networkManager.GetNetworkRunner().Disconnect(_networkManager.GetNetworkRunner().LocalPlayer);
            }
        }

        public void CheckMatchingProgress()
        {
            if (!IsMatching || _networkManager.GetNetworkRunner() == null)
                return;

            bool isMatchComplete = _networkManager.GetNetworkRunner().SessionInfo.PlayerCount ==
                                 _networkManager.GetNetworkRunner().SessionInfo.MaxPlayers;

            if (isMatchComplete)
            {
                IsMatching = false;
                GameMode = BossSlayingTourney.Core.GameMode.Online;
                OnMatchingCompleted.OnNext(Unit.Default);
            }
        }

        public void Dispose()
        {
            OnMatchingStarted?.Dispose();
            OnMatchingStopped?.Dispose();
            OnMatchingCompleted?.Dispose();
            OnMatchingError?.Dispose();
            OnPlayerCountChanged?.Dispose();
        }
    }
}
