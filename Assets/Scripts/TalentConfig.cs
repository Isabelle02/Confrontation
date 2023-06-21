using System;
using UnityEngine;

public enum Talent
{
    UnitSpeed,
    UnitForce,
    MilitaryReproduction,
    ArmyReproduction,
    Mine,
    Farm,
    WizardTower,
    Forge,
    Stable,
    Quarry,
    Workshop,
    Fort
}

[CreateAssetMenu(fileName = "Talent", menuName = "Infos/Talent")]
[Serializable]
public class TalentConfig : ScriptableObject
{
    public Talent Talent;
    public int Cost;
    public string Info;
}