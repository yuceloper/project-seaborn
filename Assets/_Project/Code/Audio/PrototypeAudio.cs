using System;
using System.Collections.Generic;
using Seaborn.Ship;
using UnityEngine;

namespace Seaborn.Audio
{
    public static class PrototypeAudio
    {
        public static void PlayCannon(Vector3 position)
        {
            PrototypeAudioRuntime.Instance.PlayCannon(position);
        }

        public static void PlayHullImpact(Vector3 position)
        {
            PrototypeAudioRuntime.Instance.PlayHullImpact(position);
        }

        public static void PlayWaterSplash(Vector3 position)
        {
            PrototypeAudioRuntime.Instance.PlayWaterSplash(position);
        }

        public static void PlaySinking(Vector3 position)
        {
            PrototypeAudioRuntime.Instance.PlaySinking(position);
        }

        public static void PlayCreak(
            Vector3 position,
            float volume)
        {
            PrototypeAudioRuntime.Instance.PlayCreak(
                position,
                volume
            );
        }
    }

    internal sealed class PrototypeAudioRuntime :
        MonoBehaviour
    {
        private const int SampleRate = 22050;
        private const float ShipScanInterval = 1f;

        private static PrototypeAudioRuntime instance;

        private readonly List<AudioClip> generatedClips =
            new List<AudioClip>();

        private AudioClip cannonClip;
        private AudioClip hullImpactClip;
        private AudioClip waterSplashClip;
        private AudioClip sinkingClip;
        private AudioClip creakClip;

        private float nextShipScan;

        public static PrototypeAudioRuntime Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject runtimeObject =
                        new GameObject(
                            "Prototype Audio Runtime"
                        );
                    instance = runtimeObject.AddComponent<
                        PrototypeAudioRuntime>();
                    DontDestroyOnLoad(runtimeObject);
                }

                return instance;
            }
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Warmup()
        {
            _ = Instance;
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            AudioClip ambience = CreateAmbience();
            cannonClip = CreateCannon();
            hullImpactClip = CreateHullImpact();
            waterSplashClip = CreateWaterSplash();
            sinkingClip = CreateSinking();
            creakClip = CreateCreak();

            AudioSource ambienceSource =
                gameObject.AddComponent<AudioSource>();
            ambienceSource.clip = ambience;
            ambienceSource.loop = true;
            ambienceSource.playOnAwake = false;
            ambienceSource.spatialBlend = 0f;
            ambienceSource.volume = 0.38f;
            ambienceSource.priority = 180;
            ambienceSource.Play();
        }

        private void Update()
        {
            if (Time.unscaledTime < nextShipScan)
            {
                return;
            }

            nextShipScan =
                Time.unscaledTime + ShipScanInterval;

            ShipHealth[] ships = FindObjectsByType<ShipHealth>(
                FindObjectsSortMode.None
            );

            foreach (ShipHealth ship in ships)
            {
                if (ship.GetComponent<PrototypeShipAudio>() ==
                    null)
                {
                    ship.gameObject.AddComponent<
                        PrototypeShipAudio>();
                }
            }
        }

        public void PlayCannon(Vector3 position)
        {
            PlaySpatial(
                cannonClip,
                position,
                0.72f,
                0.94f,
                1.04f
            );
        }

        public void PlayHullImpact(Vector3 position)
        {
            PlaySpatial(
                hullImpactClip,
                position,
                0.55f,
                0.94f,
                1.08f
            );
        }

        public void PlayWaterSplash(Vector3 position)
        {
            PlaySpatial(
                waterSplashClip,
                position,
                0.48f,
                0.9f,
                1.08f
            );
        }

        public void PlaySinking(Vector3 position)
        {
            PlaySpatial(
                sinkingClip,
                position,
                0.62f,
                0.92f,
                1.02f
            );
        }

        public void PlayCreak(
            Vector3 position,
            float volume)
        {
            PlaySpatial(
                creakClip,
                position,
                Mathf.Clamp01(volume),
                0.9f,
                1.1f
            );
        }

        private static void PlaySpatial(
            AudioClip clip,
            Vector3 position,
            float volume,
            float minimumPitch,
            float maximumPitch)
        {
            if (clip == null)
            {
                return;
            }

            GameObject soundObject =
                new GameObject(clip.name);
            soundObject.transform.position = position;

            AudioSource source =
                soundObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.volume = volume;
            source.pitch = UnityEngine.Random.Range(
                minimumPitch,
                maximumPitch
            );
            source.spatialBlend = 0.68f;
            source.rolloffMode =
                AudioRolloffMode.Linear;
            source.minDistance = 4f;
            source.maxDistance = 42f;
            source.dopplerLevel = 0.08f;
            source.priority = 128;
            source.Play();

            Destroy(
                soundObject,
                clip.length /
                Mathf.Max(0.1f, source.pitch) +
                0.1f
            );
        }

        private AudioClip CreateAmbience()
        {
            const float duration = 8f;
            int sampleCount =
                Mathf.CeilToInt(duration * SampleRate);
            float[] samples =
                new float[sampleCount * 2];

            System.Random random = new System.Random(1931);
            float leftNoise = 0f;
            float rightNoise = 0f;

            for (int index = 0;
                 index < sampleCount;
                 index++)
            {
                float time = index / (float)SampleRate;
                float edgeFade = Mathf.Min(
                    Mathf.Clamp01(time / 0.7f),
                    Mathf.Clamp01(
                        (duration - time) / 0.7f
                    )
                );

                leftNoise = Mathf.Lerp(
                    leftNoise,
                    NextSigned(random),
                    0.0065f
                );
                rightNoise = Mathf.Lerp(
                    rightNoise,
                    NextSigned(random),
                    0.0052f
                );

                float swell =
                    Mathf.Sin(time * 0.72f) * 0.32f +
                    Mathf.Sin(time * 1.31f + 1.4f) *
                    0.18f;

                samples[index * 2] =
                    (leftNoise * 0.62f + swell * 0.09f) *
                    edgeFade;
                samples[index * 2 + 1] =
                    (rightNoise * 0.62f + swell * 0.09f) *
                    edgeFade;
            }

            return CreateClip(
                "Open Sea Ambience",
                samples,
                2
            );
        }

        private AudioClip CreateCannon()
        {
            return CreateMonoClip(
                "Cannon Report",
                1.25f,
                271,
                (time, noise) =>
                {
                    float attack =
                        Mathf.Clamp01(time / 0.012f);
                    float body = Mathf.Exp(-time * 3.7f);
                    float lowTone = Mathf.Sin(
                        time *
                        Mathf.Lerp(72f, 39f, time) *
                        Mathf.PI *
                        2f
                    );

                    return attack * body *
                        (lowTone * 0.72f +
                         noise * Mathf.Exp(-time * 9f) *
                         0.58f);
                }
            );
        }

        private AudioClip CreateHullImpact()
        {
            return CreateMonoClip(
                "Hull Impact",
                0.52f,
                337,
                (time, noise) =>
                {
                    float envelope =
                        Mathf.Exp(-time * 10f);
                    float woodTone =
                        Mathf.Sin(time * 185f) *
                        Mathf.Exp(-time * 6f);

                    return envelope * noise * 0.72f +
                        woodTone * 0.28f;
                }
            );
        }

        private AudioClip CreateWaterSplash()
        {
            return CreateMonoClip(
                "Water Splash",
                0.92f,
                419,
                (time, noise) =>
                {
                    float attack =
                        Mathf.Clamp01(time / 0.025f);
                    float envelope =
                        Mathf.Exp(-time * 4.2f);
                    float bubble =
                        Mathf.Sin(
                            time *
                            (95f - time * 52f) *
                            Mathf.PI *
                            2f
                        ) * 0.12f;

                    return attack * envelope *
                        (noise * 0.48f + bubble);
                }
            );
        }

        private AudioClip CreateSinking()
        {
            return CreateMonoClip(
                "Sinking Rumble",
                2.8f,
                557,
                (time, noise) =>
                {
                    float envelope =
                        Mathf.Sin(
                            Mathf.Clamp01(time / 2.8f) *
                            Mathf.PI
                        );
                    float rumble =
                        Mathf.Sin(time * 31f) * 0.34f +
                        Mathf.Sin(time * 47f + 0.7f) *
                        0.2f;

                    return envelope *
                        (rumble + noise * 0.12f);
                }
            );
        }

        private AudioClip CreateCreak()
        {
            return CreateMonoClip(
                "Hull Creak",
                0.78f,
                683,
                (time, noise) =>
                {
                    float envelope =
                        Mathf.Sin(
                            Mathf.Clamp01(time / 0.78f) *
                            Mathf.PI
                        );
                    float bend =
                        Mathf.Sin(
                            time *
                            (118f - time * 42f)
                        ) * 0.42f;

                    return envelope *
                        (bend + noise * 0.035f);
                }
            );
        }

        private AudioClip CreateMonoClip(
            string clipName,
            float duration,
            int seed,
            Func<float, float, float> sample)
        {
            int sampleCount =
                Mathf.CeilToInt(duration * SampleRate);
            float[] samples = new float[sampleCount];
            System.Random random = new System.Random(seed);

            for (int index = 0;
                 index < sampleCount;
                 index++)
            {
                float time = index / (float)SampleRate;
                samples[index] = Mathf.Clamp(
                    sample(time, NextSigned(random)),
                    -0.92f,
                    0.92f
                );
            }

            return CreateClip(clipName, samples, 1);
        }

        private AudioClip CreateClip(
            string clipName,
            float[] samples,
            int channels)
        {
            int lengthSamples = samples.Length / channels;
            AudioClip clip = AudioClip.Create(
                clipName,
                lengthSamples,
                channels,
                SampleRate,
                false
            );
            clip.SetData(samples, 0);
            generatedClips.Add(clip);
            return clip;
        }

        private static float NextSigned(System.Random random)
        {
            return (float)random.NextDouble() * 2f - 1f;
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }

            foreach (AudioClip clip in generatedClips)
            {
                if (clip != null)
                {
                    Destroy(clip);
                }
            }
        }
    }

    [DisallowMultipleComponent]
    internal sealed class PrototypeShipAudio : MonoBehaviour
    {
        private Rigidbody shipRigidbody;
        private Vector3 previousPosition;
        private float nextCreakTime;

        private void Awake()
        {
            shipRigidbody = GetComponent<Rigidbody>();
            previousPosition = transform.position;
            ScheduleNextCreak(1.2f, 3.4f);
        }

        private void Update()
        {
            if (Time.time < nextCreakTime)
            {
                previousPosition = transform.position;
                return;
            }

            float deltaTime = Mathf.Max(
                Time.deltaTime,
                0.0001f
            );
            Vector3 frameVelocity =
                (transform.position - previousPosition) /
                deltaTime;

            Vector3 velocity =
                shipRigidbody != null &&
                !shipRigidbody.isKinematic
                    ? shipRigidbody.linearVelocity
                    : frameVelocity;

            float speed = Vector3.ProjectOnPlane(
                velocity,
                Vector3.up
            ).magnitude;
            float speedFactor =
                Mathf.Clamp01(speed / 5f);

            PrototypeAudio.PlayCreak(
                transform.position,
                Mathf.Lerp(0.08f, 0.18f, speedFactor)
            );

            ScheduleNextCreak(
                Mathf.Lerp(3.4f, 1.7f, speedFactor),
                Mathf.Lerp(5.2f, 3f, speedFactor)
            );
            previousPosition = transform.position;
        }

        private void ScheduleNextCreak(
            float minimumDelay,
            float maximumDelay)
        {
            nextCreakTime =
                Time.time +
                UnityEngine.Random.Range(
                    minimumDelay,
                    maximumDelay
                );
        }
    }
}
