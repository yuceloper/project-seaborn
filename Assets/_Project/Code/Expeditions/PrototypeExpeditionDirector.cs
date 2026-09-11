using System;
using Seaborn.Hunting;
using Seaborn.Ship;
using UnityEngine;

namespace Seaborn.Expeditions
{
    public enum PrototypeExpeditionState
    {
        AtHarbor,
        Underway,
        ReturnRecommended,
        Completed,
        Failed
    }

    [DisallowMultipleComponent]
    public sealed class PrototypeExpeditionDirector :
        MonoBehaviour
    {
        [SerializeField, Min(1f)]
        private float harborRadius = 4.8f;

        [SerializeField, Min(1)]
        private int recommendedReturnValue = 90;

        [SerializeField, Min(1f)]
        private float pressureBeginsAfter = 210f;

        [SerializeField, Min(1f)]
        private float maximumPressureAfter = 360f;

        public static PrototypeExpeditionDirector Instance
        {
            get;
            private set;
        }

        public event Action<PrototypeExpeditionState>
            StateChanged;
        public event Action ExpeditionChanged;

        public PrototypeExpeditionState State
        {
            get;
            private set;
        } = PrototypeExpeditionState.AtHarbor;

        public float ElapsedTime =>
            IsActive
                ? Mathf.Max(0f, Time.time - startedAt)
                : finalDuration;

        public float PressureNormalized =>
            !IsActive
                ? 0f
                : Mathf.InverseLerp(
                    pressureBeginsAfter,
                    maximumPressureAfter,
                    ElapsedTime
                );

        public int CurrentUnsecuredValue =>
            cargo != null
                ? cargo.UnsecuredSilverValue
                : 0;

        public int RecommendedReturnValue =>
            recommendedReturnValue;

        public bool IsActive =>
            State == PrototypeExpeditionState.Underway ||
            State ==
                PrototypeExpeditionState.ReturnRecommended;

        public Vector3 HarborPosition => harborPosition;

        public float HarborRadius => harborRadius;

        public bool IsPlayerAtHarbor =>
            player != null &&
            HorizontalDistance(
                player.position,
                harborPosition
            ) <= harborRadius;

        private Transform player;
        private PrototypeHuntCargo cargo;
        private ShipHealth shipHealth;
        private Vector3 harborPosition;
        private float startedAt;
        private float finalDuration;
        private float nextChangedSignal;
        private int securedValue;
        private int lostValue;

        public static void EnsureCreated(
            Transform playerTransform)
        {
            if (playerTransform == null)
            {
                return;
            }

            PrototypeExpeditionDirector director =
                FindFirstObjectByType<
                    PrototypeExpeditionDirector>();

            if (director == null)
            {
                GameObject directorObject =
                    new GameObject(
                        "Prototype Expedition Director"
                    );
                director =
                    directorObject.AddComponent<
                        PrototypeExpeditionDirector>();
            }

            director.BindPlayer(playerTransform);
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Update()
        {
            if (player == null)
            {
                return;
            }

            bool isAtHarbor = IsPlayerAtHarbor;

            if ((State ==
                    PrototypeExpeditionState.AtHarbor ||
                 State ==
                    PrototypeExpeditionState.Completed) &&
                !isAtHarbor)
            {
                BeginExpedition();
            }

            if (!IsActive)
            {
                return;
            }

            if (State ==
                    PrototypeExpeditionState.Underway &&
                CurrentUnsecuredValue >=
                    recommendedReturnValue)
            {
                SetState(
                    PrototypeExpeditionState
                        .ReturnRecommended
                );

                Debug.Log(
                    $"Sefer hedefi tamamlandı: " +
                    $"{CurrentUnsecuredValue} güvencesiz " +
                    "silver. Limana dön veya riski büyüt.",
                    this
                );
            }

            if (Time.time >= nextChangedSignal)
            {
                nextChangedSignal = Time.time + 0.25f;
                ExpeditionChanged?.Invoke();
            }
        }

        private void BindPlayer(
            Transform playerTransform)
        {
            if (player == playerTransform &&
                cargo != null &&
                shipHealth != null)
            {
                return;
            }

            Unsubscribe();

            player = playerTransform;
            cargo =
                player.GetComponentInChildren<
                    PrototypeHuntCargo>();
            shipHealth =
                player.GetComponentInChildren<ShipHealth>();
            harborPosition = player.position;

            PrototypeSafeHarborProtection.EnsureAttached(
                player,
                this
            );

            if (cargo != null)
            {
                cargo.CargoChanged +=
                    HandleCargoChanged;
                cargo.CargoSecured +=
                    HandleCargoSecured;
                cargo.CargoLost +=
                    HandleCargoLost;
            }

            if (shipHealth != null)
            {
                shipHealth.Sunk += HandleShipSunk;
            }

            ExpeditionChanged?.Invoke();
        }

        private void BeginExpedition()
        {
            startedAt = Time.time;
            finalDuration = 0f;
            securedValue = 0;
            lostValue = 0;
            SetState(PrototypeExpeditionState.Underway);

            Debug.Log(
                $"Sefer başladı. Tavsiye edilen dönüş " +
                $"değeri: {recommendedReturnValue} silver.",
                this
            );
        }

        private void HandleCargoChanged()
        {
            ExpeditionChanged?.Invoke();
        }

        private void HandleCargoSecured(int value)
        {
            if (!IsActive || value <= 0)
            {
                return;
            }

            securedValue += value;
            finalDuration =
                Mathf.Max(0f, Time.time - startedAt);
            SetState(PrototypeExpeditionState.Completed);

            Debug.Log(
                $"Sefer tamamlandı: {securedValue} silver " +
                $"güvenceye alındı " +
                $"({finalDuration:0.0} saniye).",
                this
            );
        }

        private void HandleCargoLost(int value)
        {
            lostValue += Mathf.Max(0, value);

            if (IsActive)
            {
                FailExpedition();
            }
        }

        private void HandleShipSunk()
        {
            if (IsActive)
            {
                FailExpedition();
            }
        }

        private void FailExpedition()
        {
            finalDuration =
                Mathf.Max(0f, Time.time - startedAt);
            SetState(PrototypeExpeditionState.Failed);

            Debug.Log(
                $"Sefer başarısız: {lostValue} silver " +
                $"yük kaybedildi " +
                $"({finalDuration:0.0} saniye).",
                this
            );
        }

        private void SetState(
            PrototypeExpeditionState newState)
        {
            if (State == newState)
            {
                return;
            }

            State = newState;
            StateChanged?.Invoke(State);
            ExpeditionChanged?.Invoke();
        }

        private void Unsubscribe()
        {
            if (cargo != null)
            {
                cargo.CargoChanged -=
                    HandleCargoChanged;
                cargo.CargoSecured -=
                    HandleCargoSecured;
                cargo.CargoLost -=
                    HandleCargoLost;
            }

            if (shipHealth != null)
            {
                shipHealth.Sunk -= HandleShipSunk;
            }
        }

        private static float HorizontalDistance(
            Vector3 first,
            Vector3 second)
        {
            Vector3 offset = first - second;
            offset.y = 0f;
            return offset.magnitude;
        }

        private void OnDestroy()
        {
            Unsubscribe();

            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
