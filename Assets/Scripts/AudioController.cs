using UnityEngine;

public class AudioController : MonoBehaviour
{
    public static AudioController Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    // Cargar y devolver el clip según su nombre
    public AudioClip GetAudioForPin(string idPin)
    {
        AudioClip clip = Resources.Load<AudioClip>($"/Dialogos/{idPin}");

        if (clip == null)
            Debug.LogWarning($"[AudioManager] No se encontró el audio para {idPin}");

        return clip;
    }
}
