using UnityEngine;

namespace Interfaces
{
    public interface IUnitController : IBuilding
    {
        public int ArmyCount { get; }
        public void UpdateArmyCount(int unitCount);
        public void SetActiveLine(bool check);
        public void SetLineEndPos(Vector3 pos);
        public void SetActiveOutLine(bool active);
    }
}