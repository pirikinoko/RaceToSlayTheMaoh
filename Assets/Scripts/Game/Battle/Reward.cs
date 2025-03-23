using System.Collections.Generic;
using UnityEngine;

public class Reward
{
    public string Description { get; private set; }

    public delegate string StatusReword(Entity target);

    public StatusReword StatusRewordAction { get; private set; }

    public Reward()
    {
        SetRandomReword();
    }

    private void SetRandomReword()
    {
        // 報酬の候補となるアクションと説明をリストにまとめる
        List<(StatusReword action, string description)> rewordActions = new List<(StatusReword, string)>
        {
            (IncreaseHealth, "HPが10ポイント増加する"),
            (IncreaseMana, "MPが10ポイント増加する"),
            (IncreasePower, "攻撃力が2ポイント増加する")
        };

        // ランダムにインデックスを選択
        int index = Random.Range(0, rewordActions.Count);

        // 選択した報酬をセット
        StatusRewordAction = rewordActions[index].action;
        Description = rewordActions[index].description;
    }

    private string IncreaseHealth(Entity target)
    {
        target.Parameter.HitPoint += 10;
        return $"{target.name}のHPが10ポイント増加した！";
    }

    private string IncreaseMana(Entity target)
    {
        target.Parameter.ManaPoint += 10;
        return $"{target.name}のMPが10ポイント増加した！";
    }

    private string IncreasePower(Entity target)
    {
        target.Parameter.Power += 1;
        return $"{target.name}の攻撃力が2ポイント増加した！";
    }

    public string Execute(Entity target)
    {
        return StatusRewordAction?.Invoke(target);
    }
}
