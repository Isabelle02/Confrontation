using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FuryLion.UI;
using Core;
using Data;
using Entities;
using Interfaces;
using Systems;
using Views;

public class Gameplay : MonoBehaviour
{
    [SerializeField] private LayerMask _buildingsMask;
    [SerializeField] private LayerMask _cellMask;
    
    [SerializeField] private float _minZoom = 4f;
    [SerializeField] private float _maxZoom = 8f;
    
    private static Gameplay _instance;
    
    private World _currentWorld;

    private bool _isPaused = true;
    private bool _isDragged;
    private Vector3 _mouseDownPos;
    
    private Vector2 _center;
    private Vector3 _size;

    private BuildingPanel _buildingPanel;

    private readonly List<IUnitController> _selectedBuildings = new List<IUnitController>();

    private void Awake()
    {
        _instance = this;
    }

    public static async void Init(int lvlIndex)
    {
        _instance._buildingPanel = Recycler.Get<BuildingPanel>();

        var config = LevelManager.GetLevelConfig(lvlIndex);
        _instance._center = config.Center;
        _instance._size = config.Size;

        _instance._currentWorld = new World();
        var data = LevelManager.LoadWorld(lvlIndex);

        ShopManager.InitItems(LevelManager.BuildingsConfig);
        var aiControllers = new List<AIController>();
        foreach (var o in data.Objects)
        {
            if (!(o is TeamData t) || t.TeamID == 0)
                continue;
            
            if (t.TeamID == 1)
            {
                var customer = new CustomerController(t.TeamID, t.Money, t.Mana);
                ShopManager.AddCustomer(customer);
                GamePage.Init(customer, 1);
                customer.UpdatedMoney += GamePage.OnUpdateMoney;
                customer.UpdatedMana += GamePage.OnUpdateMana;
                customer.UpdatedMoney += _instance._buildingPanel.SwitchColor;
                customer.UpdatedMoney += c => t.Money = c.Money;
                customer.UpdatedMana += c => t.Mana = c.Mana;
                continue;
            }

            var ai = new AIController(t.TeamID, t.Money, t.Mana,
                new List<float> {5f, 6.1f, 5.6f, 7.2f, 7.5f, 8f, 11f, 10.5f, 10.8f, 9f, 9.8f, 9.7f, 9.6f});
            ai.UpdatedMoney += c => t.Money = c.Money;
            ai.UpdatedMana += c => t.Mana = c.Mana;
            ai.SendUnits += _instance.SendUnits;
            aiControllers.Add(ai);
            ShopManager.AddCustomer(ai);
        }
        
        var bonusSystem = new BonusSystem();
        var updateSystem = new UpdateSystem();
        var currencySystem = new CurrencySystem(ShopManager.Customers);
        var boostSystem = new BoostSystem();
        var teamChangeSystem = new TeamChangeSystem();
        var warFogSystem = new WarFogSystem();

        teamChangeSystem.PassedLevel += () => OnEndedLevel(true);
        teamChangeSystem.LostLevel += () => OnEndedLevel(false);
        teamChangeSystem.ChangedTeamID += _instance.OnChangedTeamID;
        foreach (var ai in aiControllers)
        {
            updateSystem.AddUpdatableObject(ai);
            teamChangeSystem.ChangedTeamID += ai.OnChangedTeamID;
        }

        GamePage.UpdatedBoost += boostSystem.OnBoost;
        
        _instance._currentWorld.Init(data, _instance.transform, bonusSystem, teamChangeSystem, 
            warFogSystem, updateSystem, currencySystem, boostSystem);

        foreach (var c in ShopManager.Customers) 
            warFogSystem.UpdateAvailable(c.TeamID);
        
        UnitUtility.MoveAll();
        
        await updateSystem.Update();
    }

    private static void OnEndedLevel(bool isWin)
    {
        SetPause(true);
        Deactivate();
        LevelManager.ResetLevel();
        EndGamePage.IsWin = isWin;
        PageManager.Open<EndGamePage>();
    }

    public static void SetPause(bool isPaused)
    {
        _instance._isPaused = isPaused;
        _instance._buildingPanel?.Hide();
        _instance._currentWorld.GetSystem<UpdateSystem>()?.SetPause(isPaused);
        UnitUtility.SetPause(isPaused);
    }

    public static void Deactivate()
    {
        foreach (var building in _instance._selectedBuildings)
            building.SetActiveOutLine(false);
        
        _instance._selectedBuildings.Clear();
        _instance._currentWorld.Deactivate();
        UnitUtility.Clear();
        ShopManager.Clear();
    }

    private void Update()
    {
        if (_isPaused)
        {
            HandleButton();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            _mouseDownPos = MouseManager.GetMousePosition(0, CameraManager.GameCamera);
            SelectBuilding();
        }

        if (Input.touchCount == 2 && _selectedBuildings.Count == 0)
            CameraManager.CameraZoom(Input.GetTouch(0), Input.GetTouch(1), CameraManager.GameCamera, _minZoom, _maxZoom);

        if (Input.GetMouseButtonUp(0))
        {
            if (_isDragged)
            {
                var targetBuilding = MouseManager.GetObject<BuildingView>(_buildingsMask, CameraManager.GameCamera);
                if ((targetBuilding is SettlementView && !(targetBuilding is FortView) ||
                    targetBuilding is FortView && targetBuilding.BuildingEntity.TeamID == 1) && 
                    ShopManager.Customers.Find(c => c.TeamID == 1)
                        .AvailableCells.Select(c => c.Building).Contains(targetBuilding.BuildingEntity))
                {
                    SendUnits(_selectedBuildings, targetBuilding.BuildingEntity);
                    HideLines();
                    foreach (var building in _selectedBuildings)
                        building.SetActiveOutLine(false);
                    
                    _selectedBuildings.Clear();
                }
                
                HideLines();
                _isDragged = false;
            }
            else
            {
                HideLines();
                foreach (var building in _selectedBuildings)
                    building.SetActiveOutLine(false);
                
                _selectedBuildings.Clear();
                if (HandleButton() == null)
                {
                    var button = MouseManager.GetObject<BaseButton>(_buildingsMask, CameraManager.GameCamera);
                    if (button != null)
                        button.OnClick();

                    HandleBuildingPanel();
                }
            }
            
        }

        if (Input.GetMouseButton(0))
        {
            if (_mouseDownPos != MouseManager.GetMousePosition(0, CameraManager.GameCamera))
            {
                _isDragged = true;
                if(_selectedBuildings.Count > 0)
                    SelectBuilding();
                
                DrawLines();
                if (_selectedBuildings.Count == 0)
                    CameraManager.CameraMove(CameraManager.GameCamera, _mouseDownPos, _center, _size);
            }
        }
        
        //TODO закомментировать перед сборкой проекта
        CameraManager.Zoom(CameraManager.GameCamera, Input.GetAxis("Mouse ScrollWheel"), _minZoom, _maxZoom);
    }

    public static BaseEntity CreateNewObject(ObjectData data)
    {
        return _instance._currentWorld.CreateNewObject(data);
    }

    private BaseButton HandleButton()
    {
        BaseButton baseButton = null;
        var popUpObj = MouseManager.GetObject<Element>(CameraManager.PopUpMask, CameraManager.PopUpCamera);
        var messageBoxOnj = MouseManager.GetObject<Element>(CameraManager.MessageBoxMask, CameraManager.MessageBoxCamera);
        var pageObj = MouseManager.GetObject<Element>(CameraManager.PageMask, CameraManager.PageCamera);
        var academyObj = MouseManager.GetObject<Element>(CameraManager.AcademyMask, CameraManager.AcademyCamera);

        if (popUpObj != null)
        {
            if (IsButtonClick(CameraManager.PopUpCamera) && popUpObj is BaseButton popUpButton)
                baseButton = popUpButton;
        }
        else if (messageBoxOnj != null)
        {
            if (IsButtonClick(CameraManager.MessageBoxCamera) && messageBoxOnj is BaseButton messageBoxButton)
                baseButton = messageBoxButton;
        }
        else if (pageObj != null)
        {
            if (IsButtonClick(CameraManager.PageCamera) && pageObj is BaseButton pageButton)
                baseButton = pageButton;
        }
        else if (academyObj != null)
        {
            if (IsButtonClick(CameraManager.AcademyCamera) && academyObj is BaseButton academyButton)
                baseButton = academyButton;
        }

        baseButton?.OnClick();
        return baseButton;
    }

    private bool IsButtonClick(tk2dCamera curCamera)
    {
        if (Input.GetMouseButtonDown(0)) 
            _mouseDownPos = MouseManager.GetMousePosition(0, curCamera);

        if (Input.GetMouseButton(0) 
            && _mouseDownPos != MouseManager.GetMousePosition(0, curCamera))
            _isDragged = true;

        if (Input.GetMouseButtonUp(0))
            if (_isDragged)
                _isDragged = false;
            else
                return true;

        return false;
    }

    private void HandleBuildingPanel()
    {
        if (_buildingPanel.gameObject.activeSelf)
        {
            _buildingPanel.Hide();
            return;
        }

        var buildingView = MouseManager.GetObject<BuildingView>(_buildingsMask, CameraManager.GameCamera);
        var cellView = MouseManager.GetObject<CellView>(_cellMask, CameraManager.GameCamera);
        if (buildingView != null)
        {
            if ( buildingView.BuildingEntity.TeamID != 1 || buildingView is FortView)
                return;

            UpgradePopUp.TargetCell = cellView.CellEntity;
            PopupManager.Open<UpgradePopUp>();
        }
        else
        {
            if (cellView != null)
                _buildingPanel.Show(cellView);
        }
    }

    private void SelectBuilding()
    {
        var building = MouseManager.GetObject<BuildingView>(_buildingsMask, CameraManager.GameCamera);
        if (building == null)
            return;

        if (!ShopManager.Customers.Find(c => c.TeamID == 1).
            AvailableCells.Select(c => c.Building).Contains(building.BuildingEntity))
            return;
        
        if (building.BuildingEntity.TeamID != 1)
            return;

        if (!(building.BuildingEntity is IUnitController unitController)) 
            return;

        if (!_selectedBuildings.Contains(unitController))
        {
            _selectedBuildings.Add(unitController);
            unitController.SetActiveOutLine(true);
        }
    }

    private void OnChangedTeamID(List<ICell> cells)
    {
        foreach (var cell in cells)
        {
            if (cell == _buildingPanel.Target || cell.Building == _buildingPanel.Target)
                _buildingPanel.Hide();
            
            if (!(cell.Building is IUnitController unitController))
                continue;

            unitController.SetActiveLine(false);
            if (_selectedBuildings.Contains(unitController))
            {
                _selectedBuildings.Remove(unitController);
                unitController.SetActiveOutLine(false);
            }
        }
    }

    private void SendUnits(List<IUnitController> selectedBuildings, IBuilding targetBuilding)
    {
        var teamID = 0;
        var unitCount = 0;
        foreach (var b in selectedBuildings)
        {
            if (b == targetBuilding)
                continue;
                
            teamID = b.TeamID;
            switch (b)
            {
                case IFort fort:
                case IBarracks barracks:
                    unitCount = b.ArmyCount;
                    break;
                case ISettlement settlement: 
                    unitCount = b.ArmyCount / 2;
                    break;
            }
            
            b.UpdateArmyCount(b.ArmyCount - unitCount);
            for (var i = 0; i < unitCount; i++)
            {
                var unitData = new UnitData(teamID, b.Position, targetBuilding.Position);
                var unit = _currentWorld.CreateNewObject(unitData);
                if (!(unit is UnitEntity u))
                    continue;
                
                u.Move();
            }
        }
        
        if (teamID == 1)
            Vibration.Vibrate(100);
    }

    private void HideLines()
    {
        foreach (var building in _selectedBuildings)
            building.SetActiveLine(false);
    }

    private void DrawLines()
    {
        foreach (var building in _selectedBuildings)
        {
            building.SetActiveLine(true);
            building.SetLineEndPos(MouseManager.GetMousePosition(4, CameraManager.GameCamera));
        }
    }
}
