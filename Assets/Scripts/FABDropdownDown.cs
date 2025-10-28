using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FABDropdownDown : MonoBehaviour
{
    [Header("Botones de épocas (hijos del FAB)")]
    [SerializeField] private List<Button> botonesEpocas;

    [Header("Configuración")]
    [SerializeField] private float separacion = 110f;
    [SerializeField] private float duracionAnim = 0.25f;

    [Header("Referencias")]
    [SerializeField] private PlaneManagerPines planeManager;

    private bool abierto = false;
    private bool desbloqueado = false;
    private List<Vector2> posicionesOriginales = new List<Vector2>();

    void Awake()
    {
        // Guarda posiciones iniciales
        foreach (var boton in botonesEpocas)
        {
            RectTransform rect = boton.GetComponent<RectTransform>();
            posicionesOriginales.Add(rect.anchoredPosition);
            boton.gameObject.SetActive(false);
        }

        // Ocultar al inicio (lo activa el PlaneManager)
        gameObject.SetActive(false);
    }

    void Start()
    {
        // Asignar eventos a cada botón
        for (int i = 0; i < botonesEpocas.Count; i++)
        {
            int index = i;
            botonesEpocas[i].onClick.AddListener(() => CambiarEpoca(index));
        }
    }

    public void DesbloquearFAB()
    {
        desbloqueado = true;
        gameObject.SetActive(true);
        Debug.Log("[FABDropdownDown] 🟠 FAB activado permanentemente desde PlaneManagerPines.");

        // Animación sencilla sin LeanTween
        StartCoroutine(AppearanceAnim());
    }

    private IEnumerator AppearanceAnim()
    {
        RectTransform rect = GetComponent<RectTransform>();
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one;
        float t = 0f;

        rect.localScale = startScale;

        while (t < 1f)
        {
            t += Time.deltaTime * 3f; // velocidad
            rect.localScale = Vector3.Lerp(startScale, endScale, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }

        rect.localScale = endScale;
    }



    public void ToggleMenu()
    {
        if (!desbloqueado) return;

        abierto = !abierto;
        StopAllCoroutines();
        StartCoroutine(AnimarBotones(abierto));
    }

    private IEnumerator AnimarBotones(bool abrir)
    {
        float delay = 0.03f;

        for (int i = 0; i < botonesEpocas.Count; i++)
        {
            Button boton = botonesEpocas[i];
            RectTransform rect = boton.GetComponent<RectTransform>();
            Vector2 basePos = posicionesOriginales[i];

            if (abrir)
                boton.gameObject.SetActive(true);

            Vector2 origen = abrir ? basePos : basePos + new Vector2(0, -separacion * (i + 1));
            Vector2 destino = abrir ? basePos + new Vector2(0, -separacion * (i + 1)) : basePos;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / duracionAnim;
                rect.anchoredPosition = Vector2.Lerp(origen, destino, Mathf.SmoothStep(0, 1, t));
                yield return null;
            }

            if (!abrir)
                boton.gameObject.SetActive(false);

            if (abrir) yield return new WaitForSeconds(delay);
        }
    }

    private void CambiarEpoca(int index)
    {
        if (planeManager == null)
        {
            Debug.LogWarning("[FABDropdownDown] No hay referencia a PlaneManagerPines.");
            return;
        }

        var mapas = planeManager.GetMapas();
        if (mapas == null || mapas.Count == 0)
        {
            Debug.LogWarning("[FABDropdownDown] No hay mapas cargados aún.");
            return;
        }

        // Desactiva todos los mapas
        foreach (var mapa in mapas)
            mapa.SetActive(false);

        // Activa solo el seleccionado
        if (index >= 0 && index < mapas.Count)
        {
            mapas[index].SetActive(true);
            Debug.Log($"[FABDropdownDown] Activado mapa {index + 1} → {mapas[index].name}");
        }

        ToggleMenu();
    }
}
