using System;
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
    public EntityIdentifier Id;
    public string Name;
    public int Hp;
    public int Atk;
    public Texture2D Icon;

    internal object FirstOrDefault(Func<object, bool> value)
    {
<<<<<<< Updated upstream
        throw new NotImplementedException();
=======
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
>>>>>>> Stashed changes
    }
}
