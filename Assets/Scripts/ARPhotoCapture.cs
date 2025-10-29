// ARPhotoManager.cs
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using UnityEngine.Android;
using System.Collections.Generic;

public class ARPhotoManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject capturePanel;           // Panel con el marco y la UI principal
    public GameObject previewPanel;           // Panel para previsualizar la foto
    public RawImage previewImage;             // Imagen en la previsualización
    public Button captureButton;              // Botón para tomar foto
    public Button downloadButton;             // Botón de descarga (en previsualización)
    public Button retryButton;                // Botón para volver del preview al marco
    public List<RawImage> photoSlots;         // Recuadros inferiores donde se muestran fotos guardadas

    [Header("Animaciones y efectos")]
    public AudioSource shutterSound;          // Sonido al tomar la foto (opcional)

    private Texture2D lastCapturedPhoto;
    private int nextSlotIndex = 0;

    void Start()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
                Permission.RequestUserPermission(Permission.ExternalStorageWrite);
        }

        captureButton.onClick.AddListener(() => StartCoroutine(CapturePhoto()));
        retryButton.onClick.AddListener(() => {
            previewPanel.SetActive(false);
            capturePanel.SetActive(true);
        });
        downloadButton.onClick.AddListener(DownloadPhotoFromPreview);

        foreach (var slot in photoSlots)
        {
            RawImage currentSlot = slot;
            Button btn = slot.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => OpenPhotoPreview(currentSlot.texture));
            }
        }

        previewPanel.SetActive(false);
        capturePanel.SetActive(true);
    }

    IEnumerator CapturePhoto()
    {
        yield return new WaitForEndOfFrame();

        Texture2D photo = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        photo.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        photo.Apply();

        lastCapturedPhoto = photo;
        shutterSound?.Play();

        // Añadir directamente a los slots
        if (nextSlotIndex < photoSlots.Count)
        {
            photoSlots[nextSlotIndex].texture = lastCapturedPhoto;
            nextSlotIndex++;
        }


        IEnumerator CapturePhoto()
        {
            Debug.Log("📸 Se hizo clic en el botón de tomar foto");

            yield return new WaitForEndOfFrame();

            Texture2D photo = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            photo.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            photo.Apply();

            lastCapturedPhoto = photo;
            shutterSound?.Play();

            if (nextSlotIndex < photoSlots.Count)
            {
                photoSlots[nextSlotIndex].texture = lastCapturedPhoto;
                nextSlotIndex++;
            }
        }

    }

    void OpenPhotoPreview(Texture texture)
    {
        if (texture == null) return;

        previewImage.texture = texture;
        previewPanel.SetActive(true);
        capturePanel.SetActive(false);
        lastCapturedPhoto = texture as Texture2D;
    }

    void DownloadPhotoFromPreview()
    {
        if (lastCapturedPhoto == null) return;

        string fileName = "ARPhotoPreview_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
        string path = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllBytes(path, lastCapturedPhoto.EncodeToPNG());

#if UNITY_ANDROID
        string galleryDir = "/storage/emulated/0/DCIM/ARPhotos/";
        Directory.CreateDirectory(galleryDir);
        File.Copy(path, Path.Combine(galleryDir, fileName), true);
#endif

        Debug.Log("Foto descargada desde previsualización: " + path);
    }
}
