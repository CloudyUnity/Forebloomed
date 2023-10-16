using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class V_UIManager : MonoBehaviour
{
    public static V_UIManager Instance;

    [SerializeField] List<TextPair> _allButtons = new List<TextPair>();
    [SerializeField] List<GameObject> _allButtonsSizeOnly = new List<GameObject>();
    [SerializeField] GraphicRaycaster _graphicRay;
    [SerializeField] Texture2D _cursorTex;
    [SerializeField] Vector2 _hotSpot;
    List<GameObject> _lastSelected = new List<GameObject>();

    void Awake()
    {
        Instance = this;
        Cursor.SetCursor(_cursorTex, _hotSpot, CursorMode.Auto);
    }

    public void AddButton(GameObject go) => _allButtonsSizeOnly.Add(go);

    public List<GameObject> GetHovered()
    {
        if (A_InputManager.GamepadMode)
            return new List<GameObject>() { EventSystem.current.currentSelectedGameObject };

        var eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;

        var raysastResults = new List<RaycastResult>();
        _graphicRay.Raycast(eventData, raysastResults);

        var results = new List<GameObject>();
        foreach (var result in raysastResults)
            results.Add(result.gameObject);

        return results;
    }

    public bool IsHovered(GameObject go)
    {
        if (EventSystem.current.currentSelectedGameObject == go.transform.parent.gameObject)
            return true;

        foreach (var result in GetHovered())
        {
            if (result == go)
                return true;
        }
        return false;
    }

    private void Update()
    {
        bool paused = V_HUDManager.Instance != null && V_HUDManager.Instance.IsPaused;
        bool dead = Player.Instance != null && Player.Instance.Dead;
        bool mainMenu = Player.Instance == null && V_HUDManager.Instance == null;
        if (paused || dead || mainMenu)
            ModifyButtons();
    }

    void ModifyButtons()
    {
        List<GameObject> hovering = GetHovered();

        List<GameObject> newSelected = new List<GameObject>();

        if (hovering.Count == 0)
            _lastSelected = new List<GameObject>();

        foreach (TextPair txt in _allButtons)
        {
            if (hovering.Contains(txt.parentGameObject))
            {
                txt.parent.localScale = Vector2.one * Mathf.Lerp(txt.parent.localScale.x, 1.1f, Time.unscaledDeltaTime * 12);
                txt.black.color = Color.white;

                newSelected.Add(txt.parentGameObject);
                if (!_lastSelected.Contains(txt.parentGameObject))
                    A_EventManager.InvokePlaySFX("HoverMenu");
                continue;
            }

            txt.parent.localScale = Vector2.one * Mathf.Lerp(txt.parent.localScale.x, 1, Time.unscaledDeltaTime * 12);
            txt.black.color = Color.black;
        }

        if (_allButtonsSizeOnly.Count == 0)
        {
            _lastSelected = newSelected;
            return;
        }            

        _allButtonsSizeOnly.RemoveAll(i => i == null);

        foreach (GameObject button2 in _allButtonsSizeOnly)
        {
            if (hovering.Contains(button2))
            {
                button2.transform.localScale = Vector2.one * Mathf.Lerp(button2.transform.localScale.x, 1.1f, Time.unscaledDeltaTime * 8);

                newSelected.Add(button2);
                if (!_lastSelected.Contains(button2))
                    A_EventManager.InvokePlaySFX("HoverMenu");
                continue;
            }

            button2.transform.localScale = Vector2.one * Mathf.Lerp(button2.transform.localScale.x, 1, Time.unscaledDeltaTime * 8);
        }
        _lastSelected = newSelected;
    }
}
