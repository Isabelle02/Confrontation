using FuryLion.UI;
using UnityEngine;

public class TalentButton : BaseButton
{
    [SerializeField] private TalentConfig _talentConfig;
    [SerializeField] private GameObject _cost;
    [SerializeField] private GameObject _blockPanel;

    private Text _costText;

    public TalentConfig TalentConfig => _talentConfig;
    
    private void Awake()
    {
        _costText = _cost.GetComponentInChildren<Text>();
        _costText.Value = _talentConfig.Cost.ToString();
    }

    public void SetBlockPanelActive(bool isActive)
    {
        _blockPanel.SetActive(isActive);
    }

    public void SetCostActive(bool isActive)
    {
        _cost.gameObject.SetActive(isActive);
    }
}