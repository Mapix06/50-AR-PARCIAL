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

    [Header("Audio de bienvenida")]
    [SerializeField] private AudioClip audioBienvenida;

    private Animator animator;
    private AudioSource audioSource;
    private Camera cam;
    private Vector3? targetPos = null;
    private bool reachedTarget = false;
    private bool estaHablando = false;
    private AudioClip audioPendiente = null;

    void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        cam = Camera.main;

        if (animator != null)
        {
            animator.Rebind();
            animator.SetBool("isWalking", false);
            animator.SetBool("isTalking", false);
        }

        // 🎬 Reproduce audio de bienvenida al iniciar
        if (audioBienvenida != null)
        {
            StartCoroutine(RotateToCamera());
            StartCoroutine(ReproducirDialogo(audioBienvenida));
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
            GameObject clickedObject = hit.collider.gameObject;
            PinMapa pin = clickedObject.GetComponent<PinMapa>();

            if (pin != null)
            {
                Vector3 destination = pin.targetPoint != null ? pin.targetPoint.position : pin.transform.position;
                destination.y = transform.position.y;
                targetPos = destination;
                reachedTarget = false;

                // 🎧 Extrae el AudioClip del pin
                AudioSource pinAudio = pin.GetComponent<AudioSource>();
                if (pinAudio != null && pinAudio.clip != null)
                {
                    audioPendiente = pinAudio.clip;
                }
                else
                {
                    audioPendiente = null;
                    Debug.LogWarning($"[CatController] El pin {pin.name} no tiene AudioSource con clip.");
                }

                pin.OnPinClicked();

                Debug.Log($"[CatController] Pin tocado: {clickedObject.name} => destino: {destination}");
            }
            else
            {
                Vector3 p = hit.point;
                p.y = transform.position.y;
                targetPos = p;
                reachedTarget = false;
                audioPendiente = null;

                Debug.Log($"[CatController] Movimiento libre hacia: {p} (objeto: {clickedObject.name})");
            }
        }
    }

    private void HandleMovement()
    {
        if (targetPos.HasValue)
        {
            Vector3 target = targetPos.Value;
            Vector3 dir = target - transform.position;
            dir.y = 0f;
            float dist = dir.magnitude;

            if (dist > stoppingDistance)
            {
                Vector3 move = dir.normalized * moveSpeed * Time.deltaTime;
                transform.position += move;

                Quaternion targRot = Quaternion.LookRotation(dir.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targRot, rotateSpeed * Time.deltaTime);

                animator?.SetBool("isWalking", true);
                return;
            }
            else if (!reachedTarget)
            {
                reachedTarget = true;
                animator?.SetBool("isWalking", false);
                StartCoroutine(RotateToCamera());

                if (audioPendiente != null)
                {
                    StartCoroutine(ReproducirDialogo(audioPendiente));
                }

                targetPos = null;
            }
        }
        else
        {
            animator?.SetBool("isWalking", false);
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

    private System.Collections.IEnumerator ReproducirDialogo(AudioClip clip)
    {
        if (clip == null) yield break;

        estaHablando = true;
        animator?.SetBool("isTalking", true);

        audioSource.clip = clip;
        audioSource.Play();

        Debug.Log($"[CatController] Reproduciendo diálogo: {clip.name}");

        yield return new WaitForSeconds(clip.length);

        animator?.SetBool("isTalking", false);
        estaHablando = false;

        Debug.Log("[CatController] Diálogo finalizado.");
    }

    private void OnDrawGizmos()
    {
        if (targetPos.HasValue)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(targetPos.Value, 0.08f);
            Gizmos.DrawLine(transform.position, targetPos.Value);
        }
    }
}
