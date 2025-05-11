using Cysharp.Threading.Tasks;
using UnityEngine;
using R3;

public class Entity : MonoBehaviour
{
    public int Id { get; private set; }

    public EntityType EntityType;
    public Parameter Parameter { get; private set; }

    public ReadOnlyReactiveProperty<int> HitPointRp => _hitPointRp;
    public ReadOnlyReactiveProperty<int> ManaPointRp => _manaPointRp;

    public bool IsAlive { get; set; } = true;

    public bool IsNpc { get; private set; } = false;

    public int AttackPower => Parameter.Power + _abnormalCondition.PowerGain;

    private ReactiveProperty<int> _hitPointRp = new ReactiveProperty<int>();
    private ReactiveProperty<int> _manaPointRp = new ReactiveProperty<int>();

    private AbnormalCondition _abnormalCondition;
    private SpriteRenderer _spriteRenderer;


    public void Initialize(Parameter parameter, bool isNpc)
    {
        Parameter = parameter;

        Id = EntityMaster.AssignId();
        gameObject.name = parameter.Name;

        _abnormalCondition = new AbnormalCondition();

        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.sprite = Parameter.IconSprite;

        _hitPointRp.Value = Parameter.HitPoint;
        _manaPointRp.Value = Parameter.ManaPoint;

        IsNpc = isNpc;
    }

    public int Attack(Entity target)
    {
        // 攻撃力のポテンシャルのオフセット内でランダムな値を返す
        int damage = Constants.GetRandomizedValueWithinOffsetWithMissPotential(AttackPower, Constants.AttackOffsetPercent, 10);
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


    public void ChangeVisibility(bool isVisible)
    {
        _spriteRenderer.enabled = isVisible;
    }

    public void SetAbnormalCondition(AbnormalCondition condition)
    {
        _abnormalCondition = condition;
    }

    public void ResetAbnormalCondition()
    {
        _abnormalCondition = new AbnormalCondition();
    }
}
