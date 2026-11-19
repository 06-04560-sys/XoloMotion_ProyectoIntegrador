using UnityEngine;

namespace Tsakel.Environment
{
    /// <summary>
    /// Billboard cilindrico para sprites de vegetacion 2D.
    /// Gira solo alrededor del eje vertical Y (siempre erguido) para que la cara frontal
    /// del sprite mire hacia la camara. No cambia posicion, escala ni altura.
    /// Usa Main Camera (la camara hija de Player) salvo que se asigne otra.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Tsakel/Environment/Billboard Vegetation")]
    public class BillboardVegetation : MonoBehaviour
    {
        [Tooltip("Camara a la que se orienta. Vacio = Main Camera.")]
        [SerializeField] private Camera targetCamera;

        [Tooltip("Giro extra en grados sobre Y. 0 = la cara visible del sprite (-Z local) mira a la camara. Usa 180 si algun sprite se ve al reves.")]
        [SerializeField] private float yawOffset = 0f;

        private static Camera cachedMainCamera;

        private bool tiltCaptured;
        private float tiltX;
        private float tiltZ;

        private void LateUpdate()
        {
            Camera cam = ResolveCamera();
            if (cam == null) return;
            FaceCamera(cam.transform.position);
        }

        /// <summary>Orienta el sprite hacia una posicion de camara, girando solo en Y.</summary>
        public void FaceCamera(Vector3 cameraPosition)
        {
            if (!tiltCaptured)
            {
                // Se conserva la inclinacion original en X y Z (normalmente 0).
                Vector3 e = transform.eulerAngles;
                tiltX = e.x;
                tiltZ = e.z;
                tiltCaptured = true;
            }

            Vector3 away = transform.position - cameraPosition;
            away.y = 0f;
            if (away.sqrMagnitude < 0.0001f) return;

            // El frente visible del sprite es -Z local: se apunta +Z lejos de la camara.
            float yaw = Mathf.Atan2(away.x, away.z) * Mathf.Rad2Deg + yawOffset;
            transform.rotation = Quaternion.Euler(tiltX, yaw, tiltZ);
        }

        private Camera ResolveCamera()
        {
            if (targetCamera != null) return targetCamera;
            if (cachedMainCamera == null) cachedMainCamera = Camera.main;
            return cachedMainCamera;
        }
    }
}
