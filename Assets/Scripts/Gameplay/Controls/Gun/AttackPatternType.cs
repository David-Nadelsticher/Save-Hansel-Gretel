namespace Gameplay.Controls.Gun
{
    
    [System.Serializable] public enum AttackPatternType
    {
        None = 0,
        Single = 1,
        Spread = 2,
        Circular = 3,
    }

    [System.Serializable] public class AttackPatternParams
    {
        public float projectileSpeed = 10f;
        public int projectilesPerSpread = 8;
        public float spreadAngle = 90f;
    }
    [System.Serializable] public class AttackPatternEntity
    {
       public AttackPatternType attackPatternType;
        public AttackPatternParams attackPatternParams;
    }
}