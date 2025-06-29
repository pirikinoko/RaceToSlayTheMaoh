using Cysharp.Threading.Tasks;
using Fusion;
using R3;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BattleLogController : NetworkBehaviour
{
    public Observable<Unit> OnAllLogsRead => _onAllLogsRead;

    private Subject<Unit> _onAllLogsRead = new();

    private Queue<string> _logs = new Queue<string>();
    private Label _label;

    [Networked]
    private bool _isFlipable { get; set; }

    [Networked, OnChangedRender(nameof(UpdateLabel))]
    private NetworkString<_64> SyncedLog { get; set; }

    private void UpdateLabel()
    {
        _label.text = SyncedLog.Value;
    }

    public void Initialize(Label logLabel)
    {
        _label = logLabel;
    }

    private void Update()
    {
        if (!_isFlipable)
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
        SyncedLog = text;
    }

    public void AddLog(string log)
    {
        _logs.Enqueue(log);
        SyncedLog = _logs.Peek();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void Rpc_RequestFlipLog()
    {
        FlipLog();
    }

    public void FlipLog()
    {
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
        SyncedLog = string.Empty;
        _isFlipable = false;
    }

    public void EnableFlip()
    {
        _isFlipable = true;
    }
}
