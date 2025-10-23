using UnityEngine;
using TMPro;

public class DiagnosticoVisual : MonoBehaviour
{
    public TextMeshProUGUI debugText;

    void Start()
    {
        if (debugText == null)
        {
            Debug.LogError("❌ [DiagnosticoVisual] debugText no está asignado en el Inspector");
            return;
        }

        string estado = "=== DIAGNÓSTICO VISUAL ===\n\n";

        // Verificar NewBehaviourScript.instance
        if (NewBehaviourScript.instance == null)
        {
            estado += "❌ NewBehaviourScript.instance es NULL\n";
        }
        else
        {
            estado += "✅ NewBehaviourScript.instance existe\n";

            // Verificar panelPreguntas
            if (NewBehaviourScript.instance.panelPreguntas == null)
                estado += "❌ panelPreguntas es NULL\n";
            else
                estado += $"✅ PanelPreguntas activo: {NewBehaviourScript.instance.panelPreguntas.activeInHierarchy}\n";

            // Verificar botonReintentar
            if (NewBehaviourScript.instance.botonReintentar == null)
                estado += "❌ botonReintentar es NULL\n";
            else
                estado += $"✅ Botón Reintentar interactable: {NewBehaviourScript.instance.botonReintentar.interactable}\n";
        }

        // Verificar SubtitulosZylo.Instance
        if (SubtitulosZylo.Instance == null)
        {
            estado += "❌ SubtitulosZylo.Instance es NULL\n";
        }
        else
        {
            estado += "✅ SubtitulosZylo.Instance existe\n";

            TextMeshProUGUI subtituloTexto = SubtitulosZylo.Instance.GetComponentInChildren<TextMeshProUGUI>();

            if (subtituloTexto == null)
                estado += "❌ No se encontró TextMeshProUGUI en SubtitulosZylo\n";
            else
                estado += $"✅ Texto subtítulos: '{subtituloTexto.text}'\n";
        }

        debugText.text = estado;
        Debug.Log(estado);
    }
}