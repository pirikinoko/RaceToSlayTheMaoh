using UnityEngine;

public class Constants
{
    // ******* AssetReference *******   

    public static string AssetReferenceParameter = "Parameter";
    public static string AssetReferencePlayer = "Player";
    public static string AssetReferenceEnemy = "Enemy";
    public static string AssetReferencePlayerIcon = "PlayerIcon";
    public static string AssetReferenceHeartIcon = "HeartIcon";
    public static string AssetReferenceManaIcon = "ManaIcon";
    public static string AssetReferencePowerIcon = "PowerIcon";
    public static string AssetReferenceDamageNumberEffect = "DamageNumberEffect";

    // ******* General *******
    // 指定されたパーセンテージのオフセット内でランダムな値を返す
    // 例: baseValue = 100, offsetPercent = 20 の場合, 80から120の間でランダムな値を返す
    public static int GetRandomizedValueWithinOffset(int baseValue, int offsetPercent)
    {
        var offsetValue = (int)(baseValue * offsetPercent / 100);
        // minが1未満の場合は1を返す
        var min = ((baseValue - offsetValue) < 1) ? 1 : baseValue - offsetValue;
        // +1は上限値を含めるため
        var max = baseValue + offsetValue + 1;
        var randomValue = Random.Range(min, max);
        // 最低でも1を返す
        return randomValue >= 1 ? randomValue : 1;
    }

    // ******* Player *******

    public static float PlayerMoveSpeed = 2.0f;

    // ******* Entity *******

    public static int MaxHitPoint = 50;
    public static int MaxManaPoint = 30;
    public static int AttackOffsetPercent = 50;

    // ******* Field *******

    public static Vector3 FieldCornerDownLeft = new Vector3(-7, -7, 0);
    public static Vector3 FieldCornerUpRight = new Vector3(8, 8, 0);
    public static Vector3 PlayerSpownPosition = new Vector3(-7, -7, 0);

    // ******* Battle *******
    public static int MaxTurn = 20;

    public static string GetSentenceWhenStartBattle(string language, string leftEntityname, string rightEntityname)
    {
        switch (language)
        {
            case "Japanese":
                return string.Format("{0}と{1}の熱い戦いが始まる!", leftEntityname, rightEntityname);
            case "English":
                return string.Format("{0} and {1} have been faced!", leftEntityname, rightEntityname);
            default:
                return string.Format("{0} and {1} have been faced!", leftEntityname, rightEntityname);
        }
    }

    public static string GetSentenceWhileWaitingAction(string language, string entityName)
    {
        switch (language)
        {
            case "Japanese":
                return string.Format("{0}の動きを待っている...", entityName);
            case "English":
                return string.Format("{0} should take an action...", entityName);
            default:
                return string.Format("{0} should take an action...", entityName);
        }
    }

    public static string GetAttackSentence(Language language, string attacker)
    {
        switch (language)
        {
            case Language.Japanese:
                return string.Format("{0}の攻撃!", attacker);
            case Language.English:
                return string.Format("{0} is Attacking!", attacker);
            default:
                return string.Format("{0} is Attacking!", attacker);
        }
    }

    public static string GetAttackResultSentence(Language language, string damageReciever, int damage)
    {
        switch (language)
        {
            case Language.Japanese:
                return string.Format("{0}に{1}のダメージが入った!", damageReciever, damage);
            case Language.English:
                return string.Format("{0} has taken {1} damage!", damageReciever, damage);
            default:
                return string.Format("{0} has taken {1} damage!", damageReciever, damage);
        }
    }

    public static string GetSkillSentence(Language language, string entityName, string skillName)
    {
        switch (language)
        {
            case Language.Japanese:
                return string.Format("{0}の{1}が炸裂!", entityName, skillName);
            case Language.English:
                return string.Format("{0} is using {1}", entityName, skillName);
            default:
                return string.Format("{0} is using {1}", entityName, skillName);
        }
    }

    public static string GetResultSentence(Language language, string winner, string loser)
    {
        switch (language)
        {
            case Language.Japanese:
                return string.Format("{0}が{1}を撃破!", winner, loser);
            case Language.English:
                return string.Format("{0} beat {1}!", winner, loser);
            default:
                return string.Format("{0} beat {1}!", winner, loser);
        }
    }

    public static string GetSentenceWhenBothDied(Language language)
    {
        switch (language)
        {
            case Language.Japanese:
                return "相討ちだ!";
            case Language.English:
                return "Both died!";
            default:
                return "Both died!";
        }
    }

    public static string GetSentenceWhenTurnOver(Language language)
    {
        switch (language)
        {
            case Language.Japanese:
                return "日が暮れてしまったようだ.....";
            case Language.English:
                return "This battle seems to be endless.....";
            default:
                return "This battle seems to be endless.....";
        }
    }

    public static string GetSentenceWhenSelectingReward(Language language)
    {
        switch (language)
        {
            case Language.Japanese:
                return "報酬を選択してください";
            case Language.English:
                return "Please select a reward";
            default:
                return "Please select a reward";
        }
    }

    public static string GetSentenceWhenAlreadyHoldingTheSkill(Language language)
    {
        switch (language)
        {
            case Language.Japanese:
                return "既に習得済みです";
            case Language.English:
                return "You have already learned this skill";
            default:
                return "You have already learned this skill";
        }
    }

    public static string GetSkillGetSentence(Language language)
    {
        switch (language)
        {
            case Language.Japanese:
                return "{0}が{1}を習得した!";
            case Language.English:
                return "{0} has learned {1}!";
            default:
                return "{0} has learned {1}!";
        }
    }

    public static string GetHitPointSentence(Language language)
    {
        switch (language)
        {
            case Language.Japanese:
                return "{0}のHPが{1}回復した!";
            case Language.English:
                return "{0} has healed {1}HP!";
            default:
                return "{0} has healed {1}HP!";
        }
    }

    public static string GetManaSentence(Language language)
    {
        switch (language)
        {
            case Language.Japanese:
                return "{0}のMPが{1}回復した!";
            case Language.English:
                return "{0} has refreshed {1}MagicPoint!";
            default:
                return "{0} has refreshed {1}MagicPoint!";
        }
    }

    public static string GetPowerSentence(Language language)
    {
        switch (language)
        {
            case Language.Japanese:
                return "{0}が{1}を習得した!";
            case Language.English:
                return "{0} has learned {1}!";
            default:
                return "{0} has learned {1}!";
        }
    }

    public static string GetSkillCaption(string skillName)
    {
        switch (skillName)
        {
            case "ヒール":
                return HealCaption;
            case "噛みつく":
                return "Bite";
            default:
                return "Unknown";
        }
    }

    public static string HealCaption = "Heal";

    // ******* Reward *******
    public static string GetStatusRewardTitle(Language language)
    {
        switch (language)
        {
            case Language.Japanese:
                return "ステータス";
            case Language.English:
                return "Status";
            default:
                return "Status";
        }
    }

    // ****** ImageEffectKey *******
    public static string ImageAnimationKeySlash = "SlashAnimationEffect";
    public static string ImageAnimationKeyHeal = "HealAnimationEffect";
    public static string ImageAnimationKeyBite = "BiteAnimationEffect";
}
