using UnityEngine;

public class ZyloAnim: MonoBehaviour
{
    [SerializeField] private Animator animator;

    public void ActivarPensar() => animator.SetBool("isThinking", true);
    public void DesactivarPensar() => animator.SetBool("isThinking", false);

    public void ActivarHablar() => animator.SetBool("isTalking", true);
    public void DesactivarHablar() => animator.SetBool("isTalking", false);
}
