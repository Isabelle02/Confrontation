public static class AIAcademy
{
    private static float _baseSpeed = 0.3f;
    private static float _baseForce = 1f;
    private static float _baseMilitaryReproduction = 4f;
    private static float _baseArmyReproduction = 2f;
    private static float _baseDebuffProtection;
    private static float _protection;

    public static float Speed => _baseSpeed * GetCoeff();
    public static float Force => _baseForce * GetCoeff();
    public static float MilitaryReproduction => _baseMilitaryReproduction - GetCoeff() / 10;
    public static float ArmyReproduction => _baseArmyReproduction - GetCoeff() / 10;
    public static float DebuffProtection => _baseDebuffProtection;
    public static float Protection => _protection;

    private static float GetCoeff() => (10 + LevelManager.CurrentLevel) / 10f;
}