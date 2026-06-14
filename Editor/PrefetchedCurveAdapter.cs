using System;
using System.Reflection;
using UnityEngine;

namespace DivineDragon
{
    /// <summary>
    /// Reflection bridge to the game's <c>PrefetchedCurve_Bridge</c> / <c>TrailTrack</c> types.
    ///
    /// Those types live in the dumped game scripts (the predefined <c>Assembly-CSharp</c> assembly),
    /// which a package assembly definition cannot reference at compile time. So instead of a hard
    /// reference we resolve them by name at runtime and expose a typed facade, mirroring the
    /// TerrainAssetAdapter pattern in DivineDragon.MapTools. If the game scripts are absent the
    /// adapter degrades gracefully (BridgeType is null, FromObject returns null).
    /// </summary>
    public sealed class PrefetchedCurveAdapter
    {
        private static readonly Type bridgeType = ResolveType("PrefetchedCurve_Bridge");
        private static readonly Type trailTrackType = ResolveType("TrailTrack");
        private static readonly FieldInfo rightHandField = GetTrackField("RightHand");
        private static readonly FieldInfo leftHandField = GetTrackField("LeftHand");
        private static bool hasLoggedMissingType;

        private static Type ResolveType(string name)
        {
            Type type = Type.GetType(name + ", Assembly-CSharp");
            if (type != null)
            {
                return type;
            }

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetType(name);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        // RightHand / LeftHand are declared on the base PrefetchedCurve; a public-instance lookup
        // on the bridge type finds inherited public fields.
        private static FieldInfo GetTrackField(string name)
        {
            return bridgeType?.GetField(name, BindingFlags.Instance | BindingFlags.Public);
        }

        /// <summary>The resolved PrefetchedCurve_Bridge type, or null if the game scripts are absent.</summary>
        public static Type BridgeType
        {
            get
            {
                if (bridgeType == null && !hasLoggedMissingType)
                {
                    hasLoggedMissingType = true;
                    Debug.LogWarning("DivineDragon.AnimationTools: PrefetchedCurve_Bridge type not found. Prefetched curve tooling will have limited functionality.");
                }

                return bridgeType;
            }
        }

        /// <summary>True if <paramref name="obj"/> is a PrefetchedCurve_Bridge instance.</summary>
        public static bool Is(UnityEngine.Object obj)
        {
            return obj != null && bridgeType != null && bridgeType.IsInstanceOfType(obj);
        }

        public static PrefetchedCurveAdapter FromObject(UnityEngine.Object obj)
        {
            return Is(obj) ? new PrefetchedCurveAdapter((ScriptableObject)obj) : null;
        }

        public ScriptableObject Asset { get; }

        public bool IsValid => Asset != null;

        public PrefetchedCurveAdapter(ScriptableObject asset)
        {
            Asset = asset;
        }

        public TrailTrackAdapter RightHand => GetTrack(rightHandField);

        public TrailTrackAdapter LeftHand => GetTrack(leftHandField);

        private TrailTrackAdapter GetTrack(FieldInfo handField)
        {
            if (Asset == null || handField == null)
            {
                return null;
            }

            object track = handField.GetValue(Asset);
            if (track == null && trailTrackType != null)
            {
                // Mirror Unity's serialization, which never leaves a [Serializable] class field null.
                track = Activator.CreateInstance(trailTrackType);
                handField.SetValue(Asset, track);
            }

            return track != null ? new TrailTrackAdapter(track) : null;
        }

        /// <summary>
        /// Reflection facade over a game TrailTrack instance: its six public AnimationCurve fields.
        /// AnimationCurve is a reference type, so a curve set here (and then mutated) persists on the
        /// underlying asset.
        /// </summary>
        public sealed class TrailTrackAdapter
        {
            private static readonly FieldInfo rootXField = GetCurveField("RootX");
            private static readonly FieldInfo rootYField = GetCurveField("RootY");
            private static readonly FieldInfo rootZField = GetCurveField("RootZ");
            private static readonly FieldInfo tipXField = GetCurveField("TipX");
            private static readonly FieldInfo tipYField = GetCurveField("TipY");
            private static readonly FieldInfo tipZField = GetCurveField("TipZ");

            private static FieldInfo GetCurveField(string name)
            {
                return trailTrackType?.GetField(name, BindingFlags.Instance | BindingFlags.Public);
            }

            private readonly object track;

            public TrailTrackAdapter(object track)
            {
                this.track = track;
            }

            public AnimationCurve RootX { get => Get(rootXField); set => Set(rootXField, value); }
            public AnimationCurve RootY { get => Get(rootYField); set => Set(rootYField, value); }
            public AnimationCurve RootZ { get => Get(rootZField); set => Set(rootZField, value); }
            public AnimationCurve TipX { get => Get(tipXField); set => Set(tipXField, value); }
            public AnimationCurve TipY { get => Get(tipYField); set => Set(tipYField, value); }
            public AnimationCurve TipZ { get => Get(tipZField); set => Set(tipZField, value); }

            private AnimationCurve Get(FieldInfo field)
            {
                return field != null && track != null ? field.GetValue(track) as AnimationCurve : null;
            }

            private void Set(FieldInfo field, AnimationCurve value)
            {
                if (field != null && track != null)
                {
                    field.SetValue(track, value);
                }
            }
        }
    }
}
