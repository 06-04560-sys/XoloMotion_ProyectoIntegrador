using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Tsakel.Game
{
    /// <summary>
    /// Modo camara fotografica: clic derecho (mantener) o C levanta la camara, rueda o Q/E zoom,
    /// clic izquierdo o Espacio dispara. Mientras esta levantada el jugador no se mueve.
    /// </summary>
    public class PhotoCameraController : MonoBehaviour
    {
        public enum CamState { Down, Raising, Up, Lowering }

        [Header("Referencias")]
        [SerializeField] private Camera cam;
        [SerializeField] private Tsakel.Player.PlayerController player;

        [Header("Reglas del GDD")]
        [SerializeField] private float raiseSeconds = 0.4f;
        [SerializeField] private float minZoom = 1f;
        [SerializeField] private float maxZoom = 4f;
        [SerializeField] private float cooldownSeconds = 1.5f;
        [SerializeField] private float maxDistance = 25f;
        [Range(0.05f, 1f)] [SerializeField] private float minCoverage = 0.20f;
        [Range(0f, 1f)] [SerializeField] private float minVisibleFraction = 0.4f;

        [Header("Visor")]
        [Range(0.3f, 0.95f)] [SerializeField] private float viewfinderHeight = 0.78f;
        [SerializeField] private float scrollZoomSpeed = 0.01f;
        [SerializeField] private float keyZoomSpeed = 1.5f;

        public event Action<AnimalSubject, Texture2D> PhotoTaken;
        public event Action<PhotoVerdict> PhotoRejected;

        private readonly List<AnimalSubject> animals = new List<AnimalSubject>();
        private CamState state = CamState.Down;
        private float amount;
        private float zoom = 1f;
        private float cooldownLeft;
        private float baseFov = 60f;
        private bool toggled;

        public bool Blocked { get; set; }
        public CamState State { get { return state; } }
        public float Amount { get { return amount; } }
        public float Zoom { get { return zoom; } }
        public float MinZoom { get { return minZoom; } }
        public float MaxZoom { get { return maxZoom; } }
        public float CooldownLeft { get { return cooldownLeft; } }
        public float CooldownTotal { get { return cooldownSeconds; } }
        public float ViewfinderHeight { get { return viewfinderHeight; } }
        public AnimalSubject Target { get; private set; }
        public PhotoEvaluation Live { get; private set; }

        public void SetAnimals(IList<AnimalSubject> list)
        {
            animals.Clear();
            if (list != null) animals.AddRange(list);
        }

        private void Awake()
        {
            if (cam == null) cam = Camera.main;
            if (player == null) player = FindFirstObjectByType<Tsakel.Player.PlayerController>();
            if (cam != null) baseFov = cam.fieldOfView;
        }

        private void Update()
        {
            if (cam == null || player == null) return;
            float dt = Time.unscaledDeltaTime;
            if (cooldownLeft > 0f) cooldownLeft -= dt;

            if (Blocked)
            {
                if (state != CamState.Down) ForceDown();
                player.SensitivityMultiplier = GameSettings.SensitivityFactor;
                return;
            }

            Mouse mouse = Mouse.current;
            Keyboard kb = Keyboard.current;
            bool rmb = mouse != null && mouse.rightButton.isPressed;
            if (kb != null && kb.cKey.wasPressedThisFrame) toggled = !toggled;
            bool want = rmb || toggled;

            if (want && (state == CamState.Down || state == CamState.Lowering))
            {
                state = CamState.Raising;
                player.SetMovementEnabled(false);
            }
            else if (!want && (state == CamState.Up || state == CamState.Raising))
            {
                state = CamState.Lowering;
            }

            if (state == CamState.Raising)
            {
                amount += dt / Mathf.Max(0.05f, raiseSeconds);
                if (amount >= 1f) { amount = 1f; state = CamState.Up; }
            }
            else if (state == CamState.Lowering)
            {
                amount -= dt / Mathf.Max(0.05f, raiseSeconds);
                if (amount <= 0f)
                {
                    amount = 0f;
                    state = CamState.Down;
                    zoom = minZoom;
                    player.SetMovementEnabled(true);
                }
            }

            // zoom (solo con la camara arriba)
            if (amount > 0.99f)
            {
                float scroll = mouse != null ? mouse.scroll.ReadValue().y : 0f;
                zoom += scroll * scrollZoomSpeed;
                if (kb != null)
                {
                    if (kb.eKey.isPressed) zoom += keyZoomSpeed * dt;
                    if (kb.qKey.isPressed) zoom -= keyZoomSpeed * dt;
                }
                zoom = Mathf.Clamp(zoom, minZoom, maxZoom);
            }

            // FOV segun el zoom: tan(fov/2) se divide entre el zoom
            if (state == CamState.Down)
            {
                cam.fieldOfView = baseFov;
            }
            else
            {
                float half = Mathf.Tan(baseFov * 0.5f * Mathf.Deg2Rad) / zoom;
                cam.fieldOfView = 2f * Mathf.Atan(half) * Mathf.Rad2Deg;
            }
            player.SensitivityMultiplier = GameSettings.SensitivityFactor / Mathf.Max(1f, zoom);

            // objetivo en el visor y evaluacion en vivo
            if (amount > 0.3f)
            {
                Live = PhotoEvaluator.PickAndEvaluate(cam, animals, viewfinderHeight, maxDistance, minCoverage, minVisibleFraction);
                Target = Live.Animal;
            }
            else
            {
                Target = null;
                Live = new PhotoEvaluation();
            }

            bool shoot = (mouse != null && mouse.leftButton.wasPressedThisFrame) || (kb != null && kb.spaceKey.wasPressedThisFrame);
            if (shoot && state == CamState.Up && cooldownLeft <= 0f) Shoot();
        }

        private void Shoot()
        {
            cooldownLeft = cooldownSeconds;
            PhotoEvaluation ev = PhotoEvaluator.PickAndEvaluate(cam, animals, viewfinderHeight, maxDistance, minCoverage, minVisibleFraction);
            AnimalSubject subject = ev.Animal;
            if (!ev.IsValid)
            {
                if (PhotoRejected != null) PhotoRejected(ev.Verdict);
                return;
            }
            Texture2D tex = Capture();
            if (PhotoTaken != null) PhotoTaken(subject, tex);
        }

        /// <summary>Renderiza exactamente lo que se ve dentro del visor cuadrado.</summary>
        private Texture2D Capture()
        {
            const int size = 512;
            float prevFov = cam.fieldOfView;
            float photoFov = 2f * Mathf.Atan(viewfinderHeight * Mathf.Tan(prevFov * 0.5f * Mathf.Deg2Rad)) * Mathf.Rad2Deg;
            RenderTexture rt = RenderTexture.GetTemporary(size, size, 24, RenderTextureFormat.ARGB32);
            RenderTexture prevTarget = cam.targetTexture;
            cam.fieldOfView = photoFov;
            cam.targetTexture = rt;
            cam.Render();
            cam.targetTexture = prevTarget;
            cam.fieldOfView = prevFov;

            RenderTexture prevActive = RenderTexture.active;
            RenderTexture.active = rt;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, size, size), 0, 0);
            tex.Apply();
            RenderTexture.active = prevActive;
            RenderTexture.ReleaseTemporary(rt);
            return tex;
        }

        public void ForceDown()
        {
            state = CamState.Down;
            amount = 0f;
            toggled = false;
            zoom = minZoom;
            Target = null;
            if (cam != null) cam.fieldOfView = baseFov;
        }
    }
}
