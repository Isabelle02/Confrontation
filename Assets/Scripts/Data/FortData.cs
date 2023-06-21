using System;
using Core;
using Entities;

namespace Data
{
    [Serializable]
    public class FortData : BuildingData
    {
        public int ArmyCount;

        protected override BaseEntity CreateEntity(IWorld world)
        {
            return new FortEntity(this);
        }
    }
}
