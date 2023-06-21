using FuryLion.UI;
using UnityEngine;

public class UpgradeInfoItem : ListElement
{
    [SerializeField] private Text _description;
    [SerializeField] private Text _currentValue;
    [SerializeField] private Text _upgradeValue;

    public void SetData(string description, string current, string upgrade)
    {
        _description.Value = description;
        _currentValue.Value = current;
        _upgradeValue.Value = upgrade;
    }

    public void ClearUpgradeField()
    {
        _upgradeValue.Value = "";
    }
}