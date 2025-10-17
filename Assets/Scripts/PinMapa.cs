using UnityEngine;

[DisallowMultipleComponent]
public class PinMapa : MonoBehaviour
{
    [Header("Objetos a mostrar/ocultar al tocar el pin")]
    [SerializeField] private GameObject[] objetosAR;

    public bool FueActivado { get; private set; } = false;

    void Awake()
    {
        if (objetosAR == null) return;
        foreach (var obj in objetosAR)
        {
            if (obj) obj.SetActive(false);
        }
    }

    /// <summary>
    /// Toca el pin: alterna visibilidad de sus objetos.
    /// Marca como activado la primera vez para el conteo del mapa.
    /// </summary>
    public void OnPinClicked()
    {
        bool hayActivo = false;
        foreach (var obj in objetosAR)
        {
            if (obj && obj.activeSelf) { hayActivo = true; break; }
        }

        if (hayActivo) OcultarObjetos();
        else MostrarObjetos();

        if (!FueActivado)
        {
            FueActivado = true;
            Debug.Log($"[PinMapa] Activado por primera vez: {name}");
        }
    }

    public void MostrarObjetos()
    {
        if (objetosAR == null) return;
        foreach (var obj in objetosAR) if (obj) obj.SetActive(true);
    }

    public void OcultarObjetos()
    {
        if (objetosAR == null) return;
        foreach (var obj in objetosAR) if (obj) obj.SetActive(false);
    }
}
