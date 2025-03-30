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

    // ******* General *******
    // 指定されたパーセンテージのオフセット内でランダムな値を返す
    // 例: baseValue = 100, offsetPercent = 20 の場合, 80から120の間でランダムな値を返す
    public static int GetRandomizedValueWithinOffset(int baseValue, int offsetPercent)
    {
        var offsetValue = (int)(baseValue * offsetPercent / 100);
        var min = baseValue - offsetValue;
        var max = baseValue + offsetValue;
        var randomValue = Random.Range(min, max);
        // 最低でも1を返す
        return randomValue >= 1 ? randomValue : 1;
    }

    // ******* Player *******

    public static float PlayerMoveSpeed = 1.5f;

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
                return string.Format("{0}��{1}�̖ڂ��������悤��!", leftEntityname, rightEntityname);
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
                return string.Format("{0}�̍s����҂��Ă���...", entityName);
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
                return string.Format("{0}�̍U���I", attacker);
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
                return string.Format("{0}��{1}�̃_���[�W��^�����I", damageReciever, damage);
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
                return string.Format("{0}��{1}", entityName, skillName);
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
                return string.Format("{0}��{1}��|����!", winner, loser);
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
                return "���ғ|��Ă��܂����I";
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
                return "���̐킢�ɂ͌��������������ɂȂ�.....";
            case Language.English:
                return "This battle seems to be endless.....";
            default:
                return "This battle seems to be endless.....";
        }
    }

    public static string GetSkillGetSentence(Language language)
    {
        switch (language)
        {
            case Language.Japanese:
                return "{0}��{1}���擾����!";
            case Language.English:
                return "{0} has learned {1}!";
            default:
                return "This battle seems to be endless.....";
        }
    }
    // ******* RewordResult *******
    public static string GetHitPointSentence(Language language)
    {
        switch (language)
        {
            case Language.Japanese:
                return "{0}��{1}HP�񕜂���!";
            case Language.English:
                return "{0} has healed {1}HP!";
            default:
                return "This battle seems to be endless.....";
        }
    }
    public static string GetManaSentence(Language language)
    {
        switch (language)
        {
            case Language.Japanese:
                return "{0}��{1}MP�⋋����!";
            case Language.English:
                return "{0} has refreshed {1}MagicPoint!";
            default:
                return "This battle seems to be endless.....";
        }
    }
    public static string GetPowerSentence(Language language)
    {
        switch (language)
        {
            case Language.Japanese:
                return "{0}��{1}���擾����!";
            case Language.English:
                return "{0} has learned {1}!";
            default:
                return "This battle seems to be endless.....";
        }
    }
    // ******* SkillCaption *******
    public static string GetSkillCaption(string skillName)
    {
        switch (skillName)
        {
            case "�q�[��":
                return HealCaption;
            case "���݂�":
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
                return "�X�e�[�^�X";
            case Language.English:
                return "Status";
            default:
                return "Status";
        }
    }
}
