using System;
using Data;
using FuryLion.UI;
using Interfaces;
using UnityEngine;
using Views;

namespace Entities
{
    public class CapitalEntity : BuildingEntity<CapitalData>, IFarm, IUnitController, IBonusDependent, ICapital, IUpdatable, 
        IBankable, IBoostable
    {
        private CapitalView _capitalView;

        private float _baseForce;
        private float _baseProtection;
        
        private float _militaryReproduction;
        private float _armyReproduction;
        private float _protection;
        private float _force;

        private float _baseMilitaryTimeScale;
        private float _militaryTimeScale;
        private float _passMilitaryTime;
        
        private float _baseArmyTimeScale;
        private float _armyTimeScale;
        private float _passArmyTime;

        private float _moneyTimeScale = 1;
        private float _passMoneyTime;

        private float _boost = 1;
        
        public int MaxMilitaryCount = 20;
        
        public event Action<IBankable> Updated;

        public float Boost
        {
            get => _boost;
            set
            {
                _militaryTimeScale = _militaryTimeScale * _boost / value;
                _armyTimeScale = _armyTimeScale * _boost / value;
                _moneyTimeScale = _moneyTimeScale * _boost / value;
                _boost = value;
            }
        }

        public float BaseMilitaryReproduction { get; private set; }

        public float BaseArmyReproduction { get; private set; }
        
        private float MilitaryReproduction
        {
            get => _militaryReproduction;
            set
            {
                _militaryTimeScale = value * (1 - Data.Level * 0.1f);
                _militaryReproduction = value;
            }
        }
        
        private float ArmyReproduction
        {
            get => _armyReproduction;
            set
            {
                _armyTimeScale = value * (1 - Data.Level * 0.1f);
                _armyReproduction = value;
            }
        }

        public int ArmyCount => Data.ArmyCount;

        public CapitalEntity(CapitalData data) : base(data)
        {
            ChangedTeamID += (b, teamID, newTeamID) => OnChangedTeamID(newTeamID);
            OnChangedTeamID(TeamID);
            SetUp();
            if (TeamID == 0)
                data.ArmyCount = 0;

            if (TeamID == 1)
            {
                var transform1 = CameraManager.GameCamera.transform;
                transform1.position = new Vector3(data.Position.x, data.Position.y, transform1.position.z);
            }
            
            _capitalView = Recycler.Get<CapitalView>();
            _capitalView.transform.position = data.Position;
            _capitalView.BuildingEntity = this;
            _capitalView.SetLevel(data.Level);
            _capitalView.SetArmyCount(data.ArmyCount);
            _capitalView.SetMilitaryCount(data.MilitaryCount);
            _capitalView.SetLineRendererSettings();
        }
        
        private void OnChangedTeamID(int newTeamID)
        {
            BaseMilitaryReproduction = newTeamID == 1 ? 
                LevelManager.PlayerData.BaseMilitaryReproduction : 
                AIAcademy.MilitaryReproduction;
            BaseArmyReproduction = newTeamID == 1 ? 
                LevelManager.PlayerData.BaseArmyReproduction : 
                AIAcademy.ArmyReproduction;
            _baseForce = newTeamID == 1 ? LevelManager.PlayerData.BaseForce : AIAcademy.Force;
            _baseProtection = newTeamID == 1 ? LevelManager.PlayerData.BaseProtection : AIAcademy.Protection;
        }

        private void SetUp()
        {
            MilitaryReproduction = BaseMilitaryReproduction;
            ArmyReproduction = BaseArmyReproduction;
            _force = _baseForce;
            _protection = _baseProtection;
        }

        public override void Dispose()
        {
            base.Dispose();
            SetUp();
        }

        public void OnUpdate(float deltaTime)
        {
            _passMilitaryTime += deltaTime;
            if (_passMilitaryTime >= _militaryTimeScale)
            {
                if (Data.MilitaryCount > MaxMilitaryCount * Data.Level / 2)
                    UpdateMilitaryCount(Data.MilitaryCount - 1);
                else 
                    UpdateMilitaryCount(Data.MilitaryCount + 1);
                
                _passMilitaryTime = 0;
            }

            _passArmyTime += deltaTime;
            if (_passArmyTime >= _armyTimeScale)
            {
                if (TeamID != 0)
                    UpdateArmyCount(Data.ArmyCount + 1);
                
                _passArmyTime = 0;
            }
            
            _passMoneyTime += deltaTime;
            if (_passMoneyTime >= _moneyTimeScale)
            {
                Updated?.Invoke(this);
                _passMoneyTime = 0;
            }
        }

        public void AddReproductionBonus(float bonus)
        {
            MilitaryReproduction -= bonus;
            ArmyReproduction -= bonus;
        }

        public void SetActiveLine(bool check)
        {
            _capitalView.SetActiveLine(check);
        }

        public void SetLineEndPos(Vector3 pos)
        {
            _capitalView.SetLineEndPos(pos);
        }

        public void SetActiveOutLine(bool active) => _capitalView.SetActiveOutLine(active);

        public void AddSpeedBonus(float bonus) { }
        
        public void AddForceBonus(float bonus)
        {
            _force += bonus;
        }

        public void AddProtectionBonus(float bonus)
        {
            _protection += bonus;
        }

        public void AddDebuffProtectionBonus(float bonus) { }

        public float GetReproductionBonus(int lvl)
        {
            return Data.Farm.ReproductionBonus * lvl;
        }

        public int GetCurrency(int lvl)
        {
            return Data.Mine.CoinEarningBonus * lvl;
        }

        public void OnCrash(Collider2D other)
        {
            if (other.gameObject.TryGetComponent<UnitView>(out var unit))
            {
                var unitEntity = unit.UnitEntity;
                if (unitEntity.TargetPosition != Data.Position)
                    return;

                if (unitEntity.TeamID == TeamID)
                    UpdateArmyCount(Data.ArmyCount + 1);
                else
                    StartBattle(unitEntity);
                
                unitEntity.Crash();
            }
        }

        private void StartBattle(UnitEntity unit)
        {
            var unitForce = unit.Force + unit.DebuffProtection;
            var armyForce =  Data.ArmyCount * (_force + _protection);
            var militaryForce = Data.MilitaryCount * (_force + _protection);

            armyForce -= militaryForce <= 0 && armyForce > 0 ? unit.Force + unitForce : (unit.Force + unitForce) / 2;
            militaryForce -= armyForce <= 0 && militaryForce > 0 ? unit.Force + unitForce : (unit.Force + unitForce) / 2;

            if (militaryForce > 0 && armyForce <= 0)
                TeamID = 0;

            if (militaryForce <= 0 && armyForce <= 0)
                TeamID = unit.TeamID;
            
            UpdateArmyCount((int) Mathf.Abs(armyForce / (_force + _protection)));
            UpdateMilitaryCount((int) Mathf.Abs(militaryForce / (_force + _protection)));
        }

        protected override void OnChangeLevel(int lvl)
        {
            base.OnChangeLevel(lvl);
            _capitalView.SetLevel(lvl);
        }
        
        public void UpdateArmyCount(int unitCount)
        {
            Data.ArmyCount = unitCount;
            _capitalView.SetArmyCount(unitCount);
        }

        private void UpdateMilitaryCount(int militaryCount)
        {
            Data.MilitaryCount = militaryCount;
            _capitalView.SetMilitaryCount(militaryCount);
        }
    }
}
