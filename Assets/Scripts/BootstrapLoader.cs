using UnityEngine;
using UnityEngine.SceneManagement;

public class BootstrapLoader : MonoBehaviour
{
    private void Start()
    {
        SceneManager.LoadSceneAsync("MapScene", LoadSceneMode.Additive).completed += (op) =>
        {
            CameraUIManager.Instance.ShowAR(); // now references exist, ShowAR can safely disable MapCamera/UI
        };
        SceneManager.LoadSceneAsync("MainARScene", LoadSceneMode.Additive);
    }
}
