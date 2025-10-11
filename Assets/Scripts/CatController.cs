using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class CatController : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private string idleStateName = "Idle";

    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 0.6f;
    [SerializeField] private float rotateSpeed = 10f;
    [SerializeField] private float moveThreshold = 0.2f;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip danceMusic;
    [SerializeField] private AudioClip sleepMusic;
    [SerializeField] private AudioClip celebrateMusic;

    private Animator animator;
    private Camera cam;
    private Joystick joystick;
    private AudioSource audioSource;

   
  

    // ==========================
    // Botones UI
    // ==========================

    public void Dance()
    {
        animator.SetTrigger("dance");
        PlayMusic(danceMusic, true);
    }

    public void Sleep()
    {
        animator.SetTrigger("sleep");
        PlayMusic(sleepMusic, false);
    }

    public void Celebrate()
    {
        animator.SetTrigger("celebrate");
        PlayMusic(celebrateMusic, false);
    }

    // ==========================
    // Música
    // ==========================

    private void PlayMusic(AudioClip clip, bool loop)
    {
        if (clip == null) return;

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.loop = loop;
        audioSource.Play();

        Debug.Log("[CatController] 🎵 Música START: " + clip.name);
    }

    private void StopAllMusic()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
            Debug.Log("[CatController] 🎵 Música STOP (movimiento o acción)");
        }
    }

}