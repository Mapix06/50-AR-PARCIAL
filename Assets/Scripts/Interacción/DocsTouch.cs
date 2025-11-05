using UnityEngine;

public class DocsTouch : MonoBehaviour
{
    [Header("Datos del objeto")]
    public string objetoNombre;
    public string documentoURL;

    [Header("Zoom de exhibición")]
    public float distanciaFrenteCamara = 1.5f;
    public float velocidadMovimiento = 2f;
    public float velocidadEscala = 2f;
    public float velocidadRotacion = 5f;

    private bool estaEnExhibicion = false;
    private bool animando = false;
    private Vector3 escalaOriginal;
    private Vector3 posicionOriginal;
    private Quaternion rotacionOriginal;
    private Vector3 destinoPosicion;
    private Quaternion destinoRotacion;

    void Start()
    {
        escalaOriginal = transform.localScale;
        posicionOriginal = transform.position;
        rotacionOriginal = transform.rotation;
    }

    void Update()
    {
        if (animando)
        {
            transform.position = Vector3.Lerp(transform.position, destinoPosicion, Time.deltaTime * velocidadMovimiento);
            transform.localScale = Vector3.Lerp(transform.localScale, escalaOriginal, Time.deltaTime * velocidadEscala);
            transform.rotation = Quaternion.Slerp(transform.rotation, destinoRotacion, Time.deltaTime * velocidadRotacion);

            if (Vector3.Distance(transform.position, destinoPosicion) < 0.01f &&
                Vector3.Distance(transform.localScale, escalaOriginal) < 0.01f &&
                Quaternion.Angle(transform.rotation, destinoRotacion) < 1f)
            {
                animando = false;
            }
        }
    }

    private void OnMouseDown()
    {
        if (!estaEnExhibicion)
        {
            ActivarExhibicion();
        }
        else
        {
            AbrirDocumento();
        }
    }

    private void ActivarExhibicion()
    {
        estaEnExhibicion = true;
        animando = true;

        Transform cam = Camera.main.transform;
        destinoPosicion = cam.position + cam.forward * distanciaFrenteCamara;

        destinoRotacion = rotacionOriginal;

        var manager = Object.FindFirstObjectByType<PlaneManager>();
        if (manager != null)
        {
            manager.MostrarDatosObjeto(objetoNombre, this);
        }
    }

    private void AbrirDocumento()
    {
        if (!string.IsNullOrEmpty(documentoURL))
        {
            Application.OpenURL(documentoURL);
        }
        else
        {
            Debug.LogWarning($"[DocsTouch] No se asignó URL para el objeto {objetoNombre}.");
        }
    }

    public void SalirDeExhibicion()
    {
        estaEnExhibicion = false;
        animando = false;
        transform.position = posicionOriginal;
        transform.localScale = escalaOriginal;
        transform.rotation = rotacionOriginal;

        var manager = Object.FindFirstObjectByType<PlaneManager>();
        if (manager != null)
        {
            manager.OcultarPanelExhibicion();
        }
    }
}
