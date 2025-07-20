using System;
using System.Collections.Generic;
using BossSlayingTourney.Core;
using Cysharp.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

namespace BossSlayingTourney.Network
{

    public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
    {
        public static NetworkManager Instance { get; private set; }

        [Networked]
        public NetworkDictionary<PlayerRef, int> PlayerIdMap { get; set; }

        [Networked]
        public NetworkDictionary<PlayerRef, NetworkString<_32>> PlayerNameMap { get; set; }

        [Networked, Capacity(50)]
        public NetworkArray<NetworkString<_128>> ChatHistory { get; }

        [Networked]
        public int ChatHistoryCount { get; set; }

        // プレイヤー情報変更イベント
        public event Action<List<string>> OnPlayerListChanged;

        // チャット履歴更新イベント
        public event Action<List<string>> OnChatHistoryUpdated;

        [SerializeField]
        private NetworkRunner networkRunnerPrefab;

        private NetworkRunner _networkRunner;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private async void Start()
        {
            _networkRunner = Instantiate(networkRunnerPrefab);
            _networkRunner.AddCallbacks(this);
        }

        public async UniTask<StartGameResult> CraeateLocalGameAsync()
        {
            var result = await _networkRunner.StartGame(new StartGameArgs
            {
                GameMode = Fusion.GameMode.Single,
                SceneManager = _networkRunner.SceneManager,
                PlayerCount = 1,
                IsVisible = false,
                IsOpen = false,
            });
            return result;
        }

        public async UniTask<StartGameResult> JoinOrCreateOldestRoomAsync()
        {
            var result = await _networkRunner.StartGame(new StartGameArgs
            {
                GameMode = Fusion.GameMode.Shared,
                SceneManager = _networkRunner.SceneManager,
                PlayerCount = Constants.MaxPlayerCount,
                IsVisible = true,
                IsOpen = true,
                MatchmakingMode = Fusion.Photon.Realtime.MatchmakingMode.FillRoom
            });
            return result;
        }


        public async UniTask<StartGameResult> JoinOrCreateRoomByNameAsync(string roomName)
        {
            var result = await _networkRunner.StartGame(new StartGameArgs
            {
                GameMode = Fusion.GameMode.Shared,
                SessionName = roomName,
                SceneManager = _networkRunner.SceneManager,
                PlayerCount = Constants.MaxPlayerCount,
                IsVisible = false,
                IsOpen = true,
                MatchmakingMode = Fusion.Photon.Realtime.MatchmakingMode.FillRoom
            });
            return result;
        }

        /// <summary>
        /// ルームに参加可能になる
        /// </summary>
        public void EnableJoin()
        {
            _networkRunner.SessionInfo.IsOpen = true;
        }

        /// <summary>
        /// ルームに参加不可能になる
        /// </summary>
        public void DisableJoin()
        {
            _networkRunner.SessionInfo.IsOpen = false;
        }

        public NetworkObject SpawnPlayer(GameObject playerPrefab, Vector3 spawnPosition, Transform parentTransform)
        {
            var playerObject = _networkRunner.Spawn(playerPrefab, spawnPosition, Quaternion.identity);
            playerObject.transform.SetParent(parentTransform);
            return playerObject;
        }

        public NetworkRunner GetNetworkRunner()
        {
            return _networkRunner;
        }

        public int GetPlayerCount()
        {
            return _networkRunner.SessionInfo.PlayerCount;
        }

        public int GetPlayerId(PlayerRef playerRef)
        {
            if (PlayerIdMap.ContainsKey(playerRef))
            {
                return PlayerIdMap[playerRef];
            }
            return -1;
        }

        public string GetPlayerName(PlayerRef playerRef)
        {
            if (PlayerNameMap.ContainsKey(playerRef))
            {
                return PlayerNameMap[playerRef].ToString();
            }
            return $"Player {GetPlayerId(playerRef)}"; // フォールバック
        }

        private void SetPlayerName(PlayerRef playerRef, string playerName)
        {
            // マスタークライアントのみが実行可能
            if (!_networkRunner.IsSharedModeMasterClient)
                return;

            if (PlayerNameMap.ContainsKey(playerRef))
            {
                PlayerNameMap.Set(playerRef, playerName);
            }
            else
            {
                PlayerNameMap.Add(playerRef, playerName);
            }
            NotifyPlayerListChanged();
        }

        public void AddChatMessage(string message)
        {
            // マスタークライアントのみが実行可能
            if (!_networkRunner.IsSharedModeMasterClient)
                return;

            if (ChatHistoryCount < ChatHistory.Length)
            {
                // 配列に空きがある場合は末尾に追加
                ChatHistory.Set(ChatHistoryCount, message);
                ChatHistoryCount++;
            }
            else
            {
                // 配列が満杯の場合は古いメッセージを削除して新しいメッセージを追加
                ShiftChatHistoryAndAdd(message);
            }
            NotifyChatHistoryUpdated();
        }

        private void ShiftChatHistoryAndAdd(string newMessage)
        {
            // 古いメッセージを1つずつ前にシフト（最古のメッセージ[0]が削除される）
            for (int i = 0; i < ChatHistory.Length - 1; i++)
            {
                ChatHistory.Set(i, ChatHistory[i + 1]);
            }

            // 最後の位置に新しいメッセージを追加
            ChatHistory.Set(ChatHistory.Length - 1, newMessage);
            // カウントは最大値を維持
            ChatHistoryCount = ChatHistory.Length;
        }

        public List<string> GetChatHistory()
        {
            var history = new List<string>();
            for (int i = 0; i < ChatHistoryCount; i++)
            {
                history.Add(ChatHistory[i].ToString());
            }
            return history;
        }

        private void NotifyChatHistoryUpdated()
        {
            OnChatHistoryUpdated?.Invoke(GetChatHistory());
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RpcRequestSetPlayerName(PlayerRef playerRef, string playerName)
        {
            SetPlayerName(playerRef, playerName);
        }

        void INetworkRunnerCallbacks.OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        void INetworkRunnerCallbacks.OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        void INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            // マスタークライアントがIDと名前の割り当てを行う
            if (runner.IsSharedModeMasterClient)
            {
                if (!PlayerIdMap.ContainsKey(player))
                {
                    var playerId = GetNetworkRunner().SessionInfo.PlayerCount;
                    PlayerIdMap.Add(player, playerId);

                    // デフォルト名を設定
                    var defaultName = $"Player {playerId}";
                    PlayerNameMap.Add(player, defaultName);
                }
            }

            // プレイヤーリスト更新イベントを発火
            NotifyPlayerListChanged();

            // 新規参加プレイヤーにチャット履歴を送信
            if (runner.IsSharedModeMasterClient)
            {
                SendChatHistoryToPlayerRPC(player);
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void SendChatHistoryToPlayerRPC(PlayerRef targetPlayer)
        {
            // 指定されたプレイヤーのみがチャット履歴を受信
            if (targetPlayer == _networkRunner.LocalPlayer)
            {
                NotifyChatHistoryUpdated();
            }
        }

        void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (runner.IsSharedModeMasterClient)
            {
                PlayerIdMap.Remove(player);
                PlayerNameMap.Remove(player);

                // IDを再割り当て
                int i = 1;
                var playerRefs = new List<PlayerRef>();
                foreach (var kvp in PlayerIdMap)
                {
                    playerRefs.Add(kvp.Key);
                }

                foreach (var playerRef in playerRefs)
                {
                    PlayerIdMap.Set(playerRef, i);
                    i++;
                }
            }

            // プレイヤーリスト更新イベントを発火
            NotifyPlayerListChanged();
        }

        private void NotifyPlayerListChanged()
        {
            var playerNames = new List<string>();
            foreach (var kvp in PlayerNameMap)
            {
                playerNames.Add(kvp.Value.ToString());
            }
            OnPlayerListChanged?.Invoke(playerNames);
        }

        public List<string> GetCurrentPlayerNames()
        {
            var playerNames = new List<string>();
            foreach (var kvp in PlayerNameMap)
            {
                playerNames.Add(kvp.Value.ToString());
            }
            return playerNames;
        }

        void INetworkRunnerCallbacks.OnInput(NetworkRunner runner, NetworkInput input) { }
        void INetworkRunnerCallbacks.OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        void INetworkRunnerCallbacks.OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
        void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner) { }
        void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        void INetworkRunnerCallbacks.OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        void INetworkRunnerCallbacks.OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        void INetworkRunnerCallbacks.OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        void INetworkRunnerCallbacks.OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        void INetworkRunnerCallbacks.OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        void INetworkRunnerCallbacks.OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        void INetworkRunnerCallbacks.OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
        void INetworkRunnerCallbacks.OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        void INetworkRunnerCallbacks.OnSceneLoadDone(NetworkRunner runner) { }
        void INetworkRunnerCallbacks.OnSceneLoadStart(NetworkRunner runner) { }
    }
}