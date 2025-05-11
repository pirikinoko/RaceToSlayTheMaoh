using UnityEngine;

public class Constants
{
    // ******* AssetReference *******   

    public static string AssetReferenceParameter = "Parameter";
    public static string GetAssetReferencePlayer(int playerId)
    {
        return string.Format("Player{0}", playerId);
    }
    public static string GetAssetReferencePlayerIcon(int playerId)
    {
        return string.Format("PlayerIcon{0}", playerId);
    }
    public static string AssetReferenceEnemy = "Enemy";
    public static string AssetReferencePlayerIcon = "PlayerIcon";
    public static string AssetReferenceHeartIcon = "HeartIcon";
    public static string AssetReferenceManaIcon = "ManaIcon";
    public static string AssetReferencePowerIcon = "PowerIcon";
    public static string AssetReferenceDamageNumberEffect = "DamageNumberEffect";
    public static string AssetReferenceCoffin = "Coffin";

    // ******* General *******
    // 指定されたパーセンテージのオフセット内でランダムな値を返す
    // 例: baseValue = 100, offsetPercent = 20 の場合, 80から120の間でランダムな値を返す
    // ただしミスすることもある
    public static int GetRandomizedValueWithinOffsetWithMissPotential(int baseValue, int offsetPercent, int missPotential)
    {
        var offsetValue = (int)(baseValue * offsetPercent / 100);
        // minが1未満の場合は1を返す
        var min = ((baseValue - offsetValue) < 1) ? 1 : baseValue - offsetValue;
        // +1は上限値を含めるため
        var max = baseValue + offsetValue + 1;
        var randomValue = Random.Range(min, max);
        // 最低でも1を返す
        randomValue = randomValue < 1 ? 1 : randomValue;
        var isMissed = Random.Range(0, 100) < missPotential;
        return isMissed ? 0 : randomValue;
    }

    public static int MissPotentialOnEveryDamageAction = 10;

    // ******* Player *******
    public static float PlayerMoveSpeed { get; set; } = 2.0f;
    public static int MaxMoves { get; set; } = 3;

    public static string GetPlayerName(Language language, int playerId)
    {
        switch (language)
        {
            case Language.Japanese:
                return string.Format("プレイヤー{0}", playerId);
            case Language.English:
                return string.Format("Player{0}", playerId);
            default:
                return string.Format("Player{0}", playerId);
        }
    }

    public static string[] GetNpcNames(Language language)
    {
        switch (language)
        {
            case Language.Japanese:
                return new string[] { "Player1", "アレックス", "モーガン", "テイラー" };
            case Language.English:
                return new string[] { "Hero", "Jordan", "Morgan", "Taylor" };
            default:
                return new string[] { "Hero", "Jordan", "Morgan", "Taylor" };
        }
    }

    // ******* Title *******
    public static string GetSentenceForLocalPlayButton(Language language, int playerCount)
    {
        switch (language)
        {
            case Language.Japanese:
                return string.Format("{0}人でローカルプレイ", playerCount);
            case Language.English:
                return string.Format("{0} Players Local Play", playerCount);
            default:
                return string.Format("{0} Players Local Play", playerCount);
        }
    }

    // ******* Main *******
    public static int MaxPlayerCountIncludingNpc { get; set; } = 4;
    public static int MinPlayerCount { get; set; } = 1;
    public static int MaxDiceValue { get; set; } = 3;
    public static float DiceRollUpdateInterval { get; set; } = 0.02f;
    public static int DiceHighlightBlinkCount { get; set; } = 5;
    public static float DiceHighlightBlinkInterval { get; set; } = 0.15f;

    // ******* Entity *******

    public static int MaxHitPoint { get; set; } = 50;
    public static int MaxManaPoint { get; set; } = 30;
    public static int AttackOffsetPercent { get; set; } = 50;

    // ******* Camera *******
    public static float CameraMoveDuration { get; set; } = 1f;
    public static float CameraZoomDuration { get; set; } = 1f;
    public static float CameraZoomFactor { get; set; } = 0.5f; // ズーム倍率を追加

    // ******* Field *******
    public static Vector3 FieldCornerUpLeft { get; set; } = new Vector3(-5, -5, 0);
    public static Vector3 FieldCornerUpRight { get; set; } = new Vector3(-1, -5, 0);
    public static Vector3 FieldCornerDownLeft { get; set; } = new Vector3(1, -5, 0);
    public static Vector3 FieldCornerDownRight { get; set; } = new Vector3(5, -5, 0);

    public static LayerMask EntityLayerMask { get; set; } = LayerMask.GetMask("Entity");

    public static Vector3[] PlayerSpownPositions { get; set; } =
    {
        FieldCornerUpLeft,
        FieldCornerUpRight,
        FieldCornerDownLeft,
        FieldCornerDownRight
    };
    public static Vector2 ScaleForActivePlayerStatusBox { get; set; } = new Vector2(1.0f, 1.0f);
    public static Vector2 ScaleForWaitingPlayersStatusBox { get; set; } = new Vector2(0.7f, 0.7f);

    public static float DelayBeforeNewTurnSeconds { get; set; } = 0.6f;

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
        if (damage == 0)
        {
            switch (language)
            {
                case Language.Japanese:
                    return string.Format("{0}は攻撃をかわした", damageReciever);
                case Language.English:
                    return string.Format("{0} has dodged the attack!", damageReciever);
                default:
                    return string.Format("{0} has dodged the attack!", damageReciever);
            }
        }
        switch (language)
        {
            case Language.Japanese:
                return string.Format("{1}のダメージ!", damageReciever, damage);
            case Language.English:
                return string.Format("{1} damage!", damageReciever, damage);
            default:
                return string.Format("{1} damage!", damageReciever, damage);
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


    public static string GetSentenceWhenEnemyWins(Language language, string enemyName)
    {
        switch (language)
        {
            case Language.Japanese:
                return string.Format("{0}は誇らしげに佇んでいる", enemyName);
            case Language.English:
                return string.Format("{0} is standing proudly", enemyName);
            default:
                return string.Format("{0} is standing proudly", enemyName);
        }
    }

    public static string GetSentenceWhenGameClear(Language language, string playerName)
    {
        switch (language)
        {
            case Language.Japanese:
                return string.Format("{0}は世界に再び光をもたらした!", playerName);
            case Language.English:
                return string.Format("{0} brought light back to the world!", playerName);
            default:
                return string.Format("{0} brought light back to the world!", playerName);
        }
    }

    public static string GetSentenceWhenSelectingReward(Language language, bool IsSelectingSelf, string SelecterName)
    {
        switch (language)
        {
            case Language.Japanese:
                return IsSelectingSelf ? "報酬を選択してください" : $"{SelecterName}が報酬を選択しています";
            case Language.English:
                return IsSelectingSelf ? "Please select your reward" : $"{SelecterName} is selecting a reward";
            default:
                return IsSelectingSelf ? "Please select your reward" : $"{SelecterName} is selecting a reward";
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

    public static string GetSkillCaption(string skillName)
    {
        switch (skillName)
        {
            case "ヒール":
                return HealCaption;
            case "噛みつく":
                return "Bite";
            case "イグニッション":
                return "Ignition";
            case "ドレイン":
                return "Drain";
            case "デストロイ":
                return "Destroy";
            case "リジェネ":
                return "Regen";
            case "スーパーヒール":
                return "SuperHeal";
            case "トレーニング":
                return "Training";
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

    // ******* Result *******
    public static string GetResultMessageWin(Language language, string winnerName)
    {
        switch (language)
        {
            case Language.Japanese:
                return string.Format("{0}は真の勇者であった！", winnerName);
            case Language.English:
                return string.Format("{0} was the true hero!", winnerName);
            default:
                return string.Format("{0} was the true hero!", winnerName);
        }
    }

    public static string GetTurnCountMessage(Language language, int turnCount)
    {
        switch (language)
        {
            case Language.Japanese:
                return $"{turnCount}ターンで世界を救った！";
            case Language.English:
                return $"You saved the world in {turnCount} turns!";
            default:
                return $"You saved the world in {turnCount} turns!";
        }
    }

    public static string GetBackToTitleButtonText(Language language)
    {
        switch (language)
        {
            case Language.Japanese:
                return "タイトルに戻る";
            case Language.English:
                return "Back to Title";
            default:
                return "Back to Title";
        }
    }

    public static float ResultFadeAlpha { get; set; } = 0.8f;
    public static float ResultFadeDuration { get; set; } = 1.5f;

    // ****** ImageEffectKey *******
    public static string ImageAnimationKeySlash = "SlashAnimationEffect";
    public static string ImageAnimationKeyHeal = "HealAnimationEffect";
    public static string ImageAnimationKeyBite = "BiteAnimationEffect";
    public static string ImageAnimationKeyIgnition = "IgnitionAnimationEffect";
    public static string ImageAnimationKeyDrain = "DrainAnimationEffect";
    public static string ImageAnimationKeyDestroy = "DestroyAnimationEffect";
    public static string ImageAnimationKeyRegen = "RegenAnimationEffect";
    public static string ImageAnimationKeySuperHeal = "SuperHealAnimationEffect";
    public static string ImageAnimationKeyTraining = "TrainingAnimationEffect";
}
