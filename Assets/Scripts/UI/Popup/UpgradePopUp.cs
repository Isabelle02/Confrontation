// Copyright (c) 2012-2022 FuryLion Group. All Rights Reserved.

using System.Collections.Generic;
using System.Globalization;
using Entities;
using FuryLion.UI;
using Interfaces;
using UnityEngine;

public sealed class UpgradePopUp : Popup
{
    [SerializeField] private BaseButton _upgradeButton;
    [SerializeField] private BaseButton _exitButton;

    [SerializeField] private Text _cost;
    [SerializeField] private Text _name;

    [SerializeField] private VerticalListView _verticalListView;

    [SerializeField] private Image _upgradeBlock;

    [SerializeField] private Text _upgradeText;
    [SerializeField] private GameObject _coin;

    private List<UpgradeInfoItem> _infoItems = new List<UpgradeInfoItem>();

    private Vector3 _baseTextPosition;

    public static CellEntity TargetCell;

    protected override void OnCreate()
    {
        _exitButton.Click += OnExitButtonClick;
        _upgradeButton.Click += OnUpgradeButtonClick;
        _baseTextPosition = _upgradeText.transform.localPosition;
    }

    protected override void OnOpenStart(ViewParam viewParam)
    {
        Gameplay.SetPause(true);
    }

    protected override void OnOpenComplete()
    {
        SetData();
    }

    private void SetData()
    {
        _coin.SetActive(TargetCell.Building.Level <= 4);
        _upgradeText.Value = TargetCell.Building.Level <= 4 ? "Улучшить" : "Максимум";
        _upgradeText.transform.localPosition = TargetCell.Building.Level <= 4 ? _baseTextPosition : new Vector3(_baseTextPosition.x, _baseTextPosition.y + 1.1f, _baseTextPosition.z);
        _cost.Value = TargetCell.Building.Level <= 4
            ? ShopManager
                .ActOnBuilding(TargetCell.Building, type => ShopManager.GetCost(type, TargetCell.Building.Level))
                .ToString() : "";
        if (TargetCell.Building.Level <= 4)
            _upgradeBlock.enabled =
                ShopManager.Customers.Find(c => c.TeamID == 1).Money < int.Parse(_cost.Value);
        
        switch (TargetCell.Building)
        {
            case ICapital capital:
                _name.Value = "Столица";
                ShowCapitalUpgradeInfo(capital as CapitalEntity);
                break;
            case ISettlement settlement:
                _name.Value = "Поселение";
                ShowSettlementUpgradeInfo(settlement as SettlementEntity);
                break;
            case IStable stable:
                _name.Value = "Конюшня";
                ShowStableUpgradeInfo(stable);
                break;
            case IBankable bankable:
                _name.Value = bankable is MineEntity ? "Шахта" : "Башня магов";
                ShowBankableUpgradeInfo(bankable);
                break;
            case IBarracks barracks:
                _name.Value = "Казармы";
                ShowBarracksUpgradeInfo(barracks as BarracksEntity);
                break;
            case IFarm farm:
                _name.Value = "Ферма";
                ShowFarmUpgradeInfo(farm);
                break;
            case IForge forge:
                _name.Value = "Кузница";
                ShowForgeUpgradeInfo(forge);
                break;
            case IQuarry quarry:
                _name.Value = "Каменоломня";
                ShowQuarryUpgradeInfo(quarry);
                break;
            case IWorkshop workshop:
                _name.Value = "Мастерская";
                ShowWorkShopUpgradeInfo(workshop);
                break;
        }

        foreach (var item in _infoItems) 
            _verticalListView.Add(item);

        if (TargetCell.Building.Level <= 4)
            return;
        
        foreach (var i in _infoItems)
            i.ClearUpgradeField();
    }

    private void ShowCapitalUpgradeInfo(CapitalEntity capital)
    {
        var militaryCapitalObj = Recycler.Get<UpgradeInfoItem>();
        float current = capital.MaxMilitaryCount * capital.Level / 2;
        float difference = capital.MaxMilitaryCount * (capital.Level + 1) / 2 -
                         capital.MaxMilitaryCount * capital.Level / 2;
        militaryCapitalObj.SetData("Максимальная численность гарнизона", current.ToString(CultureInfo.InvariantCulture),
            "+ " + difference);

        var currencyObj = Recycler.Get<UpgradeInfoItem>();
        current = capital.GetCurrency(capital.Level);
        difference = capital.GetCurrency(capital.Level + 1) - capital.GetCurrency(capital.Level);
        currencyObj.SetData("Количество добываемого золота в секунду", current.ToString(CultureInfo.InvariantCulture),
            "+ " + difference);

        var armyObj = Recycler.Get<UpgradeInfoItem>();
        current = capital.GetReproductionBonus(capital.Level);
        difference = capital.GetReproductionBonus(capital.Level + 1) - capital.GetReproductionBonus(capital.Level);
        armyObj.SetData("Бонус к ускоренному набору юнитов", current.ToString(CultureInfo.InvariantCulture),
            "+ " + difference);

        var baseMilitaryReproductionObj = Recycler.Get<UpgradeInfoItem>();
        current = capital.BaseMilitaryReproduction * (1 - capital.Level * 0.1f);
        var militaryTimeScale =
            capital.BaseMilitaryReproduction * (1 - capital.Level * 0.1f) -
            capital.BaseMilitaryReproduction * (1 - (capital.Level + 1) * 0.1f);
        baseMilitaryReproductionObj.SetData("Скорость увеличения гарнизона",
            current.ToString("F"), "- " + militaryTimeScale.ToString("F"));

        var baseArmyReproductionObj = Recycler.Get<UpgradeInfoItem>();
        current = capital.BaseArmyReproduction * (1 - capital.Level * 0.1f);
        var armyTimeScale = capital.BaseArmyReproduction * (1 - capital.Level * 0.1f) -
                            capital.BaseArmyReproduction * (1 - (capital.Level + 1) * 0.1f);
        baseArmyReproductionObj.SetData("Скорость увеличения армии",
            current.ToString("F"), "- " + armyTimeScale.ToString("F"));
        
        _infoItems = new List<UpgradeInfoItem>
        {
            militaryCapitalObj, currencyObj, armyObj, baseArmyReproductionObj,
            baseMilitaryReproductionObj
        };
    }

    private void ShowSettlementUpgradeInfo(SettlementEntity settlement)
    {
        var militarySettlementObj = Recycler.Get<UpgradeInfoItem>();
        float current = settlement.MaxMilitaryCount * settlement.Level / 2;
        float difference = settlement.MaxMilitaryCount * (settlement.Level + 1) / 2 -
                           settlement.MaxMilitaryCount * settlement.Level / 2;
        militarySettlementObj.SetData("Максимальная численность гарнизона",
            current.ToString(CultureInfo.InvariantCulture), "+ " + difference);

        var baseSettlementMilitaryReproductionObj = Recycler.Get<UpgradeInfoItem>();
        current = settlement.BaseMilitaryReproduction * (1 - settlement.Level * 0.1f);
        var militaryTimeScale =
            settlement.BaseMilitaryReproduction * (1 - settlement.Level * 0.1f) -
            settlement.BaseMilitaryReproduction * (1 - (settlement.Level + 1) * 0.1f);
        baseSettlementMilitaryReproductionObj.SetData("Скорость увеличения гарнизона",
            current.ToString("F"), "- " + militaryTimeScale.ToString("F"));

        _infoItems = new List<UpgradeInfoItem>
        {
            militarySettlementObj, baseSettlementMilitaryReproductionObj
        };
    }

    private void ShowBarracksUpgradeInfo(BarracksEntity barracks)
    {
        var baseArmyReproductionObj = Recycler.Get<UpgradeInfoItem>();
        var current = barracks.BaseArmyReproduction * (1 - barracks.Level * 0.1f);
        var armyTimeScale = barracks.BaseArmyReproduction * (1 - barracks.Level * 0.1f) -
                            barracks.BaseArmyReproduction * (1 - (barracks.Level + 1) * 0.1f);
        var difference = armyTimeScale % 10 != 0
            ? armyTimeScale.ToString("F")
            : armyTimeScale.ToString(CultureInfo.InvariantCulture);
        baseArmyReproductionObj.SetData("Скорость увеличения армии", current.ToString("F"), "- " + difference);
        _infoItems = new List<UpgradeInfoItem> {baseArmyReproductionObj};
    }

    private void ShowStableUpgradeInfo(IStable stable)
    {
        var speedBonusObj = Recycler.Get<UpgradeInfoItem>();
        speedBonusObj.SetData("Бонус к скорости юнитов",
            stable.GetSpeedBonus(stable.Level).ToString(CultureInfo.InvariantCulture),
            "+ " + (stable.GetSpeedBonus(stable.Level + 1) - stable.GetSpeedBonus(stable.Level)).ToString(CultureInfo
                .InvariantCulture));
        _infoItems = new List<UpgradeInfoItem> {speedBonusObj};
    }

    private void ShowBankableUpgradeInfo(IBankable bankable)
    {
        var currencyObj = Recycler.Get<UpgradeInfoItem>();
        var text = bankable is MineEntity
            ? "Количество добываемого золота в секунду"
            : "Количество вырабатываемой маны в секунду";
        currencyObj.SetData(text, bankable.GetCurrency(bankable.Level).ToString(),
            $"+ {bankable.GetCurrency(bankable.Level + 1) - bankable.GetCurrency(bankable.Level)}");
        _infoItems = new List<UpgradeInfoItem> {currencyObj};
    }

    private void ShowFarmUpgradeInfo(IFarm farm)
    {
        var reproductionObj = Recycler.Get<UpgradeInfoItem>();
        reproductionObj.SetData("Бонус к скорости роста юнитов",
            farm.GetReproductionBonus(farm.Level).ToString(CultureInfo.InvariantCulture),
            "+ " + (farm.GetReproductionBonus(farm.Level + 1) - farm.GetReproductionBonus(farm.Level)).ToString(
                CultureInfo
                    .InvariantCulture));
        _infoItems = new List<UpgradeInfoItem> {reproductionObj};
    }

    private void ShowForgeUpgradeInfo(IForge forge)
    {
        var forceObj = Recycler.Get<UpgradeInfoItem>();
        forceObj.SetData("Бонус к силе юнитов", forge.GetForceBonus(forge.Level).ToString(CultureInfo.InvariantCulture),
            $"+ {forge.GetForceBonus(forge.Level + 1) - forge.GetForceBonus(forge.Level)}");
        _infoItems = new List<UpgradeInfoItem> {forceObj};
    }

    private void ShowQuarryUpgradeInfo(IQuarry quarry)
    {
        var protectionObj = Recycler.Get<UpgradeInfoItem>();
        protectionObj.SetData("Бонус к защите юнитов при обороне",
            quarry.GetProtectionBonus(quarry.Level).ToString(CultureInfo.InvariantCulture),
            $"+ {quarry.GetProtectionBonus(quarry.Level + 1) - quarry.GetProtectionBonus(quarry.Level)}");
        _infoItems = new List<UpgradeInfoItem> {protectionObj};
    }

    private void ShowWorkShopUpgradeInfo(IWorkshop workShop)
    {
        var debuffObj = Recycler.Get<UpgradeInfoItem>();
        debuffObj.SetData("Бонус к защите юнитов при атаке",
            workShop.GetDebuffProtectionBonus(workShop.Level).ToString(CultureInfo.InvariantCulture),
            "+ " + (workShop.GetDebuffProtectionBonus(workShop.Level + 1) - workShop.GetDebuffProtectionBonus(workShop.Level))
            .ToString(CultureInfo.InvariantCulture));
        _infoItems = new List<UpgradeInfoItem> {debuffObj};
    }

    private void OnExitButtonClick()
    {
        Gameplay.SetPause(false);
        ClearData();
        CloseLast();
    }

    private void OnUpgradeButtonClick()
    {
        if (TargetCell.Building.Level <= 4)
        {
            if (ShopManager.Customers.Find(c => c.TeamID == 1).Money > int.Parse(_cost.Value))
                TargetCell.CellView.ShowDollar();

            ShopManager.Upgrade(TargetCell.Building);
        }

        Gameplay.SetPause(false);
        ClearData();
        CloseLast();
    }

    private void ClearData()
    {
        LevelManager.DestroyObjectsOfType<UpgradeInfoItem>();
        _infoItems.Clear();
        _verticalListView.Clear();
    }
}
