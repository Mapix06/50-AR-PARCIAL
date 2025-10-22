using UnityEngine;
using TMPro;

public class DiagnosticoVisual : MonoBehaviour
{
    public TextMeshProUGUI debugText;

    void Start()
    {
        string estado = "";
        estado += $"PanelPreguntas activo: {NewBehaviourScript.instance.panelPreguntas?.activeInHierarchy}\n";
        estado += $"Texto subtítulos: {SubtitulosZylo.Instance?.GetComponentInChildren<TextMeshProUGUI>()?.text}\n";
        estado += $"Botón Reintentar interactable: {NewBehaviourScript.instance.botonReintentar?.interactable}\n";
        debugText.text = estado;
    }
}
