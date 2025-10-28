using UnityEngine;
using UnityEngine.UI;

public class Mute : MonoBehaviour
{
    [Header("Botón de mute")]
    [SerializeField] private Button muteButton;
    [SerializeField] private Sprite iconMute;
    [SerializeField] private Sprite iconUnmute;

    private bool isMuted = false;

    void Start()
    {
        if (muteButton != null)
            muteButton.onClick.AddListener(ToggleMute);

        UpdateIcon();
    }

    private void ToggleMute()
    {
        isMuted = !isMuted;

        // 🔇 Control global del audio
        AudioListener.volume = isMuted ? 0f : 1f;

        UpdateIcon();
    }

    private void UpdateIcon()
    {
        if (muteButton != null && muteButton.image != null)
        {
            muteButton.image.sprite = isMuted ? iconMute : iconUnmute;
        }
    }
}
