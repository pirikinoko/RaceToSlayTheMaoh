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

    public class RewardParameter
    {
        public static int HpGain = 10;
        public static int MpGain = 5;
        public static int PowerGain = 1;
    }

    private void SetRandomReword()
    {
        // 報酬の候補となるアクションと説明をリストにまとめる
        List<(StatusReword action, string description)> rewordActions = new List<(StatusReword, string)>
        {
            (IncreaseHealth, $"HPが{RewardParameter.HpGain}ポイント増加する"),
            (IncreaseMana, $"MPが{RewardParameter.MpGain}ポイント増加する"),
            (IncreasePower, $"攻撃力が{RewardParameter.PowerGain}ポイント増加する")
        };

        // ランダムにインデックスを選択
        int index = Random.Range(0, rewordActions.Count);

        // 選択した報酬をセット
        StatusRewordAction = rewordActions[index].action;
        Description = rewordActions[index].description;
    }

    private string IncreaseHealth(Entity target)
    {
        int newHp = target.Parameter.HitPoint + RewardParameter.HpGain;
        target.SetHitPoint(newHp);
        return $"{target.name}のHPが{RewardParameter.HpGain}ポイント増加した！";
    }

    private string IncreaseMana(Entity target)
    {
        int newMp = target.Parameter.ManaPoint + RewardParameter.MpGain;
        target.SetManaPoint(newMp);
        return $"{target.name}のMPが{RewardParameter.MpGain}ポイント増加した！";
    }

    private string IncreasePower(Entity target)
    {
        target.Parameter.Power += RewardParameter.PowerGain;
        return $"{target.name}の攻撃力が{RewardParameter.PowerGain}ポイント増加した！";
    }

    public string Execute(Entity target)
    {
        return StatusRewordAction?.Invoke(target);
    }
}
