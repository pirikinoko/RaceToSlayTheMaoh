using UnityEngine;

public class Constants
{
    // ******* AssetReference *******   

    public static string AssetReferenceParameter = "Parameter";
    public static string AssetReferencePlayer = "Player";
    public static string AssetReferenceEnemy = "Enemy";

    // ******* Player *******

    public static float PlayerMoveSpeed = 1.5f;

    // ******* Enemy *******

    // ******* Field *******

    public static Vector3 FieldCornerDownLeft = new Vector3(-7, -7, 0);
    public static Vector3 FieldCornerUpRight = new Vector3(8, 8, 0);
    public static Vector3 PlayerSpownPosition = new Vector3(-7, -7, 0);

    // ******* Battle *******
    public static int MaxTurn = 20;

    public static string GetSentenceWhenStartBattle(string language, string entityName)
    {
        switch (language)
        {
            case "Japanese":
                return string.Format("{0}‚ªŒ»‚ê‚½!", entityName);
            case "English":
                return string.Format("{0} Appeared!", entityName);
            default:
                return string.Format("{0} Appeared!", entityName);
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

    public static string GetAttackSentence(Language language, string entityName)
    {
        switch (language)
        {
            case Language.Japanese:
                return string.Format("{0}‚ÌUŒ‚I", entityName);
            case Language.English:
                return string.Format("{0} is Attacking!", entityName);
            default:
                return string.Format("{0} is Attacking!", entityName);
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
