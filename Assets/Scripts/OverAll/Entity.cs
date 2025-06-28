using Cysharp.Threading.Tasks;
using UnityEngine;
using R3;
using TMPro;
using Fusion;
using System.Collections.Generic;

public class Entity : NetworkBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _nameLabel;
    public int Id { get; private set; }

    public EntityType EntityType;
    public Parameter Parameter { get; private set; }

    public ReadOnlyReactiveProperty<int> HitPointRp => _hitPointRp;
    public ReadOnlyReactiveProperty<int> ManaPointRp => _manaPointRp;

    [Networked, OnChangedRender(nameof(OnHpChanged))]
    public int Hp { get; set; }

    [Networked, OnChangedRender(nameof(OnMpChanged))]
    public int Mp { get; set; }

    [Networked]
    public int Power { get; set; }

    // NetworkArrayを使ってスキルリストを同期する
    [Networked, Capacity(16), OnChangedRender(nameof(OnSkillsChanged))]
    public NetworkArray<SkillList.SkillType> SkillTypes { get; }

    // --- ローカルでのみ使用するデータ ---
    public Parameter BaseParameter { get; private set; }
    public List<Skill> SyncedSkills { get; private set; } = new List<Skill>();
    public bool IsAlive => Hp > 0; // IsAliveは現在のHPから算出する
    [Networked]
    public bool IsNpc { get; private set; } = false;

    public int AttackPower => Parameter.Power + _abnormalCondition.PowerGain;

    private ReactiveProperty<int> _hitPointRp = new ReactiveProperty<int>();
    private ReactiveProperty<int> _manaPointRp = new ReactiveProperty<int>();

    private AbnormalCondition _abnormalCondition;
    private SpriteRenderer _spriteRenderer;


    public override void Spawned()
    {
        // 初期化時に一度コールバックを呼んでおくことで、
        // スポーン直後の値がUIに反映されることを保証します。
        OnHpChanged();
        OnMpChanged();
        OnSkillsChanged();
    }

    public void Initialize(Parameter parameter, bool isNpc)
    {
        Parameter = parameter;

        Id = EntityMaster.AssignId();
        gameObject.name = parameter.Name;

        _abnormalCondition = new AbnormalCondition();

        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.sprite = Parameter.FieldSprite;

        _hitPointRp.Value = Parameter.HitPoint;
        _manaPointRp.Value = Parameter.ManaPoint;

        IsNpc = isNpc;
    }

    public int Attack(Entity target)
    {
        // 攻撃力のポテンシャルのオフセット内でランダムな値を返す
        int damage = Constants.GetRandomizedValueWithinOffsetWithMissPotential(AttackPower, Constants.AttackOffsetPercent, Constants.MissPotentialOnEveryDamageAction);
        target.SetHitPoint(target.Parameter.HitPoint - damage);
        return damage;
    }

    public Skill.SkillResult UseSkill(string skillName, Entity skillUser, Entity opponent)
    {
        Skill skill = Parameter.Skills.Find(s => s.Name == skillName);
        return skill.Execute(skillUser, opponent);
    }

    public void SetHitPoint(int newHp)
    {
        // 古いHPも参照して処理をしたいので，先にリアクションプロパティを更新してから、エンティティのHPを更新する
        _hitPointRp.Value = newHp;
        Parameter.HitPoint = _hitPointRp.Value;
    }

    public void SetManaPoint(int newMana)
    {
        _manaPointRp.Value = newMana;
        Parameter.ManaPoint = _manaPointRp.Value;
    }

    private void OnHpChanged()
    {
        _hitPointRp.Value = Hp;
    }

    private void OnMpChanged()
    {
        _manaPointRp.Value = Mp;
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

    public AbnormalCondition GetAbnormalCondition()
    {
        return _abnormalCondition;
    }

    public void SetAbnormalCondition(AbnormalCondition condition)
    {
        _abnormalCondition = condition;
    }

    public void ResetAbnormalCondition()
    {
        _abnormalCondition = new AbnormalCondition();
    }

    public void SetName(string name)
    {
        _nameLabel.text = name;
    }
}
