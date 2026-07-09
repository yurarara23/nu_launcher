using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CommonUI : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputAction pauseAction;

    [Header("Pause UI")]
    [SerializeField] private CanvasGroup menuCanvasGroup;
    [SerializeField] private GameObject firstSelectedButton;

    [Header("Scene")]
    [SerializeField] private string homeSceneName = "Start";

    private bool isMenuOpen;

    private void Awake()
    {
        if (menuCanvasGroup == null)
        {
            Debug.LogError(
                "Menu Canvas Groupが設定されていません。",
                this
            );

            enabled = false;
            return;
        }

        SetMenuVisible(false);
    }

    private void OnEnable()
    {
        pauseAction.performed += OnPausePerformed;
        pauseAction.Enable();
    }

    private void OnDisable()
    {
        pauseAction.performed -= OnPausePerformed;
        pauseAction.Disable();

        Time.timeScale = 1f;
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        ToggleMenu();
    }

    public void ToggleMenu()
    {
        SetMenuVisible(!isMenuOpen);
    }

    public void OpenMenu()
    {
        SetMenuVisible(true);
    }

    public void CloseMenu()
    {
        SetMenuVisible(false);
    }

    public void ReturnToHome()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(homeSceneName);
    }

    private void SetMenuVisible(bool visible)
    {
        if (menuCanvasGroup == null)
        {
            return;
        }

        isMenuOpen = visible;

        menuCanvasGroup.alpha = visible ? 1f : 0f;
        menuCanvasGroup.interactable = visible;
        menuCanvasGroup.blocksRaycasts = visible;

        Time.timeScale = visible ? 0f : 1f;

        if (EventSystem.current == null)
        {
            return;
        }

        EventSystem.current.SetSelectedGameObject(null);

        if (visible && firstSelectedButton != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }
    }
}