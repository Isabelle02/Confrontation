using Data;
using FuryLion.UI;
using Interfaces;
using UnityEngine;
using Views;

namespace Entities
{
    public class FortEntity : BuildingEntity<FortData>, IUnitController, IFort
    {
        private FortView _fortView;

        public int ArmyCount => Data.ArmyCount;

        public FortEntity(FortData data) : base(data)
        {
            _fortView = Recycler.Get<FortView>();
            _fortView.transform.position = data.Position;
            _fortView.BuildingEntity = this;
            _fortView.SetLevel(data.Level);
            _fortView.SetArmyCount(data.ArmyCount);
            _fortView.SetLineRendererSettings();
        }

        protected override void OnChangeLevel(int lvl)
        {
            base.OnChangeLevel(lvl);
            _fortView.SetLevel(lvl);
        }

        public void SetActiveLine(bool check) => _fortView.SetActiveLine(check);

        public void SetLineEndPos(Vector3 pos) => _fortView.SetLineEndPos(pos);

        public void SetActiveOutLine(bool active) => _fortView.SetActiveOutLine(active);

        public void UpdateArmyCount(int unitCount)
        {
            Data.ArmyCount = unitCount;
            _fortView.SetArmyCount(unitCount);
        }

        public void OnCrash(Collider2D other)
        {
            if (other.gameObject.TryGetComponent<UnitView>(out var unit))
            {
                var unitEntity = unit.UnitEntity;
                if (unitEntity.TargetPosition != Data.Position)
                    return;

                UpdateArmyCount(Data.ArmyCount + 1);
                unitEntity.Crash();
            }
        }
    }
}
