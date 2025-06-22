using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public static NetworkManager Instance { get; private set; }

    [Networked]
    public NetworkDictionary<PlayerRef, int> PlayerIdMap { get; }

    [SerializeField]
    private NetworkRunner networkRunnerPrefab;

    private NetworkRunner _networkRunner;
    private int _nextPlayerId = 1;

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
        var result = await _networkRunner.StartGame(new StartGameArgs
        {
            GameMode = Fusion.GameMode.Shared
        });
    }

    public void SpawnPlayer(GameObject playerPrefab, Vector3 spawnPosition, Transform parentTransform)
    {
        var playerObject = _networkRunner.Spawn(playerPrefab, spawnPosition, Quaternion.identity);
        playerObject.transform.SetParent(parentTransform);
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
                PlayerIdMap.Add(player, _nextPlayerId);
                _nextPlayerId++;
            }
        }
    }

    void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsSharedModeMasterClient)
        {
            PlayerIdMap.Remove(player);
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