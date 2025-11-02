using UnityEngine;
using UnityEngine.EventSystems;

public class ModeloLink : MonoBehaviour, IPointerClickHandler
{
    [Header("URL del documento o página")]
    [SerializeField] private string url = "https://drive.google.com";

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!string.IsNullOrEmpty(url))
        {
            Application.OpenURL(url);
            Debug.Log($"🌐 Abriendo enlace: {url}");
        }
        else
        {
            Debug.LogWarning("[ModeloLink] ⚠️ No se asignó ninguna URL en el inspector.");
        }
    }
}
