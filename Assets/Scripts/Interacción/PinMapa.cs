using UnityEngine;

[DisallowMultipleComponent]
public class PinMapa : MonoBehaviour
{
    [Header("Objetos a mostrar/ocultar al tocar el pin")]
    [SerializeField] private GameObject[] objetosAR;

    public bool FueActivado { get; private set; } = false;

    public string idPin; // ej: "2000_2002"

    void Awake()
    {
        if (objetosAR == null) return;
        foreach (var obj in objetosAR)
            if (obj) obj.SetActive(false);
    }

    public void OnPinClicked()
    {
        bool hayActivo = false;
        foreach (var obj in objetosAR)
            if (obj && obj.activeSelf) { hayActivo = true; break; }

        if (hayActivo) OcultarObjetos();
        else MostrarObjetos();

        if (!FueActivado)
        {
            FueActivado = true;
            Debug.Log($"[PinMapa] Activado por primera vez: {name}");
        }

        // Aquí notificamos al manager
        var manager = FindObjectOfType<PlaneManagerPines>();
        manager?.NotificarPinCompletado(this);
    }

    public void MostrarObjetos()
    {
        if (objetosAR == null) return;
        foreach (var obj in objetosAR)
            if (obj) obj.SetActive(true);
    }

    public void OcultarObjetos()
    {
        if (objetosAR == null) return;
        foreach (var obj in objetosAR)
            if (obj) obj.SetActive(false);
    }
}
