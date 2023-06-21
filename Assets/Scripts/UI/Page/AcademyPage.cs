// Copyright (c) 2012-2022 FuryLion Group. All Rights Reserved.

using System.Collections.Generic;
using Data;
using FuryLion;
using FuryLion.UI;
using UnityEngine;

public sealed class AcademyPage : Page
{
    [SerializeField] private Text _currency;
    [SerializeField] private BaseButton _backButton;
    [SerializeField] private BaseButton _infoButton;
    [SerializeField] private List<TalentNode> _talentNodes = new List<TalentNode>();

    private readonly List<TalentButton> _availableTalentButtons = new List<TalentButton>();

    private Vector3 _mouseDownPos;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _mouseDownPos = MouseManager.GetMousePosition(0, CameraManager.AcademyCamera);
        }

        if (Input.GetMouseButton(0) && !InfoMessageBox.IsOpen)
        {
            if (_mouseDownPos != MouseManager.GetMousePosition(0, CameraManager.AcademyCamera))
            {
                CameraManager.CameraMove(CameraManager.AcademyCamera, _mouseDownPos, Bounds.center, Bounds.size / 2);
            }
        }
    }

    private void OnBuyButtonClick(TalentButton button)
    {
        if (!_availableTalentButtons.Contains(button) || button.TalentConfig.Cost > PlayerData.GameCurrency) 
            return;
        
        LevelManager.PlayerData.BoughtTalents.Add(button.TalentConfig.Talent);
        PlayerData.GameCurrency -= button.TalentConfig.Cost;
        _currency.Value = PlayerData.GameCurrency.ToString();
        SoundManager.PlaySound(Sounds.Music.Buy);
        SetTalent(button);
        _availableTalentButtons.Clear();
        foreach (var t in _talentNodes) 
            t.Traverse(UpdateNode);
    }

    private void OnTalentButtonClick(TalentButton button)
    {
        var position = button.Position + Vector3.down * 10;
        var isAvailable = _availableTalentButtons.Contains(button);
        TalentMessageBox.Init(position, button.TalentConfig.Info, GetBaseValue(button), 
            GetTalentValue(button), () => OnBuyButtonClick(button), isAvailable);
        MessageBoxManager.Open<TalentMessageBox>();
    }

    private string GetBaseValue(TalentButton button)
    {
        return button.TalentConfig.Talent switch
        {
            Talent.UnitSpeed => LevelManager.PlayerData.BaseSpeed.ToString("F"),
            Talent.UnitForce => LevelManager.PlayerData.BaseForce.ToString("F"),
            Talent.MilitaryReproduction => (1 / LevelManager.PlayerData.BaseMilitaryReproduction).ToString("F"),
            Talent.ArmyReproduction => (1 / LevelManager.PlayerData.BaseArmyReproduction).ToString("F"),
            _ => ""
        };
    }

    private string GetTalentValue(TalentButton button)
    {
        return button.TalentConfig.Talent switch
        {
            Talent.UnitSpeed => "+ 0,15",
            Talent.UnitForce => "+ 0.5",
            Talent.MilitaryReproduction => 
                "+ " + (1 / (LevelManager.PlayerData.BaseMilitaryReproduction - 0.15) 
                        - 1 / LevelManager.PlayerData.BaseMilitaryReproduction).ToString("F"),
            Talent.ArmyReproduction => 
                "+ " + (1 / (LevelManager.PlayerData.BaseArmyReproduction - 0.15) 
                        - 1 / LevelManager.PlayerData.BaseArmyReproduction).ToString("F"),
            _ => ""
        };
    }

    private void SetTalent(TalentButton button)
    {
        switch (button.TalentConfig.Talent)
        {
            case Talent.UnitSpeed:
                LevelManager.PlayerData.BaseSpeed += 0.15f;
                break;
            case Talent.UnitForce:
                LevelManager.PlayerData.BaseForce += 0.5f;
                break;
            case Talent.MilitaryReproduction:
                LevelManager.PlayerData.BaseMilitaryReproduction -= 0.15f;
                break;
            case Talent.ArmyReproduction:
                LevelManager.PlayerData.BaseArmyReproduction -= 0.15f;
                break;
            case Talent.Mine:
                LevelManager.PlayerData.AvailableBuildings.Add(BuildingType.Mine);
                break;
            case Talent.Farm:
                LevelManager.PlayerData.AvailableBuildings.Add(BuildingType.Farm);
                break;
            case Talent.WizardTower:
                LevelManager.PlayerData.AvailableBuildings.Add(BuildingType.WizardTower);
                break;
            case Talent.Forge:
                LevelManager.PlayerData.AvailableBuildings.Add(BuildingType.Forge);
                break;
            case Talent.Stable:
                LevelManager.PlayerData.AvailableBuildings.Add(BuildingType.Stable);
                break;
            case Talent.Quarry:
                LevelManager.PlayerData.AvailableBuildings.Add(BuildingType.Quarry);
                break;
            case Talent.Workshop:
                LevelManager.PlayerData.AvailableBuildings.Add(BuildingType.Workshop);
                break;
            case Talent.Fort:
                LevelManager.PlayerData.AvailableBuildings.Add(BuildingType.Fort);
                break;
        }
    }

    private void OnInfoButtonClick()
    {
        InfoMessageBox.Init(MessageInfo.AcademyInfo);
        MessageBoxManager.Open<InfoMessageBox>();
    }

    private void OnBackButtonClick()
    {
        BaseMessageBox.CloseLast();
        CloseLast();
    }

    protected override void OnCreate()
    {
        _backButton.Click += OnBackButtonClick;
        _infoButton.Click += OnInfoButtonClick;
        foreach (var t in _talentNodes) 
            t.Traverse(b => b.Click += () => OnTalentButtonClick(b));
    }

    protected override void OnOpenStart(ViewParam viewParam)
    {
        _currency.Value = PlayerData.GameCurrency.ToString();
        _availableTalentButtons.Clear();
        foreach (var t in _talentNodes) 
            t.Traverse(UpdateNode);
    }
    
    private void OnDisable()
    {
        Storage.Save(LevelManager.PlayerDataFilePath, LevelManager.PlayerData);
    }

    private void UpdateNode(TreeNode<TalentButton> node)
    {
        var isBought = LevelManager.PlayerData.BoughtTalents.Contains(node.Value.TalentConfig.Talent);
        if (node.Parent == null && !isBought || 
            node.Parent != null && !isBought &&
            LevelManager.PlayerData.BoughtTalents.Contains(node.Parent.Value.TalentConfig.Talent))
        {
            node.Value.SetBlockPanelActive(false);
            node.Value.SetCostActive(true);
            _availableTalentButtons.Add(node.Value);
        }
        else if (isBought)
        {
            node.Value.SetBlockPanelActive(false);
            node.Value.SetCostActive(false);
        }
    }
}
