using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "ParameterAsset", menuName = "ScriptableObjects/ParameterAsset")]
public class ParameterAsset : ScriptableObject
{
    public List<Parameter> ParameterList = new();
}

[System.Serializable]
public class Parameter
{
    public EntityType EntityType;
    public string Name;
    public int HitPoint;
    public int ManaPoint;
    public int Power;
    public List<Skill> Skills;
    public Sprite IconSprite;

    // コピーコンストラクタ
    public Parameter(Parameter original)
    {
        EntityType = original.EntityType;
        Name = original.Name;
        HitPoint = original.HitPoint;
        ManaPoint = original.ManaPoint;
        Power = original.Power;

        // スキルのディープコピー
        Skills = original.Skills.Select(skill => new Skill(skill.Name, skill.EffectId)).ToList();

        IconSprite = original.IconSprite;
    }

    // クローンメソッド
    public Parameter Clone()
    {
        return new Parameter(this);
    }
}

