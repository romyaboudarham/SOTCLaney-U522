using UnityEngine;
using UnityEngine.SceneManagement;

public class BootstrapLoader : MonoBehaviour
{
    private void Start()
    {
        SceneManager.LoadSceneAsync("MapScene", LoadSceneMode.Additive).completed += (op) =>
        {
            // Start waiting for BaseTiles/RuntimeObjectsRoot
            CameraUIManager.Instance.StartCoroutine(CameraUIManager.Instance.WaitForMapAndThenShowAR());
        };
        //SceneManager.LoadSceneAsync("MainARScene", LoadSceneMode.Additive);
    }
}
