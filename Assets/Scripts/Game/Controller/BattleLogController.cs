using Cysharp.Threading.Tasks;
using Fusion;
using R3;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace BossSlayingTourney.Game.Controllers
{

public class BattleLogController : NetworkBehaviour
{
    public Observable<Unit> OnAllLogsRead => _onAllLogsRead;

    private Subject<Unit> _onAllLogsRead = new();

    private Queue<string> _logs = new Queue<string>();
    private Label _label;

    // ネットワークスポーン状態を追跡
    private bool _isNetworkSpawned = false;

    // スポーン前にキューイングされた操作
    private Queue<System.Action> _pendingOperations = new Queue<System.Action>();

    [Networked]
    private bool _isFlipable { get; set; }

    [Networked, OnChangedRender(nameof(UpdateLabel))]
    private NetworkString<_64> SyncedLog { get; set; }

    // Fusionライフサイクル
    public override void Spawned()
    {
        _isNetworkSpawned = true;

        // スポーン前にキューイングされた操作を実行
        while (_pendingOperations.Count > 0)
        {
            var operation = _pendingOperations.Dequeue();
            operation?.Invoke();
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        _isNetworkSpawned = false;
        _pendingOperations.Clear();
    }

    private void UpdateLabel()
    {
        if (_label != null)
        {
            _label.text = SyncedLog.Value;
        }
    }

    public void Initialize(Label logLabel)
    {
        _label = logLabel;
    }

    private void Update()
    {
        if (!_isNetworkSpawned || !_isFlipable)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Rpc_RequestFlipLog();
        }
    }

    public void SetText(string text)
    {
        if (_isNetworkSpawned)
        {
            SyncedLog = text;
        }
        else
        {
            // スポーン前の場合は操作をキューイング
            _pendingOperations.Enqueue(() => SyncedLog = text);
        }
    }

    public void AddLog(string log)
    {
        _logs.Enqueue(log);

        if (_isNetworkSpawned)
        {
            SyncedLog = _logs.Peek();
        }
        else
        {
            // スポーン前の場合は操作をキューイング
            _pendingOperations.Enqueue(() => SyncedLog = _logs.Peek());
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void Rpc_RequestFlipLog()
    {
        FlipLog();
    }

    public void FlipLog()
    {
        if (!_isNetworkSpawned)
        {
            _pendingOperations.Enqueue(() => FlipLog());
            return;
        }

        _logs.Dequeue();
        if (_logs.Count == 0)
        {
            _isFlipable = false;
            _onAllLogsRead.OnNext(Unit.Default);
            return;
        }
        SyncedLog = _logs.Peek();
    }

    public void ClearLogs()
    {
        _logs.Clear();

        if (_isNetworkSpawned)
        {
            SyncedLog = string.Empty;
            _isFlipable = false;
        }
        else
        {
            _pendingOperations.Enqueue(() =>
            {
                SyncedLog = string.Empty;
                _isFlipable = false;
            });
        }
    }

    public void EnableFlip()
    {
        if (_isNetworkSpawned)
        {
            _isFlipable = true;
        }
        else
        {
            _pendingOperations.Enqueue(() => _isFlipable = true);
        }
    }
}
}