using UnityEngine;

namespace Seaborn.Hunting
{
    public interface IHarpoonTarget
    {
        bool IsHarvested { get; }

        void ApplyHarpoonHit(
            float damage,
            Vector3 hitPoint,
            GameObject hunter);
    }
}
