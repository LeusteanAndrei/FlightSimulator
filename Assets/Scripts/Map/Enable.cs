using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class Enable : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputAction toggleAction;

    [Header("Tween")]
    [SerializeField] private RectTransform menuTransform;
    [SerializeField] private float tweenDuration = 0.3f;
    [SerializeField] private Vector3 hiddenScale = Vector3.zero;
    [SerializeField] private Vector3 shownScale = Vector3.one;

    private float previousTimeScale = 1f;

    private Tween currentTween;
    private bool isOpen;

    private void Awake()
    {
        // Start hidden
        isOpen = false;

        if (menuTransform != null)
        {
            menuTransform.localScale = hiddenScale;
        }

        gameObject.SetActive(false);
        SetCursorState(false);
    }

    private void OnEnable()
    {
        toggleAction.Enable();
        toggleAction.performed += Toggle;
    }

    private void OnDisable()
    {
        toggleAction.performed -= Toggle;
        toggleAction.Disable();

        currentTween?.Kill();
    }

    public void Toggle(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        // Kill any currently running tween so spam input can't break it
        currentTween?.Kill();

        isOpen = !isOpen;

        if (isOpen)
        {
            OpenMenu();
        }
        else
        {
            CloseMenu();
        }
    }

    private void OpenMenu()
    {
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        gameObject.SetActive(true);

        SetCursorState(true);

        menuTransform.localScale = hiddenScale;

        currentTween = menuTransform
            .DOScale(shownScale, tweenDuration)
            .SetEase(Ease.OutBack) // overshoot then settle
            .SetUpdate(true);      // timescale independent
    }

    private void CloseMenu()
    {
        Time.timeScale = previousTimeScale;

        SetCursorState(false);

        currentTween = menuTransform
            .DOScale(hiddenScale, tweenDuration)
            .SetEase(Ease.InBack) // slight pull before shrinking
            .SetUpdate(true)      // timescale independent
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
            });
    }

    private void SetCursorState(bool visible)
    {
        Cursor.visible = visible;
        Cursor.lockState = visible
            ? CursorLockMode.None
            : CursorLockMode.Locked;
    }
}