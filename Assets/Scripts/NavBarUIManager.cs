using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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

    public void OnMapClick() {
        //  if (instructionPanel.activeSelf) // close instruction panel on map load
        // {
        //     instructionPanel.SetActive(false);
        // }
        SceneManager.LoadScene("MapScene");
    }

    public void OnBackpackClick() {
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
}
