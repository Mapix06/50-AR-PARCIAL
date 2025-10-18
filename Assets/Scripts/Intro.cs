using UnityEngine;
using UnityEngine.SceneManagement;

public class Intro : MonoBehaviour
{
    [SerializeField] private string nombreEscenaPrincipal = "Principal";

    public void Empezar()
    {
        SceneManager.LoadScene(nombreEscenaPrincipal);
    }

    public void Salir()
    {
        Debug.Log("Saliendo de la experiencia...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}