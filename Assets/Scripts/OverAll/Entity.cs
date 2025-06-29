using Cysharp.Threading.Tasks;
using UnityEngine;
using R3;
using TMPro;
using Fusion;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;

public class Entity : NetworkBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _nameLabel;
    public int Id { get; private set; }

    public EntityType EntityType;
    public Parameter BaseParameter { get; private set; }

    public int Hp;
    public int Mp;

    public ReadOnlyReactiveProperty<int> HitPointRp => _hitPointRp;
    public ReadOnlyReactiveProperty<int> ManaPointRp => _manaPointRp;

    private ReactiveProperty<int> _hitPointRp = new ReactiveProperty<int>();
    private ReactiveProperty<int> _manaPointRp = new ReactiveProperty<int>();

    // HPとMPはリアクティブプロパティの利用によるアニメーションがあるため，新しい値を設定する際はリアクティブプロパティを経由する
    // そのために一時的な値を保持するプロパティを用意する
    [Networked, OnChangedRender(nameof(OnHpChanged))]
    public int NewHpTemp { get; set; }

    [Networked, OnChangedRender(nameof(OnMpChanged))]
    public int NewMpTemp { get; set; }


    [Networked, OnChangedRender(nameof(OnSpriteChanged))]
    public NetworkString<_32> FieldSpriteAssetReference { get; set; }
    [Networked]
    public NetworkString<_32> BattleSpriteAssetReference { get; set; }

    // NetworkArrayを使ってスキルリストを同期する
    [Networked, Capacity(16), OnChangedRender(nameof(OnSkillsChanged))]
    public NetworkArray<SkillList.SkillType> SkillTypes { get; }

    [Networked]
    public int AbnormalConditionPowerGain { get; set; }

    [Networked]
    public Condition AbnormalConditionType { get; set; }

    [Networked]
    public bool IsNpc { get; set; } = false;

    public List<Skill> SyncedSkills { get; private set; } = new List<Skill>();
    public bool IsAlive => Hp > 0;

    public int AttackPower => BaseParameter.Power + AbnormalConditionPowerGain;

    private SpriteRenderer _spriteRenderer;


    public override void Spawned()
    {
        OnSkillsChanged();
    }

    public void Initialize(Parameter parameter, int id,
    NetworkString<_32> fieldSpriteAssetReference, NetworkString<_32> battleSpriteAssetReference, bool isNpc)
    {
        BaseParameter = parameter;
        Hp = parameter.HitPoint;
        Mp = parameter.ManaPoint;
        Id = id;
        IsNpc = isNpc;
        FieldSpriteAssetReference = fieldSpriteAssetReference;
        BattleSpriteAssetReference = battleSpriteAssetReference;

        gameObject.name = parameter.Name;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _hitPointRp.Value = Hp;
        _manaPointRp.Value = Mp;
        ResetAbnormalCondition();
    }

    public int Attack(Entity target)
    {
        // 攻撃力のポテンシャルのオフセット内でランダムな値を返す
        int damage = Constants.GetRandomizedValueWithinOffsetWithMissPotential(AttackPower, Constants.AttackOffsetPercent, Constants.MissPotentialOnEveryDamageAction);
        target.SetHitPoint(target.Hp - damage);
        return damage;
    }

    public Skill.SkillResult UseSkill(string skillName, Entity skillUser, Entity opponent)
    {
        Skill skill = SyncedSkills.Find(s => s.Name == skillName);
        return skill.Execute(skillUser, opponent);
    }

    public void SetHitPoint(int newHp)
    {
        NewHpTemp = newHp;
    }

    public void SetManaPoint(int newMana)
    {
        NewMpTemp = newMana;
    }

    private void OnSpriteChanged()
    {
        _spriteRenderer.sprite = Addressables.LoadAssetAsync<Sprite>(FieldSpriteAssetReference).WaitForCompletion();
    }

    private void OnHpChanged()
    {
        _manaPointRp.Value = NewHpTemp;
        Hp = _hitPointRp.Value;
    }

    private void OnMpChanged()
    {
        _manaPointRp.Value = NewMpTemp;
        Mp = _manaPointRp.Value;
    }

    private void OnSkillsChanged()
    {
        UpdateLocalSkillList();
    }

    private void UpdateLocalSkillList()
    {
        SyncedSkills.Clear();
        foreach (var skillType in SkillTypes)
        {
            if (skillType != default)
            {
                SyncedSkills.Add(SkillList.GetSkill(skillType));
            }
        }
    }


    public void ChangeVisibility(bool isVisible)
    {
        _spriteRenderer.enabled = isVisible;
    }

    public void SetAbnormalCondition(Condition? type, int? powerGain)
    {
        AbnormalConditionType = type ?? AbnormalConditionType;
        AbnormalConditionPowerGain = powerGain ?? AbnormalConditionPowerGain;
    }

    public void ResetAbnormalCondition()
    {
        AbnormalConditionType = Condition.None;
        AbnormalConditionPowerGain = 0;
    }

    public void SetName(string name)
    {
        _nameLabel.text = name;
    }
}
