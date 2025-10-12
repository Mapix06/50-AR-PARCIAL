using UnityEngine;

public class PinMapa : MonoBehaviour
{
    public Transform targetPoint; // punto donde el gato debe caminar
    public GameObject[] objetosAR; // objetos a activar al tocar el pin

    public void OnPinClicked()
    {
        Debug.Log($"[PinMapa] Activado: {gameObject.name}");

        // Activar objetos relacionados (pueden ser hijos del pin o del plano AR)
        foreach (var obj in objetosAR)
        {
            if (obj != null)
                obj.SetActive(true);
        }

        // (Opcional) reproducir efectos, animaciones o sonidos
    }
}