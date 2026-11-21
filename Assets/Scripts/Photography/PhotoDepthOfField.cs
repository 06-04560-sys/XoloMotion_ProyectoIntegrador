using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Tsakel.Game
{
    /// <summary>
    /// Profundidad de campo de la camara fotografica: al levantar la camara el fondo se desenfoca
    /// y el foco sigue al animal del visor (o a lo que haya al centro). El zoom acorta la profundidad
    /// de campo como un teleobjetivo. La foto del album sale con el mismo desenfoque.
    /// Usa un Volume local de esta escena; su peso sigue a PhotoCameraController.Amount.
    /// </summary>
    [RequireComponent(typeof(Volume))]
    public class PhotoDepthOfField : MonoBehaviour
    {
        [SerializeField] private PhotoCameraController photoCamera;
        [Tooltip("Distancia de foco cuando no hay animal ni nada al centro (m).")]
        [SerializeField] private float defaultFocus = 12f;
        [Tooltip("Distancia focal a zoom x1 (mm). Se multiplica por el zoom.")]
        [SerializeField] private float baseFocalLength = 50f;
        [SerializeField] private float aperture = 2f;
        [Tooltip("Que tan rapido sigue el foco (1/s).")]
        [SerializeField] private float focusSpeed = 8f;
        [SerializeField] private float maxRayDistance = 60f;

        private Volume volume;
        private DepthOfField dof;
        private float focus;

        private void Awake()
        {
            volume = GetComponent<Volume>();
            // copia en runtime para no modificar el asset del perfil
            VolumeProfile profile = volume.profile;
            if (!profile.TryGet(out dof)) dof = profile.Add<DepthOfField>(true);
            dof.mode.Override(DepthOfFieldMode.Bokeh);
            dof.aperture.Override(aperture);
            dof.focalLength.Override(baseFocalLength);
            dof.focusDistance.Override(defaultFocus);
            volume.weight = 0f;
            focus = defaultFocus;
            if (photoCamera == null) photoCamera = FindFirstObjectByType<PhotoCameraController>();
        }

        private void LateUpdate()
        {
            if (photoCamera == null) return;
            float amount = photoCamera.Amount;
            volume.weight = amount;
            if (amount <= 0.001f)
            {
                focus = defaultFocus;
                return;
            }

            float target = defaultFocus;
            if (photoCamera.Live.Animal != null && photoCamera.Live.Distance > 0f)
            {
                target = photoCamera.Live.Distance;
            }
            else
            {
                Camera cam = Camera.main;
                RaycastHit hit;
                if (cam != null && Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, maxRayDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                    target = hit.distance;
            }

            focus = Mathf.Lerp(focus, target, 1f - Mathf.Exp(-focusSpeed * Time.unscaledDeltaTime));
            dof.focusDistance.value = Mathf.Max(0.3f, focus);
            dof.focalLength.value = Mathf.Clamp(baseFocalLength * photoCamera.Zoom, 1f, 300f);
        }
    }
}
