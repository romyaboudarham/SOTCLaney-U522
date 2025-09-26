using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using UnityEngine.XR.ARFoundation;

public class NavBarUIManager : MonoBehaviour
{
    [SerializeField] private ARPlaneManager aRPlaneManager;

    private QuestManager questManager;
    private bool isMapOpen = false;

    private bool isMapBlinking;
    private Image mapButtonImage;
    private Color buttonOriginalColor;

    private bool isBackpackBlinking;
    private Image backpackButtonImage;

    bool initBackpackOpen = true;


    public static NavBarUIManager Instance { get; private set; }
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
    [Tooltip("Button that opens the map.")]
    Button m_MapButton;

    /// <summary>
    /// Button that opens the map.
    /// </summary>
    public Button MapButton
    {
        get => m_MapButton;
        set => m_MapButton = value;
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
        isMapOpen = false;
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
        questManager = FindObjectOfType<QuestManager>();
        mapButtonImage = m_MapButton.GetComponent<Image>();
        backpackButtonImage = m_BackpackButton.GetComponent<Image>();
        buttonOriginalColor = mapButtonImage.color;
    }

    // Update is called once per frame
    void Update()
    {
        if (isMapBlinking)
        {
            float t = Mathf.PingPong(Time.time * 2f, 1f); // blink speed
            mapButtonImage.color = Color.Lerp(buttonOriginalColor, Color.red, t);
        }
        if (isBackpackBlinking)
        {
            float t = Mathf.PingPong(Time.time * 2f, 1f); // blink speed
            backpackButtonImage.color = Color.Lerp(buttonOriginalColor, Color.red, t);
        }
    }

    public void OnMapOpen()
    {
        //  if (instructionPanel.activeSelf) // close instruction panel on map load
        // {
        //     instructionPanel.SetActive(false);
        // }
        questManager.SpawnQuestsOnMap();
        CameraUIManager.Instance.ShowMap();
        if (isMapBlinking)
        {
            StopMapBlinking();
        }

        if (initBackpackOpen)
        {
            initBackpackOpen = false;
            placeObjectOnboarding();
        }

        questManager.HideAllQuestPanels();
    }

    private void placeObjectOnboarding()
    {
        Debug.Log("Showing onboarding panel");
        //instructionPanel.SetActive(true);
    }

    public void OnBackpackClick()
    {
        ShowBackpack();
    }

    void ShowBackpack()
    {
        questManager.HideLocationReachedPanel();
        aRPlaneManager.enabled = true;
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
        if (isBackpackBlinking)
        {
            StopBackpackBlinking();
           // questManager.FadeAwayCanvas();
        }
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

    public void MapNewQuest()
    {
        isMapBlinking = true;
    }

    public void BackpackNewItem()
    {
        isBackpackBlinking = true;
    }

    private void StopMapBlinking()
    {
        isMapBlinking = false;
        mapButtonImage.color = buttonOriginalColor;
    }

    private void StopBackpackBlinking()
    {
        isBackpackBlinking = false;
        backpackButtonImage.color = buttonOriginalColor;
    }

    public bool IsMapOpen()
    {
        return isMapOpen;
    }
}
