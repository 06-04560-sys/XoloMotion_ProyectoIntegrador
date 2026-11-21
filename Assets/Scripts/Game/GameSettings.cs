using System;
using UnityEngine;

namespace Tsakel.Game
{
    /// <summary>Ajustes del jugador (0 a 1). Se guardan con PlayerPrefs.</summary>
    public static class GameSettings
    {
        private const string KMaster = "tsakel.volMaster";
        private const string KAnimals = "tsakel.volAnimals";
        private const string KMusic = "tsakel.volMusic";
        private const string KSens = "tsakel.sensitivity";
        public const float DefaultValue = 0.6f;

        /// <summary>Se dispara al cambiar cualquier ajuste (AudioVolume lo usa para actualizar su volumen).</summary>
        public static event Action Changed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Changed = null;
        }

        public static float Master
        {
            get { return PlayerPrefs.GetFloat(KMaster, DefaultValue); }
            set { Set(KMaster, value); Apply(); }
        }

        public static float Animals
        {
            get { return PlayerPrefs.GetFloat(KAnimals, DefaultValue); }
            set { Set(KAnimals, value); }
        }

        public static float Music
        {
            get { return PlayerPrefs.GetFloat(KMusic, DefaultValue); }
            set { Set(KMusic, value); }
        }

        public static float Sensitivity
        {
            get { return PlayerPrefs.GetFloat(KSens, DefaultValue); }
            set { Set(KSens, value); }
        }

        private static void Set(string key, float value)
        {
            PlayerPrefs.SetFloat(key, Mathf.Clamp01(value));
            if (Changed != null) Changed();
        }

        /// <summary>Factor para PlayerController: 60% = 1.0 (la sensibilidad que ya probaste).</summary>
        public static float SensitivityFactor
        {
            get { return Mathf.Max(0.15f, Sensitivity / DefaultValue); }
        }

        public static void Apply()
        {
            AudioListener.volume = Master;
        }
    }
}
