using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class NavBarUIManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The animator for the object creation menu.")]
    Animator m_ObjectMenuAnimator;

    /// <summary>
    /// The animator for the object creation menu.
    /// </summary>
    public Animator objectMenuAnimator
    {
        get => m_ObjectMenuAnimator;
        set => m_ObjectMenuAnimator = value;
    }

    bool m_ShowObjectMenu;
    public GameObject instructionPanel;

    [SerializeField]
    [Tooltip("The menu with all the creatable objects.")]
    GameObject m_ObjectMenu;

    /// <summary>
    /// The menu with all the creatable objects.
    /// </summary>
    public GameObject objectMenu
    {
        get => m_ObjectMenu;
        set => m_ObjectMenu = value;
    }

    [SerializeField]
    [Tooltip("The object spawner component in charge of spawning new objects.")]
    ObjectSpawner m_ObjectSpawner;

    /// <summary>
    /// The object spawner component in charge of spawning new objects.
    /// </summary>
    public ObjectSpawner objectSpawner
    {
        get => m_ObjectSpawner;
        set => m_ObjectSpawner = value;
    }

    [SerializeField]
    [Tooltip("Button that opens the create menu.")]
    Button m_BackpackButton;

    /// <summary>
    /// Button that opens the create menu.
    /// </summary>
    public Button BackpackButton
    {
        get => m_BackpackButton;
        set => m_BackpackButton = value;
    }

    [SerializeField]
    [Tooltip("Button that closes the object creation menu.")]
    Button m_CancelButton;

    /// <summary>
    /// Button that closes the object creation menu.
    /// </summary>
    public Button cancelButton
    {
        get => m_CancelButton;
        set => m_CancelButton = value;
    }

    void OnEnable()
    {
        m_BackpackButton.onClick.AddListener(ShowBackpack);
        m_CancelButton.onClick.AddListener(HideBackpack);
    }

    /// <summary>
    /// See <see cref="MonoBehaviour"/>.
    /// </summary>
    void OnDisable()
    {
        m_ShowObjectMenu = false;
        m_BackpackButton.onClick.RemoveListener(ShowBackpack);
        m_CancelButton.onClick.RemoveListener(HideBackpack);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnMapOpen()
    {
        //  if (instructionPanel.activeSelf) // close instruction panel on map load
        // {
        //     instructionPanel.SetActive(false);
        // }
        CameraUIManager.Instance.ShowMap();
    }

    public void OnBackpackClick()
    {
        if (instructionPanel.activeSelf) // close instruction panel on map load
        {
            instructionPanel.SetActive(false);
        }
        ShowBackpack();
    }

    void ShowBackpack()
    {
        m_ShowObjectMenu = true;
        m_ObjectMenu.SetActive(true);
        if (!m_ObjectMenuAnimator.GetBool("Show"))
        {
            m_ObjectMenuAnimator.SetBool("Show", true);
        }
    }

    /// <summary>
    /// Triggers hide animation for menu.
    /// </summary>
    public void HideBackpack()
    {
        m_ObjectMenuAnimator.SetBool("Show", false);
        m_ObjectMenu.SetActive(false);
        m_ShowObjectMenu = false;
    }
    
    /// <summary>
    /// Set the index of the object in the list on the ObjectSpawner to a specific value.
    /// This is effectively an override of the default behavior or randomly spawning an object.
    /// </summary>
    /// <param name="objectIndex">The index in the array of the object to spawn with the ObjectSpawner</param>
    public void SetObjectToSpawn(int objectIndex)
    {
        if (m_ObjectSpawner == null)
        {
            Debug.LogWarning("Object Spawner not configured correctly: no ObjectSpawner set.");
        }
        else
        {
            if (m_ObjectSpawner.objectPrefabs.Count > objectIndex)
            {
                m_ObjectSpawner.spawnOptionIndex = objectIndex;
            }
            else
            {
                Debug.LogWarning("Object Spawner not configured correctly: object index larger than number of Object Prefabs.");
            }
        }

        //HideMenu();
    }
}
