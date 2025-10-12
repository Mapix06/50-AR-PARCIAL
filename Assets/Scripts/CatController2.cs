using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
public class CatController2 : MonoBehaviour
{
    [Header("Movimiento del gato")]
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float rotateSpeed = 10f;
    [SerializeField] private float stoppingDistance = 0.2f;
    [SerializeField] private float raycastMaxDistance = 100f;

    private Animator animator;
    private Camera cam;
    private Vector3? targetPos = null;

    void Awake()
    {
        animator = GetComponent<Animator>();
        cam = Camera.main;

        if (animator != null)
        {
            animator.Rebind();
            animator.SetBool("isWalking", false);
        }
    }

    void Update()
    {
        HandleClickInput();
        HandleMovement();
    }

    private void HandleClickInput()
    {
        // Clic (Editor/PC)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 screenPos = Mouse.current.position.ReadValue();
            TrySetTargetFromScreenPoint(screenPos);
        }

        // Toque (móvil)
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
                // Si el objeto tiene PinMapa, mover hacia él y activar contenido
                Vector3 destination = pin.targetPoint != null ? pin.targetPoint.position : pin.transform.position;
                destination.y = transform.position.y; // Mantener altura del gato
                targetPos = destination;

                pin.OnPinClicked();

                Debug.Log($"[CatControllerSimple] Pin tocado: {clickedObject.name} => destino: {destination}");
            }
            else
            {
                // Movimiento libre si no es un pin
                Vector3 p = hit.point;
                p.y = transform.position.y;
                targetPos = p;

                Debug.Log($"[CatControllerSimple] Movimiento libre hacia: {p} (objeto: {clickedObject.name})");
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
                // Movimiento
                Vector3 move = dir.normalized * moveSpeed * Time.deltaTime;
                transform.position += move;

                // Rotación suave hacia el objetivo
                Quaternion targRot = Quaternion.LookRotation(dir.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targRot, rotateSpeed * Time.deltaTime);

                animator?.SetBool("isWalking", true);
                return;
            }
            else
            {
                // Llegó al destino
                targetPos = null;
            }
        }

        // Quieto
        animator?.SetBool("isWalking", false);
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