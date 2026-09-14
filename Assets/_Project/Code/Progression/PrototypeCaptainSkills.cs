using System;
using Seaborn.Combat;
using Seaborn.Harbor;
using Seaborn.Hunting;
using Seaborn.Ship;
using UnityEngine;

namespace Seaborn.Progression
{
    public enum CaptainSkill
    {
        CannonMastery,
        Rangefinding,
        HarpoonMastery,
        HarpoonRigging,
        ReinforcedHull,
        FineSails
    }

    public enum SkillSpendResult
    {
        Completed,
        NotAtHarborOffice,
        NoSkillPoints,
        MaximumRank,
        PrerequisiteMissing
    }

    [DisallowMultipleComponent]
    public sealed class PrototypeCaptainSkills : MonoBehaviour
    {
        [SerializeField, Range(0, 3)] private int cannonMastery;
        [SerializeField, Range(0, 2)] private int rangefinding;
        [SerializeField, Range(0, 3)] private int harpoonMastery;
        [SerializeField, Range(0, 2)] private int harpoonRigging;
        [SerializeField, Range(0, 3)] private int reinforcedHull;
        [SerializeField, Range(0, 2)] private int fineSails;

        public event Action SkillsChanged;

        public int SpentPoints =>
            cannonMastery + rangefinding +
            harpoonMastery + harpoonRigging +
            reinforcedHull + fineSails;

        public int AvailablePoints =>
            progression != null
                ? Mathf.Max(
                    0,
                    progression.SkillPointsEarned -
                    SpentPoints)
                : 0;

        public bool CanSpendAtHarborOffice
        {
            get
            {
                PrototypeHarborDockingDirector docking =
                    PrototypeHarborDockingDirector.Instance;
                return Seaborn.World
                        .PrototypeExpeditionRegionDirector
                        .IsHarborScene &&
                    docking != null &&
                    docking.IsDockedAt(
                        PrototypeHarborStation.HarborOffice);
            }
        }

        private PrototypeCaptainProgression progression;
        private BroadsideController broadside;
        private HarpoonHuntingController harpoons;
        private ShipHealth health;
        private ShipMotor motor;

        public static PrototypeCaptainSkills EnsureAttached(
            Transform player)
        {
            if (player == null) return null;

            PrototypeCaptainSkills skills =
                player.GetComponent<
                    PrototypeCaptainSkills>();
            if (skills == null)
            {
                skills = player.gameObject.AddComponent<
                    PrototypeCaptainSkills>();
            }

            skills.Bind(player);
            return skills;
        }

        public int GetRank(CaptainSkill skill)
        {
            return skill switch
            {
                CaptainSkill.CannonMastery => cannonMastery,
                CaptainSkill.Rangefinding => rangefinding,
                CaptainSkill.HarpoonMastery => harpoonMastery,
                CaptainSkill.HarpoonRigging => harpoonRigging,
                CaptainSkill.ReinforcedHull => reinforcedHull,
                CaptainSkill.FineSails => fineSails,
                _ => 0
            };
        }

        public int GetMaximumRank(CaptainSkill skill)
        {
            return skill switch
            {
                CaptainSkill.CannonMastery => 3,
                CaptainSkill.Rangefinding => 2,
                CaptainSkill.HarpoonMastery => 3,
                CaptainSkill.HarpoonRigging => 2,
                CaptainSkill.ReinforcedHull => 3,
                CaptainSkill.FineSails => 2,
                _ => 0
            };
        }

        public bool MeetsPrerequisite(CaptainSkill skill)
        {
            return skill switch
            {
                CaptainSkill.Rangefinding =>
                    cannonMastery >= 1,
                CaptainSkill.HarpoonRigging =>
                    harpoonMastery >= 1,
                CaptainSkill.FineSails =>
                    reinforcedHull >= 1,
                _ => true
            };
        }

        public SkillSpendResult TrySpend(CaptainSkill skill)
        {
            if (!CanSpendAtHarborOffice)
                return SkillSpendResult.NotAtHarborOffice;
            if (GetRank(skill) >= GetMaximumRank(skill))
                return SkillSpendResult.MaximumRank;
            if (!MeetsPrerequisite(skill))
                return SkillSpendResult.PrerequisiteMissing;
            if (AvailablePoints <= 0)
                return SkillSpendResult.NoSkillPoints;

            SetRank(skill, GetRank(skill) + 1);
            ApplyModifiers(false);
            SkillsChanged?.Invoke();
            return SkillSpendResult.Completed;
        }

        public void Restore(
            int cannonDamage,
            int cannonRange,
            int harpoonDamage,
            int harpoonReload,
            int hull,
            int sails)
        {
            cannonMastery = Mathf.Clamp(cannonDamage, 0, 3);
            rangefinding = Mathf.Clamp(cannonRange, 0, 2);
            harpoonMastery = Mathf.Clamp(harpoonDamage, 0, 3);
            harpoonRigging = Mathf.Clamp(harpoonReload, 0, 2);
            reinforcedHull = Mathf.Clamp(hull, 0, 3);
            fineSails = Mathf.Clamp(sails, 0, 2);
            ApplyModifiers(false);
            SkillsChanged?.Invoke();
        }

        public string EffectDescription(CaptainSkill skill)
        {
            int rank = GetRank(skill);
            return skill switch
            {
                CaptainSkill.CannonMastery =>
                    $"+%{rank * 5} top hasarı",
                CaptainSkill.Rangefinding =>
                    $"+%{rank * 4} top menzili",
                CaptainSkill.HarpoonMastery =>
                    $"+%{rank * 8} zıpkın hasarı",
                CaptainSkill.HarpoonRigging =>
                    $"-%{rank * 5} zıpkın dolumu",
                CaptainSkill.ReinforcedHull =>
                    $"+%{rank * 8} azami gövde",
                CaptainSkill.FineSails =>
                    $"+%{rank * 4} hız • +%{rank * 3} manevra",
                _ => ""
            };
        }

        private void Bind(Transform player)
        {
            progression = player.GetComponentInChildren<
                PrototypeCaptainProgression>();
            broadside = player.GetComponentInChildren<
                BroadsideController>();
            harpoons = player.GetComponentInChildren<
                HarpoonHuntingController>();
            health = player.GetComponentInChildren<
                ShipHealth>();
            motor = player.GetComponentInChildren<
                ShipMotor>();
            ApplyModifiers(false);
        }

        private void ApplyModifiers(bool restoreHealth)
        {
            broadside?.SetSkillModifiers(
                1f + cannonMastery * 0.05f,
                1f + rangefinding * 0.04f,
                1f
            );
            harpoons?.SetSkillModifiers(
                1f + harpoonMastery * 0.08f,
                1f - harpoonRigging * 0.05f
            );
            health?.SetSkillHealthMultiplier(
                1f + reinforcedHull * 0.08f,
                restoreHealth
            );
            motor?.SetSkillPerformance(
                1f + fineSails * 0.04f,
                1f + fineSails * 0.03f
            );
        }

        private void SetRank(CaptainSkill skill, int rank)
        {
            switch (skill)
            {
                case CaptainSkill.CannonMastery:
                    cannonMastery = rank;
                    break;
                case CaptainSkill.Rangefinding:
                    rangefinding = rank;
                    break;
                case CaptainSkill.HarpoonMastery:
                    harpoonMastery = rank;
                    break;
                case CaptainSkill.HarpoonRigging:
                    harpoonRigging = rank;
                    break;
                case CaptainSkill.ReinforcedHull:
                    reinforcedHull = rank;
                    break;
                case CaptainSkill.FineSails:
                    fineSails = rank;
                    break;
            }
        }
    }
}
