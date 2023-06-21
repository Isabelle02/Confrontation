using Core;

public interface IBonusDependent : IActor
{
    public int TeamID { get; set; }
    public void Dispose();
    public void AddSpeedBonus(float bonus);
    public void AddForceBonus(float bonus);
    public void AddReproductionBonus(float bonus);
    public void AddProtectionBonus(float bonus);
    public void AddDebuffProtectionBonus(float bonus);
}