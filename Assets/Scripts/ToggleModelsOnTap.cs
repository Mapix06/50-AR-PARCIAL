using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ToggleModelsOnTap : MonoBehaviour
{
    // Referencia al ARRaycastManager para lanzar rayos en el mundo AR
    [SerializeField] private ARRaycastManager raycastManager;

    // Los modelos que quieres mostrar / ocultar
    [SerializeField] private GameObject[] modelos;

    // Estado de visibilidad
    private bool modelosVisibles = false;

    // Para almacenar resultados del raycast
    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Update()
    {
        // Solo responder al primer toque cuando inicia (TouchPhase.Began)
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                // Raycast desde la pantalla al mundo AR (solo a planos o puntos, no a UI)
                if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon | TrackableType.FeaturePoint))
                {
                    // Tomamos el primer hit
                    Pose hitPose = hits[0].pose;

                    // Verificar si tocó alguno de tus modelos existentes usando un raycast normal de física
                    Ray ray = Camera.main.ScreenPointToRay(touch.position);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit))
                    {
                        // hit.transform es el objeto que tocaste
                        // Si es este objeto (el que tiene este script) o algún hijo, alterna visibilidad
                        if (hit.transform == transform || hit.transform.IsChildOf(transform))
                        {
                            ToggleModelos();
                        }
                    }
                }
            }
        }
    }

    private void ToggleModelos()
    {
        modelosVisibles = !modelosVisibles;
        foreach (GameObject m in modelos)
        {
            m.SetActive(modelosVisibles);
        }
    }
}
