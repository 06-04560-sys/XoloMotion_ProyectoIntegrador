using UnityEngine;

namespace Tsakel.Game
{
    /// <summary>
    /// Aplica el volumen de Ajustes (animales o musica) a un AudioSource.
    /// Uso: agregar junto al AudioSource y elegir el canal. El volumen maestro ya lo aplica GameSettings.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    public class AudioVolume : MonoBehaviour
    {
        public enum Channel { Animales, Musica }

        [SerializeField] private Channel channel = Channel.Animales;
        [Tooltip("Volumen propio del sonido (0 a 1). Se multiplica por el volumen del canal.")]
        [Range(0f, 1f)] [SerializeField] private float baseVolume = 1f;

        private AudioSource source;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
        }

        private void OnEnable()
        {
            GameSettings.Changed += Apply;
            Apply();
        }

        private void OnDisable()
        {
            GameSettings.Changed -= Apply;
        }

        private void OnValidate()
        {
            if (source != null) Apply();
        }

        private void Apply()
        {
            float channelVolume = channel == Channel.Musica ? GameSettings.Music : GameSettings.Animals;
            source.volume = baseVolume * channelVolume;
        }
    }
}
