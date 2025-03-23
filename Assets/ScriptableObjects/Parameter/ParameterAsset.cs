using System.Collections.Generic;
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
    public Sprite IconSprite;
    public string Name;
    public int HitPoint;
    public int ManaPoint;
    public int Power;
    public List<SkillList.SkillType> SkillTypes = new();
    public List<Skill> Skills = new();

    // コピーコンストラクタ
    public Parameter(Parameter original)
    {
        EntityType = original.EntityType;
        Name = original.Name;
        HitPoint = original.HitPoint;
        ManaPoint = original.ManaPoint;
        Power = original.Power;
        // スキルのディープコピー
        foreach (var skillType in original.SkillTypes)
        {
            var skill = SkillList.GetSkill(skillType);
            Skills.Add(new Skill(skill.Name, skill.Description, skill.ManaCost, skill.Action));
        }
        IconSprite = original.IconSprite;
    }

    public Parameter Clone()
    {
        return new Parameter(this);
    }
}

