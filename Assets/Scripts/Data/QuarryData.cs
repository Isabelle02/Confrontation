using Core;
using Entities;

namespace Data
{
    public class QuarryData : BuildingData
    {
        public float ProtectionBonus = 0.5f;

        protected override BaseEntity CreateEntity(IWorld world)
        {
            return new QuarryEntity(this);
        }
    }
}