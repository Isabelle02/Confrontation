using Data;
using FuryLion.UI;
using Interfaces;
using UnityEngine;
using Views;

namespace Entities
{
    public class SettlementEntity : BuildingEntity<SettlementData>, IUnitController, IBonusDependent, ISettlement, IUpdatable, IBoostable
    {
        private readonly SettlementView _settlementView;

        private float _baseForce;
        private float _baseProtection;
        
        private float _militaryReproduction;
        private float _protection;

        private float _timeScale;
        private float _passTime;

        private float _force;
        
        private float _boost = 1;
        
        public int MaxMilitaryCount = 20;

        public float Boost
        {
            get => _boost;
            set
            {
                _timeScale = _timeScale * _boost / value;
                _boost = value;
            }
        }
        
        public float BaseMilitaryReproduction { get; private set; }
        
        private float MilitaryReproduction
        {
            get => _militaryReproduction;
            set
            {
                _timeScale = value * (1 - Data.Level * 0.1f);
                _militaryReproduction = value;
            }
        }

        public int ArmyCount => Data.ArmyCount;

        public SettlementEntity(SettlementData data) : base(data)
        {
            ChangedTeamID += (b, teamID, newTeamID) => OnChangedTeamID(newTeamID);
            OnChangedTeamID(TeamID);
            SetUp();
            if (TeamID == 0)
                data.ArmyCount = 0;

            _settlementView = Recycler.Get<SettlementView>();
            _settlementView.transform.position = data.Position;
            _settlementView.BuildingEntity = this;
            _settlementView.SetLevel(data.Level);
            _settlementView.SetArmyCount(data.ArmyCount);
            _settlementView.SetMilitaryCount(data.MilitaryCount);
            _settlementView.SetLineRendererSettings();
        }
        
        private void OnChangedTeamID(int newTeamID)
        {
            BaseMilitaryReproduction = newTeamID == 1 ? 
                LevelManager.PlayerData.BaseMilitaryReproduction : 
                AIAcademy.MilitaryReproduction;
            _baseForce = newTeamID == 1 ? LevelManager.PlayerData.BaseForce : AIAcademy.Force;
            _baseProtection = newTeamID == 1 ? LevelManager.PlayerData.BaseProtection : AIAcademy.Protection;
        }

        private void SetUp()
        {
            MilitaryReproduction = BaseMilitaryReproduction;
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
            _passTime += deltaTime;
            if (_passTime >= _timeScale)
            {
                if (Data.MilitaryCount > MaxMilitaryCount * Data.Level / 2)
                    UpdateMilitaryCount(Data.MilitaryCount - 1);
                else 
                    UpdateMilitaryCount(Data.MilitaryCount + 1);
                
                _passTime = 0;
            }
        }

        public void AddReproductionBonus(float bonus)
        {
            MilitaryReproduction -= bonus;
        }

        public void SetActiveLine(bool check)
        {
            _settlementView.SetActiveLine(check);
        }

        public void SetLineEndPos(Vector3 pos)
        {
            _settlementView.SetLineEndPos(pos);
        }

        public void SetActiveOutLine(bool active) => _settlementView.SetActiveOutLine(active);

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
            _settlementView.SetLevel(lvl);
        }
        
        public void UpdateArmyCount(int unitCount)
        {
            Data.ArmyCount = unitCount;
            _settlementView.SetArmyCount(unitCount);
        }

        private void UpdateMilitaryCount(int militaryCount)
        {
            Data.MilitaryCount = militaryCount;
            _settlementView.SetMilitaryCount(militaryCount);
        }
    }
}
