using UnityEngine;

namespace Tsakel.Game
{
    /// <summary>
    /// Animal fotografiable. Incluye un comportamiento provisional de esconderse y aparecer
    /// (reduce su escala y lo oculta; todavia no hay animaciones de animales).
    /// </summary>
    [DisallowMultipleComponent]
    public class AnimalSubject : MonoBehaviour
    {
        [Tooltip("Id de la especie (debe existir en SpeciesCatalog): jaguar, tapir, guacamaya, lagartija, quetzal.")]
        [SerializeField] private string speciesId = "";

        [Header("Esconderse (provisional)")]
        [SerializeField] private bool canHide = true;
        [Tooltip("Segundos visible antes de esconderse.")]
        [SerializeField] private float visibleSeconds = 10f;
        [Tooltip("Segundos escondido antes de reaparecer.")]
        [SerializeField] private float hiddenSeconds = 5f;
        [Tooltip("Si el jugador se acerca a menos de esta distancia (m), el animal se esconde.")]
        [SerializeField] private float shyDistance = 3f;
        [SerializeField] private float popSeconds = 0.35f;
        [Tooltip("Opcional: puntos donde puede reaparecer (se recorren en orden).")]
        [SerializeField] private Transform[] spots;

        private enum State { Visible, Hiding, Hidden, Showing }

        private State state = State.Visible;
        private float timer = 10f;
        private float pop = 1f;
        private Vector3 baseScale = Vector3.one;
        private bool initialized;
        private Renderer[] renderers;
        private Transform playerTransform;
        private int spotIndex = -1;

        public string SpeciesId { get { return speciesId; } }
        public bool IsPhotographable { get { return gameObject.activeInHierarchy && state == State.Visible; } }

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            if (initialized) return;
            baseScale = transform.localScale;
            renderers = GetComponentsInChildren<Renderer>(true);
            initialized = true;
        }

        /// <summary>Reinicia el ciclo al empezar la ronda. phase desfasa el primer escondite.</summary>
        public void BeginRound(float phase)
        {
            Init();
            state = State.Visible;
            pop = 1f;
            transform.localScale = baseScale;
            SetRenderers(true);
            timer = visibleSeconds + phase;
        }

        public bool TryGetBounds(out Bounds bounds)
        {
            bounds = new Bounds(transform.position, Vector3.zero);
            bool has = false;
            if (renderers == null) return false;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer r = renderers[i];
                if (r == null || !r.enabled || !r.gameObject.activeInHierarchy) continue;
                if (!has) { bounds = r.bounds; has = true; }
                else bounds.Encapsulate(r.bounds);
            }
            return has;
        }

        private void Update()
        {
            if (!canHide || !initialized) return;
            float dt = Time.deltaTime;

            if (playerTransform == null)
            {
                Tsakel.Player.PlayerController pc = FindFirstObjectByType<Tsakel.Player.PlayerController>();
                if (pc != null) playerTransform = pc.transform;
            }
            float dist = playerTransform != null ? HorizontalDistance(playerTransform.position, transform.position) : 999f;

            switch (state)
            {
                case State.Visible:
                    timer -= dt;
                    if (timer <= 0f || dist < shyDistance) state = State.Hiding;
                    break;
                case State.Hiding:
                    pop -= dt / Mathf.Max(0.05f, popSeconds);
                    if (pop <= 0f)
                    {
                        pop = 0f;
                        state = State.Hidden;
                        timer = hiddenSeconds;
                        SetRenderers(false);
                    }
                    ApplyScale();
                    break;
                case State.Hidden:
                    timer -= dt;
                    if (timer <= 0f && dist > shyDistance * 1.3f)
                    {
                        if (spots != null && spots.Length > 0)
                        {
                            spotIndex = (spotIndex + 1) % spots.Length;
                            if (spots[spotIndex] != null) transform.SetPositionAndRotation(spots[spotIndex].position, spots[spotIndex].rotation);
                        }
                        SetRenderers(true);
                        state = State.Showing;
                    }
                    break;
                case State.Showing:
                    pop += dt / Mathf.Max(0.05f, popSeconds);
                    if (pop >= 1f)
                    {
                        pop = 1f;
                        state = State.Visible;
                        timer = visibleSeconds;
                    }
                    ApplyScale();
                    break;
            }
        }

        private void ApplyScale()
        {
            float s = pop * pop * (3f - 2f * pop);
            transform.localScale = baseScale * Mathf.Max(0.01f, s);
        }

        private void SetRenderers(bool on)
        {
            if (renderers == null) return;
            for (int i = 0; i < renderers.Length; i++)
                if (renderers[i] != null) renderers[i].enabled = on;
        }

        private static float HorizontalDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }
    }
}
