using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NavBarUIManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void CloseMapClick()
    {
        SceneManager.LoadScene("ARScene");
    }

    public void OnMapClick() {
        SceneManager.LoadScene("MapScene");
    }
}
