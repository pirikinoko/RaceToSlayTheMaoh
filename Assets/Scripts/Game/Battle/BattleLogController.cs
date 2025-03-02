using R3;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BattleLogController : MonoBehaviour
{
    public bool IsFliipable = true;
    public bool HasAllLogsRead => _logs.Count == 0;

    public Observable<Unit> OnAllLogsRead => _onAllLogsRead;

    private Subject<Unit> _onAllLogsRead = new();

    private Queue<string> _logs = new Queue<string>();
    private Label _label;


    public void Initialize(Label logLabel)
    {
        _label = logLabel;
    }

    private void Update()
    {
        if (!IsFliipable)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            FlipLog();
        }
    }

    public void SetText(string text)
    {
        _label.text = text;
    }

    public void AddLog(string log)
    {
        _logs.Enqueue(log);
        _label.text = _logs.Peek();
    }

    public void ClearLogs()
    {
        _logs.Clear();
    }

    private void FlipLog()
    {
        if (_logs.Count == 0)
        {
            _onAllLogsRead.OnNext(Unit.Default);
            return;
        }
        _label.text = _logs.Peek();
        _logs.Dequeue();
    }
}
