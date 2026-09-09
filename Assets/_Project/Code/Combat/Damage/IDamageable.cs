namespace Seaborn.Combat.Damage
{
    public interface IDamageable
    {
        bool IsSunk { get; }

        void ApplyDamage(DamageInfo damageInfo);
    }
}
