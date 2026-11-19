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

        public static float Master
        {
            get { return PlayerPrefs.GetFloat(KMaster, DefaultValue); }
            set { PlayerPrefs.SetFloat(KMaster, Mathf.Clamp01(value)); Apply(); }
        }

        public static float Animals
        {
            get { return PlayerPrefs.GetFloat(KAnimals, DefaultValue); }
            set { PlayerPrefs.SetFloat(KAnimals, Mathf.Clamp01(value)); }
        }

        public static float Music
        {
            get { return PlayerPrefs.GetFloat(KMusic, DefaultValue); }
            set { PlayerPrefs.SetFloat(KMusic, Mathf.Clamp01(value)); }
        }

        public static float Sensitivity
        {
            get { return PlayerPrefs.GetFloat(KSens, DefaultValue); }
            set { PlayerPrefs.SetFloat(KSens, Mathf.Clamp01(value)); }
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
