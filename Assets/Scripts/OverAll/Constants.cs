using UnityEngine;

public class Constants
{
    // ******* AssetReference *******   

    public static string AssetReferenceParameter = "Parameter";
    public static string AssetReferencePlayer = "Player";
    public static string AssetReferenceEnemy = "Enemy";

    // ******* Player *******

    public static float PlayerMoveSpeed = 1.5f;

    // ******* Entity *******

    public static int MaxHitPoint = 50;
    public static int MaxManaPoint = 30;

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
                return string.Format("{0}‚Æ{1}‚Ì–Ú‚ª‡‚Á‚½‚æ‚¤‚¾!", leftEntityname, rightEntityname);
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
                return string.Format("{0}‚Ìs“®‚ğ‘Ò‚Á‚Ä‚¢‚é...", entityName);
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
                return string.Format("{0}‚ÌUŒ‚I", attacker);
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
                return string.Format("{0}‚É{1}‚Ìƒ_ƒ[ƒW‚ğ—^‚¦‚½I", damageReciever, damage);
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
                return string.Format("{0}‚Ì{1}", entityName, skillName);
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
                return string.Format("{0}‚Í{1}‚ğ“|‚µ‚½!", winner, loser);
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
                return "—¼Ò“|‚ê‚Ä‚µ‚Ü‚Á‚½I";
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
                return "‚±‚Ìí‚¢‚É‚ÍŒˆ’…‚ª’…‚«‚»‚¤‚É‚È‚¢.....";
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
                return "{0}‚Í{1}‚ğæ“¾‚µ‚½!";
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
                return "{0}‚Í{1}HP‰ñ•œ‚µ‚½!";
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
                return "{0}‚Í{1}MP•â‹‹‚µ‚½!";
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
                return "{0}‚Í{1}‚ğæ“¾‚µ‚½!";
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
            case "ƒq[ƒ‹":
                return HealCaption;
            case "Šš‚İ‚Â‚­":
                return "Bite";
            default:
                return "Unknown";
        }
    }
    public static string HealCaption = "Heal";
}
