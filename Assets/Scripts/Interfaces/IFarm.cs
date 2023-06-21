namespace Interfaces
{
    public interface IFarm : IBuilding
    {
        public float GetReproductionBonus(int lvl);
    }
}