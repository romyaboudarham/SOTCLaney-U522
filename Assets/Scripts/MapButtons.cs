using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapButtons : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnMapClose() {
        //  if (instructionPanel.activeSelf) // close instruction panel on map load
        // {
        //     instructionPanel.SetActive(false);
        // }
        CameraUIManager.Instance.ShowAR();
    }
}
