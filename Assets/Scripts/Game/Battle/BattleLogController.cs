using Cysharp.Threading.Tasks;
using R3;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BattleLogController : MonoBehaviour
{
    public bool HasAllLogsRead => _logs.Count == 0;

    public Observable<Unit> OnAllLogsRead => _onAllLogsRead;

    private Subject<Unit> _onAllLogsRead = new();

    private Queue<string> _logs = new Queue<string>();
    private Label _label;

    private bool isFlipable;
    private int _waitTimeToBeFilippableMills = 500;

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

    public async UniTask AddLogAsync(string log)
    {
        _logs.Enqueue(log);
        _label.text = _logs.Peek();

        // ログが追加されてもすぐにはフリップできないようにする
        await UniTask.Delay(_waitTimeToBeFilippableMills, cancellationToken: this.GetCancellationTokenOnDestroy());
        isFlipable = true;
    }

    private void FlipLog()
    {
        if (_logs.Count == 0)
        {
            isFlipable = false;
            _onAllLogsRead.OnNext(Unit.Default);
            return;
        }
        _label.text = _logs.Peek();
        _logs.Dequeue();
    }
}
