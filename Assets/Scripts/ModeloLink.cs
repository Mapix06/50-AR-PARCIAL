using UnityEngine;

/// <summary>
/// Objeto interactivo AR o 3D que abre un enlace externo (Drive, web, etc.)
/// Soporta clics en PC y toques táctiles en Android.
/// </summary>
public class ModeloLink : MonoBehaviour
{
    [Header("URL del enlace")]
    [SerializeField] private string url = "https://drive.google.com";

    [Header("Cámara AR / Principal")]
    [SerializeField] private Camera camaraAR;

    [Header("Opciones de Detección")]
    [SerializeField] private LayerMask capasDetectables;
    [SerializeField] private float distanciaMaximaRaycast = 30f;

    [Header("Efectos Visuales (opcional)")]
    [SerializeField] private bool rotarConstantemente = true;
    [SerializeField] private float velocidadRotacion = 50f;
    [SerializeField] private bool animarEscala = true;
    [SerializeField] private float velocidadPulso = 2f;
    [SerializeField] private float escalaMin = 0.9f;
    [SerializeField] private float escalaMax = 1.1f;

    private Vector3 escalaOriginal;
    private Collider miCollider;

    void Start()
    {
        // Cámara
        if (camaraAR == null)
            camaraAR = Camera.main;

        // Collider
        miCollider = GetComponent<Collider>();
        if (miCollider == null)
        {
            miCollider = gameObject.AddComponent<SphereCollider>();
            ((SphereCollider)miCollider).radius = 0.3f;
            Debug.LogWarning("[ObjetoInteractivoEnlace] No tenía Collider. Se añadió automáticamente.");
        }

        escalaOriginal = transform.localScale;
    }

    void Update()
    {
        // Efectos visuales
        if (rotarConstantemente)
            transform.Rotate(Vector3.up, velocidadRotacion * Time.deltaTime);

        if (animarEscala)
        {
            float pulso = Mathf.Lerp(escalaMin, escalaMax,
                (Mathf.Sin(Time.time * velocidadPulso) + 1f) / 2f);
            transform.localScale = escalaOriginal * pulso;
        }

        DetectarToques();
    }

    private void DetectarToques()
    {
        bool toqueDetectado = false;
        Vector2 posicion = Vector2.zero;

        // Toque táctil
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
            {
                toqueDetectado = true;
                posicion = t.position;
            }
        }
        // Click de mouse (Editor o PC)
        else if (Input.GetMouseButtonDown(0))
        {
            toqueDetectado = true;
            posicion = Input.mousePosition;
        }

        if (toqueDetectado)
            RaycastDesdePantalla(posicion);
    }

    private void RaycastDesdePantalla(Vector2 posicion)
    {
        if (camaraAR == null) return;

        Ray ray = camaraAR.ScreenPointToRay(posicion);
        if (Physics.Raycast(ray, out RaycastHit hit, distanciaMaximaRaycast, capasDetectables == 0 ? ~0 : capasDetectables))
        {
            if (hit.collider == miCollider || hit.collider.transform == transform)
            {
                AbrirEnlace();
            }
        }
    }

    private void AbrirEnlace()
    {
        if (string.IsNullOrEmpty(url))
        {
            Debug.LogWarning("[ObjetoInteractivoEnlace] No se ha asignado una URL.");
            return;
        }

        Debug.Log($"🌐 Abriendo enlace: {url}");
        Application.OpenURL(url);
    }

#if UNITY_EDITOR || UNITY_STANDALONE
    void OnMouseDown()
    {
        // Fallback para modo Editor / PC
        AbrirEnlace();
    }
#endif
}
