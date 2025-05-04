using Cysharp.Threading.Tasks;
using R3;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BattleLogController : MonoBehaviour
{
    public Observable<Unit> OnAllLogsRead => _onAllLogsRead;

    private Subject<Unit> _onAllLogsRead = new();

    private Queue<string> _logs = new Queue<string>();
    private Label _label;

    private bool isFlipable;

    public void Initialize(Label logLabel)
    {
        _label = logLabel;
    }

    private void Update()
    {
        if (!isFlipable)
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

        isFlipable = true;
    }

    private void FlipLog()
    {
        _logs.Dequeue();
        if (_logs.Count == 0)
        {
            isFlipable = false;
            _onAllLogsRead.OnNext(Unit.Default);
            return;
        }
        _label.text = _logs.Peek();
    }

    public void ClearLogs()
    {
        _logs.Clear();
        _label.text = string.Empty;
        isFlipable = false;
    }
}
