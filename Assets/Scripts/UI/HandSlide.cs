using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public sealed class HandSlide : MonoBehaviour
{
    [Tooltip("Pixels from bottom edge that trigger the hand to appear.")]
    [SerializeField] float triggerZone = 60f;

    [Tooltip("Slide speed in pixels/second.")]
    [SerializeField] float slideSpeed = 800f;
    [SerializeField] float offset = 150f;
    
    RectTransform _rect;
    CanvasGroup   _cg;

    float _hiddenY;
    float _visibleY;

    void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _cg   = GetComponent<CanvasGroup>();

        _visibleY = 0f;
        _hiddenY  = -_rect.rect.height - offset;
        _rect.anchoredPosition = new Vector2(0, _hiddenY);

        SetInteractable(false);
    }

    void Update()
    {
        Vector2 mouse = Input.mousePosition;
        bool mouseNearBottom = mouse.y <= triggerZone;
        bool mouseOverPanel  =
            RectTransformUtility.RectangleContainsScreenPoint(_rect, mouse);

        bool wantVisible = mouseNearBottom || mouseOverPanel;

        float targetY = wantVisible ? _visibleY : _hiddenY;
        float newY = Mathf.MoveTowards(_rect.anchoredPosition.y,
            targetY,
            slideSpeed * Time.unscaledDeltaTime);
        if (Mathf.Abs(newY - _rect.anchoredPosition.y) > 0.1f)
            _rect.anchoredPosition = new Vector2(0, newY);

        SetInteractable(wantVisible);
    }

    void SetInteractable(bool state)
    {
        _cg.blocksRaycasts = state;
        _cg.interactable   = state;
        _cg.alpha          = state ? 1f : 0.5f;
    }
}