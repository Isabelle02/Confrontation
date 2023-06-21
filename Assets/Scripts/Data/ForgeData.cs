using Core;
using Entities;

namespace Data
{
    public class ForgeData : BuildingData
    {
        public float ForceBonus = 0.05f;
    
        protected override BaseEntity CreateEntity(IWorld world)
        {
            return new ForgeEntity(this);
        }
    }
}