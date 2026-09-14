using System;
using Seaborn.Hunting;
using UnityEngine;

namespace Seaborn.Progression
{
    [DisallowMultipleComponent]
    public sealed class PrototypeCaptainProgression : MonoBehaviour
    {
        public const int MaximumLevel = 30;

        [SerializeField, Min(0)]
        private int totalExperience;

        public event Action<int, int> ExperienceChanged;
        public event Action<int> LevelChanged;

        public int TotalExperience => totalExperience;
        public int Level => CalculateLevel(totalExperience);
        public int SkillPointsEarned => Level / 2;
        public int HighestUnlockedMapTier =>
            Mathf.Clamp(1 + (Level - 1) / 4, 1, 8);

        public int ExperienceIntoLevel =>
            totalExperience -
            ExperienceRequiredForLevel(Level);

        public int ExperienceForNextLevel =>
            Level >= MaximumLevel
                ? 0
                : ExperienceCostForLevel(Level);

        private PrototypeHuntCargo cargo;

        public static PrototypeCaptainProgression EnsureAttached(
            Transform player)
        {
            if (player == null) return null;

            PrototypeCaptainProgression progression =
                player.GetComponent<
                    PrototypeCaptainProgression>();
            if (progression == null)
            {
                progression = player.gameObject.AddComponent<
                    PrototypeCaptainProgression>();
            }

            progression.Bind(player);
            return progression;
        }

        public void AddExperience(int amount, string reason)
        {
            if (amount <= 0 || Level >= MaximumLevel) return;

            int previousLevel = Level;
            int maximumExperience =
                ExperienceRequiredForLevel(MaximumLevel);
            totalExperience = Mathf.Clamp(
                totalExperience + amount,
                0,
                maximumExperience
            );

            int nextLevel = Level;
            ExperienceChanged?.Invoke(
                totalExperience,
                nextLevel
            );

            Debug.Log(
                $"{reason}: +{amount} kaptan EXP " +
                $"(Seviye {nextLevel})",
                this
            );

            if (nextLevel <= previousLevel) return;

            for (int level = previousLevel + 1;
                 level <= nextLevel;
                 level++)
            {
                LevelChanged?.Invoke(level);
                Debug.Log(
                    $"Kaptan seviyesi yükseldi: {level}. " +
                    $"Harita tier sınırı: {HighestUnlockedMapTier}.",
                    this
                );
            }
        }

        public void RestoreExperience(int amount)
        {
            totalExperience = Mathf.Clamp(
                amount,
                0,
                ExperienceRequiredForLevel(MaximumLevel)
            );
            ExperienceChanged?.Invoke(
                totalExperience,
                Level
            );
        }

        public bool MeetsLevel(int requiredLevel)
        {
            return Level >= Mathf.Max(1, requiredLevel);
        }

        public static int ExperienceCostForLevel(int level)
        {
            if (level < 1 || level >= MaximumLevel) return 0;
            return 100 + (level - 1) * 75;
        }

        public static int ExperienceRequiredForLevel(int level)
        {
            int clamped = Mathf.Clamp(
                level,
                1,
                MaximumLevel
            );
            int result = 0;
            for (int current = 1;
                 current < clamped;
                 current++)
            {
                result += ExperienceCostForLevel(current);
            }

            return result;
        }

        private static int CalculateLevel(int experience)
        {
            int level = 1;
            while (level < MaximumLevel &&
                   experience >=
                   ExperienceRequiredForLevel(level + 1))
            {
                level++;
            }

            return level;
        }

        private void Bind(Transform player)
        {
            PrototypeHuntCargo nextCargo =
                player.GetComponentInChildren<
                    PrototypeHuntCargo>();
            if (cargo == nextCargo) return;

            if (cargo != null)
            {
                cargo.CargoSecured -= HandleCargoSecured;
            }

            cargo = nextCargo;
            if (cargo != null)
            {
                cargo.CargoSecured += HandleCargoSecured;
            }
        }

        private void HandleCargoSecured(int silverValue)
        {
            int experience = Mathf.Max(
                10,
                Mathf.CeilToInt(silverValue * 0.5f)
            );
            AddExperience(
                experience,
                "Sefer dönüşü"
            );
        }

        private void OnDestroy()
        {
            if (cargo != null)
            {
                cargo.CargoSecured -= HandleCargoSecured;
            }
        }
    }
}
