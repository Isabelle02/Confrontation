using System;
using System.Collections.Generic;
using System.Linq;
using Entities;
using Interfaces;
using UnityEngine;
using Random = UnityEngine.Random;

public class AIController : CustomerController, IUpdatable
{
    private readonly List<IUnitController> _selectedBuildings = new List<IUnitController>();
    private readonly Dictionary<BuildingType, int> _buildingPriorities;

    private int _money;
    private int _mana;
    private IUnitController _target;

    private readonly List<float> _timeStepScaleRange;

    private float _timeStepScale;
    private float _passStepTime;
    
    public event Action<List<IUnitController>, IUnitController> SendUnits;

    public AIController(int teamID, int money, int mana, List<float> timeScaleRange) : base(teamID, money, mana)
    {
        _buildingPriorities = new Dictionary<BuildingType, int>()
        {
            {BuildingType.Barracks, 4},
            {BuildingType.Mine, 3},
            {BuildingType.Farm, 2},
            {BuildingType.Forge, 1},
            {BuildingType.Stable, 1},
            {BuildingType.Capital, 3},
            {BuildingType.Settlement, 1},
            {BuildingType.WizardTower, 3},
            {BuildingType.Quarry, 2},
            {BuildingType.Workshop, 2},
            {BuildingType.Fort, 1},
        };

        _timeStepScaleRange = timeScaleRange;
        _timeStepScale = _timeStepScaleRange[Random.Range(0, _timeStepScaleRange.Count)];
        UpdatedMoney += c => HandleBuilding();
    }

    public void OnUpdate(float deltaTime)
    {
        _passStepTime += deltaTime;
        if (_passStepTime >= _timeStepScale)
        {
            OnStep();
            if (_target != null)
            {
                StepFixes();
                SendUnits?.Invoke(_selectedBuildings, _target);
            }

            _target = null;
            _selectedBuildings.Clear();
            _timeStepScale = _timeStepScaleRange[Random.Range(0, _timeStepScaleRange.Count)];
            _passStepTime = 0;
        }
    }
    
    public void OnChangedTeamID(List<ICell> cells)
    {
        foreach (var c in cells)
        {
            if (!(c.Building is IUnitController unitController))
                continue;

            if (_selectedBuildings.Contains(unitController))
                _selectedBuildings.Remove(unitController);
        }
    }

    private void HandleBuilding()
    {
        var cells = AvailableCells.FindAll(c => c.TeamID == TeamID);
        var max = _buildingPriorities.Values.ToList().Max();
        var maxes = _buildingPriorities.Where(b => b.Value == max).ToList();
        var buildingType = maxes[Random.Range(0, maxes.Count)].Key;
        var buildingCells = cells.FindAll(c => c.Building != null);
        var isBuying = Convert.ToBoolean(Random.Range(0, 2));
        if (!isBuying || buildingCells.Count >= cells.Count * 0.5 || cells.FindAll(c => c.Building == null).Count == cells.Count)
        {
            var upgradingCells= buildingType switch
            {
                BuildingType.Barracks => buildingCells.FindAll(c => c.Building.GetType() == typeof(BarracksEntity)),
                BuildingType.Mine => buildingCells.FindAll(c => c.Building.GetType() == typeof(MineEntity)),
                BuildingType.Farm => buildingCells.FindAll(c => c.Building.GetType() == typeof(FarmEntity)),
                BuildingType.Forge => buildingCells.FindAll(c => c.Building.GetType() == typeof(ForgeEntity)),
                BuildingType.Stable => buildingCells.FindAll(c => c.Building.GetType() == typeof(StableEntity)),
                BuildingType.Quarry => buildingCells.FindAll(c => c.Building.GetType() == typeof(QuarryEntity)),
                BuildingType.Workshop => buildingCells.FindAll(c => c.Building.GetType() == typeof(WorkshopEntity)),
                BuildingType.Capital => buildingCells.FindAll(c => c.Building.GetType() == typeof(CapitalEntity)),
                BuildingType.Settlement => buildingCells.FindAll(c => c.Building.GetType() == typeof(SettlementEntity)),
                BuildingType.WizardTower => buildingCells.FindAll(c => c.Building.GetType() == typeof(WizardTowerEntity)),
                _ => new List<ICell>()
            };

            if (upgradingCells.Count == 0) 
                return;
            
            var cell = upgradingCells[Random.Range(0, upgradingCells.Count)];
            if (ShopManager.ActOnBuilding(cell.Building, 
                type => ShopManager.GetCost(type, cell.Building.Level)) <= Money * 0.6)
            {
                if (ShopManager.Upgrade(cell.Building))
                    _buildingPriorities[buildingType]--;
            }
        }
        else
        {
            var buyingCells = cells.FindAll(c => c.Building == null);
            if (buyingCells.Count == 0)
                return;
            
            var cell = buyingCells[Random.Range(0, buyingCells.Count)];
            if (ShopManager.GetCost(buildingType) <= Money * 0.7)
            {
                if (ShopManager.Buy(cell, buildingType))
                    _buildingPriorities[buildingType]--;
            }
        }
    }

    private void StepFixes()
    {
        if (_target is ISettlement settlement && settlement.TeamID == TeamID && !(settlement is ICapital))
            for (var i = 0; i < _selectedBuildings.Count; i++)
            {
                if (_selectedBuildings[i] is SettlementEntity s && s.ArmyCount <= 10)
                {
                    _selectedBuildings.Remove(s);
                    continue;
                }

                if (_selectedBuildings[i] is CapitalEntity c && c.ArmyCount <= 25)
                    _selectedBuildings.Remove(c);
            }
    }

    private void OnStep()
    {
        var unitControllers = new List<IUnitController>();
        foreach (var c in AvailableCells)
            if (c.Building is IUnitController unitController)
                unitControllers.Add(unitController);
        
        var buildings = unitControllers.FindAll(b => b.TeamID == TeamID).ToList();
        var armyCount = 0f;
        var random = Random.Range(0, 2);
        foreach (var b in buildings.Where(b => random == 1 || b is IBarracks))
            switch (b)
            {
                case IBarracks barracks:
                    armyCount += b.ArmyCount;
                    _selectedBuildings.Add(b);
                    break;
                case IFort fort:
                    armyCount += b.ArmyCount;
                    _selectedBuildings.Add(b);
                    break;
                case ISettlement settlement:
                    if (settlement is CapitalEntity capital && capital.ArmyCount <= 15)
                        break;

                    if (settlement is SettlementEntity s && s.ArmyCount <= 10)
                        break;
                    
                    armyCount += Mathf.RoundToInt(b.ArmyCount / 2f);
                    _selectedBuildings.Add(b);
                    break;
            }

        var target = FindTarget(unitControllers, TeamID);
        _target = target;
    }
    
    private IUnitController FindTarget(List<IUnitController> buildings,int id)
    {
        var dictionary = new Dictionary<int, IUnitController>();
        var targetsNeutral = buildings.FindAll(b => 
            b.TeamID == 0 && !(b is IBarracks) && !(b is IFort));
        var targetsAIOwner = buildings.FindAll(b => 
            b.TeamID == id && !(b is IBarracks));
        var targetsEnemies = buildings.FindAll(b => 
            b.TeamID != 0 && b.TeamID != id && !(b is IBarracks) && !(b is IFort));
        
        if (targetsNeutral.Count != 0)
        {
            var settlementNeutral = GetTarget(targetsNeutral);
            dictionary.Add(settlementNeutral.ArmyCount, settlementNeutral);
        }
            
        if (targetsAIOwner.Count != 0)
        {
            var settlementAIOwner = GetTarget(targetsAIOwner);
            if (dictionary.Keys.All(k => k != settlementAIOwner.ArmyCount))
            {
                if (settlementAIOwner.ArmyCount < 15 || settlementAIOwner is ICapital)
                    dictionary.Add(settlementAIOwner.ArmyCount, settlementAIOwner);
            }
        }

        if (targetsEnemies.Count != 0)
        {
            var settlementEnemies = GetTarget(targetsEnemies);
            if (dictionary.Keys.All(k => k != settlementEnemies.ArmyCount))
                dictionary.Add(settlementEnemies.ArmyCount, settlementEnemies);
        }

        var minArmy = dictionary.Select(settlement => settlement.Key).Prepend(100).Min();
        
        foreach (var target in dictionary.Where(target => target.Value is ICapital))
        {
            _timeStepScale = 1f;
            return target.Value;
        }

        var result = dictionary.Where(s => s.Key == minArmy).ToList();
        
        return result.Count == 0 ? null : result.First(r => r.Key == minArmy).Value;
    }
    
    private IUnitController GetTarget(List<IUnitController> targets)
    {
        foreach (var target in targets)
        {
            switch (target)
            {
                case CapitalEntity capital when capital.TeamID == TeamID && capital.ArmyCount <= 20:
                    return target;
                case ICapital capital when capital.TeamID != TeamID:
                    return target;
            }
        }

        var minArmy = targets[0].ArmyCount;
        minArmy = targets.Select(t => t.ArmyCount).Prepend(minArmy).Min();
        var minArmySettlements = targets.Where(n => n.ArmyCount == minArmy).ToList();
        return minArmySettlements[Random.Range(0, minArmySettlements.Count - 1)];
    }
}
