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

    private ReactiveProperty<int> _hitPointRp = new ReactiveProperty<int>();
    private ReactiveProperty<int> _manaPointRp = new ReactiveProperty<int>();

    private AbnormalCondition _abnormalCondition;
    private SpriteRenderer _spriteRenderer;


    public void Initialize(Parameter parameter)
    {
        Parameter = parameter;

        Id = EntityMaster.AssignId();
        gameObject.name = parameter.Name;

        _abnormalCondition = new AbnormalCondition();

        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.sprite = Parameter.IconSprite;

        _hitPointRp.Value = Parameter.HitPoint;
        _manaPointRp.Value = Parameter.ManaPoint;
    }

    public int Attack(Entity target)
    {
        // 攻撃力のポテンシャルを計算
        int potential = Parameter.Power + _abnormalCondition.PowerGain;
        // 攻撃力のポテンシャルのオフセット内でランダムな値を返す
        int damage = Constants.GetRandomizedValueWithinOffset(potential, Constants.AttackOffsetPercent);
        target.TakeDamage(damage);
        return damage;
    }

    public Skill.SkillResult UseSkill(string skillName, Entity skillUser, Entity opponent)
    {
        Skill skill = Parameter.Skills.Find(s => s.Name == skillName);
        return skill.Execute(skillUser, opponent);
    }

    public void TakeDamage(int damage)
    {
        // 先にリアクションプロパティを更新してから、エンティティのHPを更新する
        // これによって、エンティティのHPが変化したことをBattleControllerに通知し，差分を取得できる
        _hitPointRp.Value = Parameter.HitPoint - damage;
        Parameter.HitPoint = _hitPointRp.Value;
    }

    public void UseManaPoint(int manaCost)
    {
        _manaPointRp.Value = Parameter.ManaPoint - manaCost;
        Parameter.ManaPoint = _manaPointRp.Value;
    }
}
