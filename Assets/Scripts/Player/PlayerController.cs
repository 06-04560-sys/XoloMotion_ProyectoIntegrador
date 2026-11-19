using UnityEngine;
using UnityEngine.InputSystem;

namespace Tsakel.Player
{
    /// <summary>
    /// Movimiento en primera persona para Tsakel: CharacterController + nuevo Input System.
    /// Solo caminar y mirar (sin salto, carrera ni agacharse).
    /// Para bloquear el control desde otros scripts (camara fotografica, album, menus):
    /// SetMovementEnabled, SetLookEnabled, SetControlsEnabled y SetCursorLocked.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Referencias")]
        [Tooltip("Camara del jugador (Main Camera). Si esta vacia se busca una Camera hija.")]
        [SerializeField] private Transform cameraTransform;
        [Tooltip("Asset de controles: Assets/Input/TsakelControls.")]
        [SerializeField] private InputActionAsset controls;

        [Header("Movimiento")]
        [Tooltip("Velocidad al caminar en metros por segundo.")]
        [Min(0f)] [SerializeField] private float moveSpeed = 3.5f;
        [Tooltip("Que tan rapido alcanza la velocidad (m/s2). Mas alto = mas directo.")]
        [Min(0.1f)] [SerializeField] private float acceleration = 20f;
        [Tooltip("Que tan rapido se detiene (m/s2). Mas alto = frena mas rapido.")]
        [Min(0.1f)] [SerializeField] private float deceleration = 25f;
        [SerializeField] private float gravity = -9.81f;

        [Header("Mirada")]
        [Tooltip("Sensibilidad del mouse en grados por pixel.")]
        [Min(0f)] [SerializeField] private float mouseSensitivity = 0.1f;
        [Tooltip("Sensibilidad del joystick derecho en grados por segundo.")]
        [Min(0f)] [SerializeField] private float stickSensitivity = 120f;
        [Tooltip("Limite de la mirada vertical hacia arriba y hacia abajo, en grados.")]
        [Range(10f, 89f)] [SerializeField] private float verticalLimit = 80f;

        [Header("Camara")]
        [Tooltip("Altura de la camara respecto a los pies del jugador, en metros.")]
        [SerializeField] private float cameraHeight = 1.65f;

        [Header("Cursor")]
        [SerializeField] private bool lockCursorOnStart = true;
        [Tooltip("Si el cursor se libero (Esc en el Editor), un clic lo vuelve a capturar mientras la mirada este activa.")]
        [SerializeField] private bool relockCursorOnClick = true;

        [Header("Estado inicial")]
        [SerializeField] private bool movementEnabled = true;
        [SerializeField] private bool lookEnabled = true;

        private CharacterController controller;
        private InputAction moveAction;
        private InputAction lookMouseAction;
        private InputAction lookStickAction;

        private Vector3 horizontalVelocity;
        private float verticalVelocity;
        private float pitch;

        public bool MovementEnabled => movementEnabled;
        public bool LookEnabled => lookEnabled;

        /// <summary>Multiplicador externo de sensibilidad (ajustes y zoom de la camara fotografica).</summary>
        public float SensitivityMultiplier { get; set; } = 1f;

        public float MoveSpeed
        {
            get => moveSpeed;
            set => moveSpeed = Mathf.Max(0f, value);
        }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();

            if (cameraTransform == null)
            {
                Camera cam = GetComponentInChildren<Camera>();
                if (cam != null) cameraTransform = cam.transform;
            }

            if (cameraTransform == null)
            {
                Debug.LogError("PlayerController: no hay camara asignada ni camara hija.", this);
            }
            else
            {
                cameraTransform.localPosition = new Vector3(0f, cameraHeight, 0f);
                cameraTransform.localRotation = Quaternion.identity;
            }

            if (controls == null)
            {
                Debug.LogError("PlayerController: falta asignar el asset de controles (TsakelControls).", this);
                enabled = false;
                return;
            }

            InputActionMap map = controls.FindActionMap("Player", true);
            moveAction = map.FindAction("Move", true);
            lookMouseAction = map.FindAction("LookMouse", true);
            lookStickAction = map.FindAction("LookStick", true);
        }

        private void OnEnable()
        {
            if (moveAction == null) return;
            moveAction.Enable();
            lookMouseAction.Enable();
            lookStickAction.Enable();
        }

        private void OnDisable()
        {
            if (moveAction == null) return;
            moveAction.Disable();
            lookMouseAction.Disable();
            lookStickAction.Disable();
        }

        private void Start()
        {
            SetCursorLocked(lockCursorOnStart);
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            TryRelockCursor();
            UpdateLook(dt);
            UpdateMovement(dt);
        }

        private void UpdateLook(float dt)
        {
            if (!lookEnabled || cameraTransform == null) return;

            // El mouse solo cuenta con el cursor capturado; el joystick siempre.
            Vector2 mouse = Cursor.lockState == CursorLockMode.Locked
                ? lookMouseAction.ReadValue<Vector2>() * (mouseSensitivity * SensitivityMultiplier)
                : Vector2.zero;
            Vector2 stick = lookStickAction.ReadValue<Vector2>() * (stickSensitivity * SensitivityMultiplier * dt);
            Vector2 look = mouse + stick;
            if (look == Vector2.zero) return;

            transform.Rotate(0f, look.x, 0f, Space.Self);
            pitch = Mathf.Clamp(pitch - look.y, -verticalLimit, verticalLimit);
            cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        private void UpdateMovement(float dt)
        {
            Vector2 input = movementEnabled
                ? Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f)
                : Vector2.zero;

            Vector3 target = (transform.right * input.x + transform.forward * input.y) * moveSpeed;
            float rate = input.sqrMagnitude > 0.0001f ? acceleration : deceleration;
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, target, rate * dt);

            if (controller.isGrounded && verticalVelocity < 0f)
                verticalVelocity = -2f;
            else
                verticalVelocity += gravity * dt;

            Vector3 velocity = horizontalVelocity + Vector3.up * verticalVelocity;
            controller.Move(velocity * dt);
        }

        private void TryRelockCursor()
        {
            if (!relockCursorOnClick || !lookEnabled) return;
            if (Cursor.lockState == CursorLockMode.Locked) return;
            Mouse mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
                SetCursorLocked(true);
        }

        /// <summary>Activa o bloquea solo el caminar. Al bloquear, el jugador se detiene al instante.</summary>
        public void SetMovementEnabled(bool value)
        {
            movementEnabled = value;
            if (!value) horizontalVelocity = Vector3.zero;
        }

        /// <summary>Activa o bloquea solo la mirada (mouse y joystick derecho).</summary>
        public void SetLookEnabled(bool value)
        {
            lookEnabled = value;
        }

        /// <summary>Activa o bloquea caminar y mirar a la vez.</summary>
        public void SetControlsEnabled(bool value)
        {
            SetMovementEnabled(value);
            SetLookEnabled(value);
        }

        /// <summary>Captura u oculta/libera el cursor (para menus o el album).</summary>
        public void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
