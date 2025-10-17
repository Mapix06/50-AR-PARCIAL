using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class CatController : MonoBehaviour
{
    [Header("Movimiento del gato")]
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float rotateSpeed = 10f;
    [SerializeField] private float stoppingDistance = 0.2f;

    [Header("Audio de bienvenida")]
    [SerializeField] private AudioClip audioBienvenida;
    [TextArea]
    [SerializeField] private string textoBienvenida;

    private Animator animator;
    private AudioSource audioSource;
    private Camera cam;

    private Vector3? targetPos = null;
    private bool reachedTarget = false;
    private PinMapa pinPendiente = null;

    public static AudioSource audioEnReproduccion;

    void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        cam = Camera.main;

        animator?.SetBool("isWalking", false);
        animator?.SetBool("isTalking", false);

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0.2f;

        // Reproducir bienvenida al inicio
        if (audioBienvenida != null)
            StartCoroutine(ReproducirBienvenida());
    }

    void Update()
    {
        HandleClickInput();
        HandleMovement();
    }

    private void HandleClickInput()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            TrySetTargetFromScreenPoint(Mouse.current.position.ReadValue());

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            TrySetTargetFromScreenPoint(Touchscreen.current.primaryTouch.position.ReadValue());
    }

    private void TrySetTargetFromScreenPoint(Vector2 screenPos)
    {
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            PinMapa pin = hit.collider.GetComponent<PinMapa>();
            if (pin != null)
            {
                targetPos = new Vector3(pin.transform.position.x, transform.position.y, pin.transform.position.z);
                reachedTarget = false;
                pinPendiente = pin;
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

        Vector3 dir = targetPos.Value - transform.position;
        dir.y = 0f;
        if (dir.magnitude > stoppingDistance)
        {
            transform.position += dir.normalized * moveSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), rotateSpeed * Time.deltaTime);
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
            }

            targetPos = null;
        }
    }

    private IEnumerator RotateToCamera()
    {
        if (cam == null) yield break;

        Quaternion start = transform.rotation;
        Quaternion target = Quaternion.LookRotation(cam.transform.position - transform.position);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * rotateSpeed / 2f;
            transform.rotation = Quaternion.Slerp(start, target, t);
            yield return null;
        }
    }

    private IEnumerator ReproducirBienvenida()
    {
        animator?.SetBool("isTalking", true);
        audioSource.clip = audioBienvenida;
        audioSource.Play();

        if (SubtitulosZylo.Instance != null)
            SubtitulosZylo.Instance.MostrarTexto(textoBienvenida);

        yield return new WaitForSeconds(audioBienvenida.length);

        animator?.SetBool("isTalking", false);
    }
}
