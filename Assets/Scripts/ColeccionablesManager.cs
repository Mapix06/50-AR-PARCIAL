using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class ColeccionablesManager : MonoBehaviour
{
    public static ColeccionablesManager Instance;

    [System.Serializable]
    public class Coleccionable
    {
        public int indiceEpoca;
        public string nombreEpoca;
        public GameObject prefabObjeto;
        public bool recolectado;
        [HideInInspector] public GameObject instanciaActual;
    }

    [Header("Configuración")]
    [SerializeField] private List<Coleccionable> coleccionables = new List<Coleccionable>();
    [SerializeField] private float escalaObjetos = 0.05f; 
    [SerializeField] private Vector3 posicionLateral = new Vector3(0.3f, -0.2f, 0.5f);
    [SerializeField] private float espaciado = 0.15f;

    [Header("Efectos")]
    [SerializeField] private AudioClip sonidoRecolectar;

    private AudioSource audioSource;

    void Awake()
    {
        Instance = this;
        audioSource = GetComponent<AudioSource>();
        CargarProgreso();
    }

    public void RecolectarPorEpoca(int indiceEpoca)
    {
        Coleccionable colec = coleccionables.Find(c => c.indiceEpoca == indiceEpoca);

        if (colec != null && !colec.recolectado)
        {
            colec.recolectado = true;
            InstanciarObjeto(colec);
            GuardarProgreso();

            Debug.Log($" Coleccionable {colec.nombreEpoca} desbloqueado!");
        }
    }

    private void InstanciarObjeto(Coleccionable colec)
    {
        if (colec.prefabObjeto == null || Camera.main == null) return;

        // Calcular posición
        int cantidadPrevios = coleccionables.FindAll(c => c.indiceEpoca < colec.indiceEpoca && c.recolectado).Count;
        Vector3 offset = posicionLateral;
        offset.x += cantidadPrevios * espaciado;

        Transform cam = Camera.main.transform;
        Vector3 posicion = cam.position + cam.TransformDirection(offset);

        // Crear objeto
        GameObject obj = Instantiate(colec.prefabObjeto, posicion, Quaternion.identity, cam);
        obj.transform.localScale = Vector3.one * escalaObjetos; 
        obj.transform.LookAt(cam);
        obj.transform.Rotate(0, 180, 0);

        colec.instanciaActual = obj;

        // Sonido
        if (audioSource != null && sonidoRecolectar != null)
            audioSource.PlayOneShot(sonidoRecolectar);

        // Animación simple
        StartCoroutine(AnimarAparicion(obj));
    }

    private IEnumerator AnimarAparicion(GameObject obj)
    {
        Vector3 escalaFinal = obj.transform.localScale;
        obj.transform.localScale = Vector3.zero;

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 2f;
            obj.transform.localScale = Vector3.Lerp(Vector3.zero, escalaFinal, t);
            yield return null;
        }
    }

    private void GuardarProgreso()
    {
        foreach (var c in coleccionables)
            PlayerPrefs.SetInt($"colec_{c.indiceEpoca}", c.recolectado ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void CargarProgreso()
    {
        foreach (var c in coleccionables)
        {
            c.recolectado = PlayerPrefs.GetInt($"colec_{c.indiceEpoca}", 0) == 1;
            if (c.recolectado && c.prefabObjeto != null && Camera.main != null)
            {
                int previos = coleccionables.FindAll(x => x.indiceEpoca < c.indiceEpoca && x.recolectado).Count;
                Vector3 offset = posicionLateral;
                offset.x += previos * espaciado;

                Transform cam = Camera.main.transform;
                Vector3 pos = cam.position + cam.TransformDirection(offset);

                c.instanciaActual = Instantiate(c.prefabObjeto, pos, Quaternion.identity, cam);
                c.instanciaActual.transform.localScale = Vector3.one * escalaObjetos; 
                c.instanciaActual.transform.LookAt(cam);
                c.instanciaActual.transform.Rotate(0, 180, 0);
            }
        }
    }

    [ContextMenu("Resetear Todo")]
    public void ResetearTodo()
    {
        foreach (var c in coleccionables)
        {
            if (c.instanciaActual != null) Destroy(c.instanciaActual);
            c.recolectado = false;
            PlayerPrefs.DeleteKey($"colec_{c.indiceEpoca}");
        }
        PlayerPrefs.Save();
    }
}