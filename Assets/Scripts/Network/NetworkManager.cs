using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public static NetworkManager Instance { get; private set; }

    [Networked]
    public NetworkDictionary<PlayerRef, int> PlayerIdMap { get; set; }

    [SerializeField]
    private NetworkRunner networkRunnerPrefab;

    [SerializeField]
    private UserController _userController;

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

    public async UniTask CraeateLocalGameAsync()
    {
        await _networkRunner.StartGame(new StartGameArgs
        {
            GameMode = Fusion.GameMode.Single,
            SceneManager = _networkRunner.SceneManager,
            PlayerCount = 1,
            IsVisible = false,
            IsOpen = false,
        });
    }

    public async UniTask JoinOrCreateOldestRoomAsync()
    {
        await _networkRunner.StartGame(new StartGameArgs
        {
            GameMode = Fusion.GameMode.Shared,
            SceneManager = _networkRunner.SceneManager,
            PlayerCount = Constants.MaxPlayerCount,
            IsVisible = true,
            IsOpen = true,
            MatchmakingMode = Fusion.Photon.Realtime.MatchmakingMode.FillRoom
        });
    }


    public async UniTask JoinOrCraateRoomByNameAsync(string roomName)
    {
        await _networkRunner.StartGame(new StartGameArgs
        {
            GameMode = Fusion.GameMode.Shared,
            SessionName = roomName,
            SceneManager = _networkRunner.SceneManager,
            PlayerCount = Constants.MaxPlayerCount,
            IsVisible = false,
            IsOpen = true,
            MatchmakingMode = Fusion.Photon.Realtime.MatchmakingMode.FillRoom
        });
    }

    public void StartGame()
    {
        _networkRunner.SessionInfo.IsOpen = false;
        _networkRunner.SessionInfo.IsVisible = false;
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

    void INetworkRunnerCallbacks.OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    void INetworkRunnerCallbacks.OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    void INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        // マスタークライアントがIDの割り当てを行う
        if (runner.IsSharedModeMasterClient)
        {
            if (!PlayerIdMap.ContainsKey(player))
            {
                PlayerIdMap.Add(player, GetNetworkRunner().SessionInfo.PlayerCount);
            }
        }
    }

    void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsSharedModeMasterClient)
        {
            PlayerIdMap.Remove(player);
            int i = 1;
            foreach (var kvp in PlayerIdMap)
            {
                PlayerIdMap.Set(kvp.Key, i++);
            }
        }
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