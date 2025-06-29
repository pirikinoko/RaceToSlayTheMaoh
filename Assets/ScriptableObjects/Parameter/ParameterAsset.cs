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
    public string Name;
    public int HitPoint;
    public int ManaPoint;
    public int Power;
    public List<SkillList.SkillType> SkillTypes = new();

    // コピー用のコンストラクタ
    public Parameter(Parameter original)
    {
        EntityType = original.EntityType;
        Name = original.Name;
        HitPoint = original.HitPoint;
        ManaPoint = original.ManaPoint;
        Power = original.Power;
        SkillTypes = new List<SkillList.SkillType>(original.SkillTypes);
    }

    public Parameter Clone()
    {
        return new Parameter(this);
    }
}

