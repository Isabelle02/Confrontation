using Data;
using FuryLion.UI;
using Interfaces;
using UnityEngine;
using Views;

namespace Entities
{
    public class BarracksEntity : BuildingEntity<BarracksData>, IUnitController, IBonusDependent, IBarracks, IUpdatable, IBoostable
    {
        private BarracksView _barracksView;
        
        private float _armyReproduction;

        private float _timeScale;
        private float _passTime;

        private float _boost = 1;

        public float Boost
        {
            get => _boost;
            set
            {
                _timeScale = _timeScale * _boost / value;
                _boost = value;
            }
        }
        
        public int ArmyCount => Data.ArmyCount;

        public float BaseArmyReproduction { get; private set; }

        private float ArmyReproduction
        {
            get => _armyReproduction;
            set
            {
                _timeScale = value * (1 - Data.Level * 0.1f);
                _armyReproduction = value;
            }
        }

        public BarracksEntity(BarracksData data) : base(data)
        {
            ChangedTeamID += (b, teamID, newTeamID) => OnChangedTeamID(newTeamID);
            OnChangedTeamID(TeamID);
            SetUp();
            
            _barracksView = Recycler.Get<BarracksView>();
            _barracksView.transform.position = data.Position;
            _barracksView.BuildingEntity = this;
            _barracksView.SetLevel(data.Level);
            _barracksView.SetUnitCount(data.ArmyCount);
            _barracksView.SetLineRendererSettings();
        }

        private void OnChangedTeamID(int newTeamID)
        {
            BaseArmyReproduction = newTeamID == 1 ? 
                LevelManager.PlayerData.BaseArmyReproduction : 
                AIAcademy.ArmyReproduction;
        }
        
        public void OnUpdate(float deltaTime)
        {
            _passTime += deltaTime;
            if (_passTime >= _timeScale)
            {
                if (TeamID != 0)
                    UpdateArmyCount(Data.ArmyCount + 1);
                
                _passTime = 0;
            }
        }
        
        protected override void OnChangeLevel(int lvl)
        {
            base.OnChangeLevel(lvl);
            _barracksView.SetLevel(lvl);
        }

        private void SetUp()
        {
            ArmyReproduction = BaseArmyReproduction;
        }

        public override void Dispose()
        {
            base.Dispose();
            SetUp();
        }

        public void AddReproductionBonus(float bonus)
        {
            ArmyReproduction -= bonus;
        }

        public void SetActiveLine(bool check) => _barracksView.SetActiveLine(check);
        
        public void SetLineEndPos(Vector3 pos) => _barracksView.SetLineEndPos(pos);

        public void SetActiveOutLine(bool active) => _barracksView.SetActiveOutLine(active);

        public void AddSpeedBonus(float bonus) { }
        
        public void AddForceBonus(float bonus) { }

        public void AddProtectionBonus(float bonus) { }

        public void AddDebuffProtectionBonus(float bonus) { }

        public void UpdateArmyCount(int unitCount)
        {
            Data.ArmyCount = unitCount;
            _barracksView.SetUnitCount(unitCount);
        }
    }
}
