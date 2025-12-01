using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;

[RequireComponent(typeof(ARPlaneManager))]
public class PlaneManager : MonoBehaviour
{
    [Header("AR")]
    [SerializeField] private ARPlaneManager arPlaneManager;
    [SerializeField] private ARRaycastManager arRaycastManager;

    [Header("Prefabs principales")]
    [SerializeField] private GameObject catPrefab;
    [Tooltip("Prefabs de mapas (máx. 5)")]
    [SerializeField] private List<GameObject> mapPrefabs = new List<GameObject>();

    [Header("UI de exhibición")]
    [SerializeField] private GameObject panelExhibicion;
    [SerializeField] private TextMeshProUGUI textoTituloObjeto;
    [SerializeField] private Button botonSalirExhibicion;

    private DocsTouch objetoActualEnExhibicion;

    [Header("Posiciones fijas relativas al gato")]
    [SerializeField]
    private List<Vector3> mapOffsets = new List<Vector3>()
    {
        new Vector3(0f, 0f, 0.5f),
        new Vector3(0.5f, 0f, 0f),
        new Vector3(0.5f, 0f, 0f),
        new Vector3(0.5f, 0f, 0f),
        new Vector3(0.5f, 0f, 0f)
    };

    [Header("Opciones")]
    [SerializeField, Range(0f, 5f)] private float distanceFromCamera = 2f;
    [SerializeField] private bool verbose = true;

    [Header("Configuración de Avance")]
    [SerializeField] private float tiempoEsperaAntesDeObjeto = 1.5f;

    [Header("Espaciado entre pines")]
    [SerializeField] private float distanciaEntrePines = 1.5f;

    private bool todosPinesCompletados = false;
    public bool TodosPinesCompletados => todosPinesCompletados;

    private GameObject catInstance;
    private CatController catController;
    private readonly List<GameObject> mapInstances = new List<GameObject>();
    private int currentMapIndex = 0;
    private int currentPinIndex = 0;
    private bool contentPlaced = false;
    private bool listoParaAvanzar = false;

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    [Header("Botón FAB Épocas")]
    [SerializeField] private GameObject fabButton;

    void Awake()
    {
        if (!arPlaneManager)
            arPlaneManager = GetComponent<ARPlaneManager>();

        if (!arRaycastManager)
            arRaycastManager = FindFirstObjectByType<ARRaycastManager>();
    }

    void Start()
    {
        arPlaneManager.requestedDetectionMode = PlaneDetectionMode.Horizontal;

        if (verbose)
            Debug.Log("[PlaneManager] Buscando plano horizontal frente al usuario...");
    }

    void Update()
    {
        if (contentPlaced) return;
        TryPlaceContentInFrontOfUser();
    }

    private void TryPlaceContentInFrontOfUser()
    {
        if (arRaycastManager == null)
        {
            Debug.LogError("[PlaneManager] ARRaycastManager no encontrado.");
            return;
        }

        Transform cam = Camera.main?.transform;
        if (cam == null)
        {
            Debug.LogError("[PlaneManager] No se encontró la cámara principal.");
            return;
        }

        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

        if (arRaycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            ARRaycastHit hit = hits[0];
            Vector3 forward = cam.forward;
            forward.y = 0;
            forward.Normalize();

            Vector3 targetPosition = cam.position + forward * distanceFromCamera;
            targetPosition.y = hit.pose.position.y;

            PlaceContentAtPosition(targetPosition);

            arPlaneManager.requestedDetectionMode = PlaneDetectionMode.None;
            foreach (var plane in arPlaneManager.trackables)
                plane.gameObject.SetActive(false);

            contentPlaced = true;
        }
    }

    private void PlaceContentAtPosition(Vector3 position)
    {
        if (catPrefab == null)
        {
            Debug.LogWarning("[PlaneManager] Asigna el prefab del gato.");
            return;
        }

        Transform cam = Camera.main?.transform;
        if (cam == null)
        {
            Debug.LogError("[PlaneManager] No se encontró la cámara principal.");
            return;
        }

        // Colocar el gato
        Vector3 directionToCat = cam.position - position;
        directionToCat.y = 0;
        Quaternion catRotation = Quaternion.LookRotation(directionToCat);
        catInstance = Instantiate(catPrefab, position, catRotation);
        catController = catInstance.GetComponent<CatController>();

        if (verbose)
            Debug.Log($"[PlaneManager] Gato colocado en: {position}");

        if (mapPrefabs.Count == 0 || mapPrefabs[0] == null)
        {
            Debug.LogError("[PlaneManager] No hay prefabs de mapas asignados.");
            return;
        }

        string[] decadas = { "70s", "80s", "90s", "2000s", "2010s" };

        // === PASO 1: Colocar el primer mapa (70s) detrás del gato ===
        Vector3 offset = (0 < mapOffsets.Count) ? mapOffsets[0] : new Vector3(0f, 0f, 0.5f);
        Vector3 firstMapPos = catInstance.transform.position + catInstance.transform.TransformDirection(offset);
        firstMapPos.y = position.y;

        // Rotación: el mapa mira hacia el gato
        Vector3 lookDir = catInstance.transform.position - firstMapPos;
        lookDir.y = 0;
        Quaternion rotacionHaciaGato = Quaternion.LookRotation(lookDir);

        GameObject firstMap = Instantiate(mapPrefabs[0], firstMapPos, rotacionHaciaGato);
        firstMap.name = "Map_1_70s";

        Transform primerPin = EncontrarPinPrincipalPorNombre(firstMap);
        if (primerPin == null)
        {
            Debug.LogError("[PlaneManager] ❌ No se encontró pin principal en 70s");
            Destroy(firstMap);
            return;
        }

        // REFERENCIAS DEL PRIMER MAPA (70s - AZUL)
        Vector3 posicionPrimerMapa = firstMap.transform.position;
        Vector3 posicionPinReferencia = primerPin.position;
        float yReferencia = firstMap.transform.position.y;
        Quaternion rotacionReferencia = firstMap.transform.rotation;

        // 🔥 OBTENER POSICIÓN ORIGINAL DEL PIN EN EL PREFAB (sin instanciar)
        Transform pinPrefabOriginal = EncontrarPinPrincipalPorNombre(mapPrefabs[0]);
        Vector3 posicionOriginalPinPrefab70s = Vector3.zero;
        if (pinPrefabOriginal != null)
        {
            posicionOriginalPinPrefab70s = pinPrefabOriginal.position;
            Debug.Log($"[PlaneManager] Posición original del pin en prefab 70s: {posicionOriginalPinPrefab70s}");
        }

        Debug.Log($"[PlaneManager] === CONFIGURACIÓN BASE (70s - AZUL) ===");
        Debug.Log($"  Posición gato: {catInstance.transform.position}");
        Debug.Log($"  Posición mapa 70s: {firstMap.transform.position}");
        Debug.Log($"  Posición PIN 70s (REFERENCIA): {posicionPinReferencia}");
        Debug.Log($"  Rotación de referencia: {rotacionReferencia.eulerAngles}");

        firstMap.SetActive(false);
        mapInstances.Add(firstMap);

        // === PASO 2: Colocar mapas usando sus posiciones originales relativas al de 70s ===
        for (int i = 1; i < mapPrefabs.Count && i < 5; i++)
        {
            if (mapPrefabs[i] == null)
            {
                Debug.LogWarning($"[PlaneManager] Prefab {i} es NULL");
                continue;
            }

            Debug.Log($"\n[{decadas[i]}] === Procesando mapa {i + 1} ===");

            // 🔥 OBTENER PIN ORIGINAL DEL PREFAB (sin instanciar)
            Transform pinPrefabActual = EncontrarPinPrincipalPorNombre(mapPrefabs[i]);

            if (pinPrefabActual == null)
            {
                Debug.LogError($"[{decadas[i]}] ❌ No se encontró pin en el prefab");
                continue;
            }

            // 🔥 CALCULAR OFFSET ENTRE EL PIN DE ESTE PREFAB Y EL PIN DEL PREFAB 70s
            Vector3 offsetEntrePinesPrefab = pinPrefabActual.position - posicionOriginalPinPrefab70s;
            Debug.Log($"[{decadas[i]}] Offset original entre pines (en prefabs): {offsetEntrePinesPrefab}");

            // 🔥 APLICAR ROTACIÓN al offset
            Vector3 offsetRotado = rotacionHaciaGato * offsetEntrePinesPrefab;
            Debug.Log($"[{decadas[i]}] Offset rotado: {offsetRotado}");

            // 🔥 CALCULAR POSICIÓN DEL PIN EN LA ESCENA
            Vector3 targetPinPosition = posicionPinReferencia + offsetRotado;
            targetPinPosition.y = yReferencia;
            Debug.Log($"[{decadas[i]}] Target pin position: {targetPinPosition}");

            // Instanciar el mapa
            GameObject nuevoMapa = Instantiate(mapPrefabs[i], firstMap.transform.position, rotacionReferencia);
            nuevoMapa.name = $"Map_{i + 1}_{decadas[i]}";

            Transform pinNuevo = EncontrarPinPrincipalPorNombre(nuevoMapa);

            if (pinNuevo != null)
            {
                Debug.Log($"[{decadas[i]}] Pin encontrado: {pinNuevo.name}");

                // 🔥 CALCULAR OFFSET del pin respecto a la raíz del mapa instanciado
                Vector3 offsetPinWorld = pinNuevo.position - nuevoMapa.transform.position;
                Debug.Log($"[{decadas[i]}] Offset pin->raíz (world): {offsetPinWorld}");

                // 🔥 MOVER LA RAÍZ para que el pin quede en targetPinPosition
                Vector3 nuevaPosicionRaiz = targetPinPosition - offsetPinWorld;
                nuevaPosicionRaiz.y = yReferencia;

                nuevoMapa.transform.position = nuevaPosicionRaiz;

                Debug.Log($"[{decadas[i]}] Nueva posición raíz: {nuevoMapa.transform.position}");
                Debug.Log($"[{decadas[i]}] Nueva posición pin: {pinNuevo.position}");

                // VERIFICACIÓN
                float distPinDesdeReferencia = Vector3.Distance(posicionPinReferencia, pinNuevo.position);
                Debug.Log($"[{decadas[i]}] ✅ Distancia pin desde 70s: {distPinDesdeReferencia:F3}m");

                float distRaices = Vector3.Distance(firstMap.transform.position, nuevoMapa.transform.position);
                Debug.Log($"[{decadas[i]}] Distancia entre raíces: {distRaices:F3}m");

                if (distRaices < 0.1f)
                {
                    Debug.LogError($"[{decadas[i]}] ⚠️⚠️⚠️ ADVERTENCIA: Mapas muy cercanos");
                }

                nuevoMapa.SetActive(false);
                mapInstances.Add(nuevoMapa);
            }
            else
            {
                Debug.LogError($"[PlaneManager] ❌ No se encontró pin en la instancia de {decadas[i]}");
                Destroy(nuevoMapa);
            }
        }

        // === VERIFICACIÓN FINAL ===
        Debug.Log("\n[PlaneManager] === VERIFICACIÓN FINAL DE ALINEACIÓN ===");
        for (int i = 0; i < mapInstances.Count; i++)
        {
            Transform pin = EncontrarPinPrincipalPorNombre(mapInstances[i]);
            if (pin != null)
            {
                float dist = Vector3.Distance(posicionPinReferencia, pin.position);
                Debug.Log($"  {decadas[i]}: Pin en {pin.position}, distancia desde 70s = {dist:F3}m");
            }
        }

        if (mapInstances.Count > 0)
        {
            currentMapIndex = 0;
            currentPinIndex = 0;
            MostrarMapaYPrimerPin();
        }
    }

    private Transform EncontrarPinPrincipalPorNombre(GameObject mapa)
    {
        Transform[] todosLosHijos = mapa.GetComponentsInChildren<Transform>(true);

        foreach (Transform hijo in todosLosHijos)
        {
            if (hijo.name.Contains("PinPrincipal"))
            {
                return hijo;
            }
        }

        Debug.LogError($"[EncontrarPinPrincipal] ❌ NO encontrado en '{mapa.name}'");
        return null;
    }

    public bool PuedeAvanzar()
    {
        return listoParaAvanzar;
    }

    public void NotificarPinCompletado(PinMapa pin)
    {
        if (pin == null) return;

        if (verbose)
            Debug.Log($"[PlaneManager] Pin completado: {pin.name} (orden {pin.OrdenPin})");

        if (currentMapIndex >= mapInstances.Count) return;

        GameObject currentMap = mapInstances[currentMapIndex];
        PinMapa[] pins = currentMap.GetComponentsInChildren<PinMapa>(true);

        System.Array.Sort(pins, (a, b) => a.OrdenPin.CompareTo(b.OrdenPin));

        if (currentPinIndex + 1 < pins.Length)
        {
            currentPinIndex++;
            pins[currentPinIndex].gameObject.SetActive(true);

            if (verbose)
                Debug.Log($"[PlaneManager] Activando siguiente pin: {pins[currentPinIndex].name}, {currentPinIndex + 1}/{pins.Length}");
        }
        else
        {
            if (verbose)
                Debug.Log($"[PlaneManager] 🎉 Todos los pines del mapa {currentMapIndex + 1} completados.");

            StartCoroutine(SecuenciaCompletarMapa(pins));
        }
    }

    private IEnumerator SecuenciaCompletarMapa(PinMapa[] pins)
    {
        listoParaAvanzar = true;
        Debug.Log("[DEBUG] SecuenciaCompletarMapa started. listoParaAvanzar set to true.");

        MarcarTodosPinesComoCompletados(pins);
        Debug.Log("[DEBUG] Pins marked complete.");
        yield return new WaitForSeconds(0.3f);

        PanelPreguntasZylo panel = FindFirstObjectByType<PanelPreguntasZylo>(FindObjectsInactive.Include);
        panel?.CerrarTodo();
        Debug.Log("[DEBUG] Panel closed.");
        yield return new WaitForSeconds(0.2f);

        Debug.Log("[DEBUG] Waiting 1.5s before activating object...");
        yield return new WaitForSeconds(tiempoEsperaAntesDeObjeto);

        GameObject mapaActual = mapInstances[currentMapIndex];
        Debug.Log($"[DEBUG] Current map: {mapaActual?.name}, active: {mapaActual?.activeSelf}");

        ObjetoInteractivoCambioMapa objetoAvanzar = mapaActual.GetComponentInChildren<ObjetoInteractivoCambioMapa>(true);
        Debug.Log($"[DEBUG] ObjetoInteractivoCambioMapa found: {objetoAvanzar != null}, name: {objetoAvanzar?.name}, current active: {objetoAvanzar?.gameObject.activeSelf}");

        if (objetoAvanzar != null)
        {
            objetoAvanzar.gameObject.SetActive(true);
            Debug.Log("[DEBUG] ObjetoInteractivoCambioMapa ACTIVATED!");
        }
        else
        {
            Debug.LogWarning("[DEBUG] ObjetoInteractivoCambioMapa NOT FOUND in map.");
        }

        Debug.Log("[DEBUG] SecuenciaCompletarMapa completed.");
    }

    private void MarcarTodosPinesComoCompletados(PinMapa[] pins)
    {
        foreach (var pin in pins)
        {
            Transform letrero = pin.transform.Find("Letrero");
            if (letrero != null)
            {
                letrero.gameObject.SetActive(false);
            }

            if (verbose)
                Debug.Log($"[PlaneManager] ✓ Pin {pin.name} marcado como completado.");
        }
    }

    public void OnObjetoAvanzarClickeado()
    {
        Debug.Log($"[PlaneManager] 🎯 OnObjetoAvanzarClickeado() llamado. listoParaAvanzar = {listoParaAvanzar}");

        if (!listoParaAvanzar)
        {
            Debug.LogWarning("[PlaneManager] ⚠️ No está listo para avanzar. Abortando.");
            return;
        }

        Debug.Log("[PlaneManager] 👆 Click en objeto para avanzar CONFIRMADO.");

        if (currentMapIndex < mapInstances.Count)
        {
            GameObject mapaActual = mapInstances[currentMapIndex];
            Debug.Log($"[PlaneManager] 🗺️ Buscando ObjetoInteractivoCambioMapa en mapa {currentMapIndex}");

            ObjetoInteractivoCambioMapa objetoAvanzar = mapaActual.GetComponentInChildren<ObjetoInteractivoCambioMapa>(true);

            if (objetoAvanzar != null)
            {
                Debug.Log($"[PlaneManager] 🔘 Desactivando objeto: {objetoAvanzar.name}");
                objetoAvanzar.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning("[PlaneManager] ⚠️ No se encontró ObjetoInteractivoCambioMapa para desactivar.");
            }
        }
        else
        {
            Debug.LogWarning($"[PlaneManager] ⚠️ currentMapIndex ({currentMapIndex}) fuera de rango.");
        }

        Debug.Log("[PlaneManager] 🚀 Llamando a AvanzarAlSiguienteMapa()...");
        AvanzarAlSiguienteMapa();
    }

    private void OcultarMapaActual()
    {
        if (currentMapIndex >= mapInstances.Count) return;

        GameObject mapaActual = mapInstances[currentMapIndex];
        mapaActual.SetActive(false);

        if (verbose)
            Debug.Log($"[PlaneManager] 🙈 Mapa {currentMapIndex + 1} ocultado.");
    }

    public void AvanzarAlSiguienteMapa()
    {
        if (verbose)
            Debug.Log($"[PlaneManager] 🔄 Cambio de mapa {currentMapIndex + 1} → {currentMapIndex + 2}");

        OcultarMapaActual();
        listoParaAvanzar = false;

        currentMapIndex++;
        currentPinIndex = 0;

        if (currentMapIndex < mapInstances.Count)
        {
            if (catController != null)
            {
                catController.DetenerMovimiento();
            }

            StartCoroutine(CambiarANuevoMapa());
        }
        else
        {
            Debug.Log("[PlaneManager] 🎊 ¡Todos los mapas completados!");
            todosPinesCompletados = true;
            ActivarFABSiUltimoMapa();
        }
    }

    private IEnumerator CambiarANuevoMapa()
    {
        yield return new WaitForEndOfFrame();

        MostrarMapaYPrimerPin();

        if (verbose)
            Debug.Log($"[PlaneManager] ✅ Cambio a mapa {currentMapIndex + 1} completado");
    }

    public List<GameObject> GetMapas() => mapInstances;
    public int GetCurrentMapIndex() => currentMapIndex;

    public List<string> GetPinesRecorridos(int mapIndex)
    {
        List<string> lista = new List<string>();
        if (mapIndex >= mapInstances.Count) return lista;

        PinMapa[] pins = mapInstances[mapIndex].GetComponentsInChildren<PinMapa>(true);
        System.Array.Sort(pins, (a, b) => a.OrdenPin.CompareTo(b.OrdenPin));

        for (int i = 0; i <= currentPinIndex && i < pins.Length; i++)
        {
            lista.Add(pins[i].name);
        }
        return lista;
    }

    public void IrAlPin(int mapIndex, string pinName)
    {
        if (mapIndex >= mapInstances.Count) return;

        GameObject mapa = mapInstances[mapIndex];
        mapa.SetActive(true);

        PinMapa[] pins = mapa.GetComponentsInChildren<PinMapa>(true);
        foreach (var pin in pins)
        {
            pin.gameObject.SetActive(pin.name == pinName);
        }

        currentMapIndex = mapIndex;
        Debug.Log($"[PlaneManager] Regresando a {pinName} en {ObtenerNombreMapa(mapIndex)}");
    }

    private void MostrarMapaYPrimerPin()
    {
        if (currentMapIndex >= mapInstances.Count) return;

        GameObject currentMap = mapInstances[currentMapIndex];
        currentMap.SetActive(true);

        PinMapa[] pins = currentMap.GetComponentsInChildren<PinMapa>(true);
        if (pins.Length == 0)
        {
            Debug.LogWarning($"[PlaneManager] El mapa {currentMapIndex + 1} no tiene pines.");
            return;
        }

        System.Array.Sort(pins, (a, b) => a.OrdenPin.CompareTo(b.OrdenPin));

        foreach (var pin in pins)
            pin.gameObject.SetActive(false);

        if (currentPinIndex < pins.Length)
        {
            pins[currentPinIndex].gameObject.SetActive(true);

            if (verbose)
                Debug.Log($"[PlaneManager] Mostrando mapa {currentMapIndex + 1}, pin {pins[currentPinIndex].name}, {currentPinIndex + 1}/{pins.Length}");
        }
    }

    private string ObtenerNombreMapa(int index)
    {
        string[] nombres = { "70s", "80s", "90s", "2000s", "2010s" };
        return (index >= 0 && index < nombres.Length) ? nombres[index] : $"Mapa {index + 1}";
    }

    private void ActivarFABSiUltimoMapa()
    {
        if (currentMapIndex >= mapInstances.Count && todosPinesCompletados)
        {
            if (fabButton != null)
            {
                fabButton.SetActive(true);
                Debug.Log("[PlaneManager] 🎯 FAB activado: todas las épocas completadas.");
            }
            else
            {
                Debug.LogWarning("[PlaneManager] No se asignó el FABButton en el inspector.");
            }
        }
    }

    public void MostrarDatosObjeto(string nombre, DocsTouch objeto)
    {
        if (textoTituloObjeto != null)
            textoTituloObjeto.text = nombre;

        objetoActualEnExhibicion = objeto;
        panelExhibicion?.SetActive(true);
        botonSalirExhibicion?.gameObject.SetActive(true);

        var btn = botonSalirExhibicion.GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() =>
        {
            objetoActualEnExhibicion?.SalirDeExhibicion();
        });

        Debug.Log($"[PlaneManager] Mostrando objeto: {nombre}");
    }
    //Mostrar coleccionable sin cerrar el panel de preguntas
    public void MostrarColeccionableSinAvanzar()
    {
        if (currentMapIndex >= mapInstances.Count) return;

        GameObject mapaActual = mapInstances[currentMapIndex];
        ObjetoInteractivoCambioMapa objetoAvanzar = mapaActual.GetComponentInChildren<ObjetoInteractivoCambioMapa>(true);

        if (objetoAvanzar != null)
        {
            objetoAvanzar.gameObject.SetActive(true);
            listoParaAvanzar = true;
            Debug.Log("[PlaneManager] 🎁 Coleccionable activado (panel de preguntas sigue abierto).");
        }
        else
        {
            Debug.LogWarning("[PlaneManager] No se encontró ObjetoInteractivoCambioMapa.");
        }
    }
    public void OcultarPanelExhibicion()
    {
        if (panelExhibicion != null)
            panelExhibicion.SetActive(false);

        if (botonSalirExhibicion != null)
            botonSalirExhibicion.gameObject.SetActive(false);

        if (textoTituloObjeto != null)
            textoTituloObjeto.text = "";

        objetoActualEnExhibicion = null;

        if (verbose)
            Debug.Log("[PlaneManager] Panel de exhibición ocultado.");
    }
}