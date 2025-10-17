using UnityEngine;
using TMPro;

public class ZyloSceneConnector : MonoBehaviour
{
    [SerializeField] private CatController zyloController;
    [SerializeField] private TextMeshProUGUI textoSubtituloZylo;

    void Start()
    {
        if (zyloController != null && textoSubtituloZylo != null)
        {
            zyloController.AsignarTextoSubtitulo(textoSubtituloZylo);
        }
    }
}
