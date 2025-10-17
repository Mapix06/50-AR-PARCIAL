using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class CatController : MonoBehaviour
{
    [Header("Movimiento del gato")]
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float rotateSpeed = 10f;
    [SerializeField] private float stoppingDistance = 0.2f;
    [SerializeField] private float raycastMaxDistance = 100f;

    [Header("Audio de bienvenida (solo en el gato)")]
    [SerializeField] private AudioClip audioBienvenida;

    private Animator animator;
    private AudioSource audioSource;
    private Camera cam;

    private Vector3? targetPos = null;
    private bool reachedTarget = false;
    private PinMapa pinPendiente = null;

    // 🔊 Control global del audio actual del pin
    private static AudioSource audioEnReproduccion = null;

    void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        cam = Camera.main;

        animator?.SetBool("isWalking", false);
        animator?.SetBool("isTalking", false);

        // Configurar AudioSource del gato
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0.2f;
        audioSource.volume = 1f;

        // Audio inicial opcional
        if (audioBienvenida != null)
        {
            StartCoroutine(RotateToCamera());
            StartCoroutine(ReproducirDialogoBienvenida(audioBienvenida));
        }
    }

    void Update()
    {
        HandleClickInput();
        HandleMovement();
    }

    private void HandleClickInput()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 screenPos = Mouse.current.position.ReadValue();
            TrySetTargetFromScreenPoint(screenPos);
        }

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            Vector2 screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
            TrySetTargetFromScreenPoint(screenPos);
        }
    }

    private void TrySetTargetFromScreenPoint(Vector2 screenPos)
    {
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, raycastMaxDistance))
        {
            PinMapa pin = hit.collider.GetComponent<PinMapa>();
            if (pin != null)
            {
                Vector3 destination = pin.transform.position;
                destination.y = transform.position.y;
                targetPos = destination;
                reachedTarget = false;
                pinPendiente = pin;

                Debug.Log($"[CatController] Pin tocado: {pin.name} → destino {destination}");
            }
        }
    }

    private void HandleMovement()
    {
        if (!targetPos.HasValue)
        {
            animator?.SetBool("isWalking", false);
            return;
        }

        Vector3 target = targetPos.Value;
        Vector3 dir = target - transform.position;
        dir.y = 0f;
        float dist = dir.magnitude;

        if (dist > stoppingDistance)
        {
            transform.position += dir.normalized * moveSpeed * Time.deltaTime;

            Quaternion targRot = Quaternion.LookRotation(dir.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targRot, rotateSpeed * Time.deltaTime);

            animator?.SetBool("isWalking", true);
            return;
        }

        if (!reachedTarget)
        {
            reachedTarget = true;
            animator?.SetBool("isWalking", false);
            StartCoroutine(RotateToCamera());

            if (pinPendiente != null)
            {
                pinPendiente.OnPinClicked();

                // Reproducir audio del pin (solo uno activo)
                AudioSource pinAudio = pinPendiente.GetComponent<AudioSource>();
                if (pinAudio != null && pinAudio.clip != null)
                {
                    // 🔇 Detiene el audio anterior si estaba sonando
                    if (audioEnReproduccion != null && audioEnReproduccion.isPlaying)
                    {
                        audioEnReproduccion.Stop();
                    }

                    // 🔊 Inicia el nuevo audio
                    audioEnReproduccion = pinAudio;
                    audioEnReproduccion.Play();
                    StartCoroutine(EsperarYNotificarFinAudio(audioEnReproduccion, pinPendiente));
                }
                else
                {
                    // Si no hay audio en el pin → notificar directamente
                    NotificarFinDePin(pinPendiente);
                }
            }

            targetPos = null;
        }
    }

    private System.Collections.IEnumerator RotateToCamera()
    {
        if (cam == null) yield break;

        Vector3 lookDir = cam.transform.position - transform.position;
        lookDir.y = 0f;

        Quaternion startRot = transform.rotation;
        Quaternion targetRot = Quaternion.LookRotation(lookDir);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * (rotateSpeed / 2f);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        transform.rotation = targetRot;
    }

    private System.Collections.IEnumerator ReproducirDialogoBienvenida(AudioClip clip)
    {
        if (clip == null) yield break;

        animator?.SetBool("isTalking", true);
        audioSource.clip = clip;
        audioSource.Play();

        yield return new WaitForSeconds(clip.length);

        animator?.SetBool("isTalking", false);
    }

    private System.Collections.IEnumerator EsperarYNotificarFinAudio(AudioSource fuente, PinMapa pin)
    {
        // Espera hasta que ese audio termine
        yield return new WaitWhile(() => fuente != null && fuente.isPlaying);

        if (audioEnReproduccion == fuente)
            audioEnReproduccion = null;

        // Cuando termina el audio, procesar el pin
        NotificarFinDePin(pin);
    }

    /// <summary>
    /// Procesa el pin después de que termine el audio
    /// </summary>
    private void NotificarFinDePin(PinMapa pin)
    {
        if (pin == null) return;

        // 1️⃣ Mostrar las preguntas interactivas
        pin.MostrarPreguntasInteractivas();

        // 2️⃣ Notificar al PlaneManager (para cambio de mapa)
        var manager = FindObjectOfType<PlaneManagerPines>();
        manager?.NotificarPinCompletado(pin);
    }
}