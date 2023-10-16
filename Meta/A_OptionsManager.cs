using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class A_OptionsManager : MonoBehaviour
{
    public static A_OptionsManager Instance;

    [SerializeField] Settings _defaults;
    [SerializeField] TextPair _screenshake;
    [SerializeField] TextPair _hideClock;
    [SerializeField] TextPair _hideHearts;
    [SerializeField] TextPair _quickRestart;
    [SerializeField] TextPair _hintMessages;
    [SerializeField] TextPair _timer;
    [SerializeField] TextPair _camlock;
    [SerializeField] TextPair _pixelFilter;
    [SerializeField] TextPair _sfxVolume;
    [SerializeField] Slider _sfxSlider;
    [SerializeField] TextPair _musicVolume;
    [SerializeField] Slider _musicSlider;
    [SerializeField] float _moveDis;
    [SerializeField] float _moveSpeed;
    [SerializeField] Button _returnButton;

    public Settings Current;

    void Awake() => Instance = this;

    private void Start()
    {
        if (!PlayerPrefs.HasKey("PixelFilter"))
        {
            Save(_defaults);
        }

        Current = Load();
        ApplyToUI();
    }

    void Save(Settings settings)
    {
        PlayerPrefs.SetInt("Screenshake", settings.Screenshake ? 1 : 0);
        PlayerPrefs.SetInt("QuickRestart", settings.QuickRestart ? 1 : 0);
        PlayerPrefs.SetFloat("SFX", settings.SFXVolume);
        PlayerPrefs.SetFloat("Music", settings.MusicVolume);
        PlayerPrefs.SetInt("HideClock", settings.HideClock ? 1 : 0);
        PlayerPrefs.SetInt("HideHearts", settings.HideHearts ? 1 : 0);
        PlayerPrefs.SetInt("Hints", settings.HintMessages ? 1 : 0);
        PlayerPrefs.SetInt("Timer", settings.Timer ? 1 : 0);
        PlayerPrefs.SetInt("CameraLock", settings.CamLock ? 1 : 0);
        PlayerPrefs.SetInt("PixelFilter", settings.PixelFilter ? 1 : 0);

        ApplyToUI();
    }

    Settings Load()
    {
        return new Settings
        {
            Screenshake = PlayerPrefs.GetInt("Screenshake") == 1,
            QuickRestart = PlayerPrefs.GetInt("QuickRestart") == 1,
            SFXVolume = PlayerPrefs.GetFloat("SFX"),
            MusicVolume = PlayerPrefs.GetFloat("Music"),
            HideClock = PlayerPrefs.GetInt("HideClock") == 1,
            HideHearts = PlayerPrefs.GetInt("HideHearts") == 1,
            HintMessages = PlayerPrefs.GetInt("Hints") == 1,
            Timer = PlayerPrefs.GetInt("Timer") == 1,
            CamLock = PlayerPrefs.GetInt("CameraLock") == 1,
            PixelFilter = PlayerPrefs.GetInt("PixelFilter") == 1,
        };
    }

    public void ToggleScreenShake() => Current.Screenshake = !Current.Screenshake;
    public void ToggleQuickRestart() => Current.QuickRestart = !Current.QuickRestart;
    public void ToggleHideClock() => Current.HideClock = !Current.HideClock;
    public void ToggleHideHearts() => Current.HideHearts = !Current.HideHearts;
    public void ToggleHints() => Current.HintMessages = !Current.HintMessages;
    public void ToggleTimer() => Current.Timer = !Current.Timer;
    public void ToggleCamLock() => Current.CamLock = !Current.CamLock;
    public void ToggleFilter() => Current.PixelFilter = !Current.PixelFilter;

    public void MoveOption(GameObject go, bool covered)
    {
        float x = covered ? _moveDis : 0;
        Vector2 newPos = go.transform.localPosition;
        newPos.x = Mathf.Lerp(newPos.x, x, Time.unscaledDeltaTime * _moveSpeed);
        go.transform.localPosition = newPos;
        go.transform.localScale = Vector2.one * (covered ? 1.1f : 1);

    }

    private void Update()
    {
        if ((V_HUDManager.Instance != null && !V_HUDManager.Instance.IsPaused) || _screenshake.black == null)
            return;

        bool anyCovered = false;

        bool screenCovered = V_UIManager.Instance.IsHovered(_screenshake.gameObject);
        _screenshake.black.color = screenCovered ? Color.yellow : Color.black;
        MoveOption(_screenshake.parentGameObject, screenCovered);
        anyCovered = screenCovered;

        bool clockCovered = V_UIManager.Instance.IsHovered(_hideClock.gameObject) && !anyCovered;
        _hideClock.black.color = clockCovered ? Color.yellow : Color.black;
        MoveOption(_hideClock.parentGameObject, clockCovered);
        anyCovered = anyCovered || clockCovered;

        bool heartCovered = V_UIManager.Instance.IsHovered(_hideHearts.gameObject) && !anyCovered;
        _hideHearts.black.color = heartCovered ? Color.yellow : Color.black;
        MoveOption(_hideHearts.parentGameObject, heartCovered);
        anyCovered = anyCovered || heartCovered;

        bool quickCovered = V_UIManager.Instance.IsHovered(_quickRestart.gameObject) && !anyCovered;
        _quickRestart.black.color = quickCovered ? Color.yellow : Color.black;
        MoveOption(_quickRestart.parentGameObject, quickCovered);
        anyCovered = anyCovered || quickCovered;

        bool hintCovered = V_UIManager.Instance.IsHovered(_hintMessages.gameObject) && !anyCovered;
        _hintMessages.black.color = hintCovered ? Color.yellow : Color.black;
        MoveOption(_hintMessages.parentGameObject, hintCovered);
        anyCovered = anyCovered || hintCovered;

        bool timerCovered = V_UIManager.Instance.IsHovered(_timer.gameObject) && !anyCovered;
        _timer.black.color = timerCovered ? Color.yellow : Color.black;
        MoveOption(_timer.parentGameObject, timerCovered);
        anyCovered = anyCovered || timerCovered;

        bool camCovered = V_UIManager.Instance.IsHovered(_camlock.gameObject) && !anyCovered;
        _camlock.black.color = camCovered ? Color.yellow : Color.black;
        MoveOption(_camlock.parentGameObject, camCovered);
        anyCovered = anyCovered || camCovered;

        bool filter = V_UIManager.Instance.IsHovered(_pixelFilter.gameObject) && !anyCovered;
        _pixelFilter.black.color = filter ? Color.yellow : Color.black;
        MoveOption(_pixelFilter.parentGameObject, filter);
        anyCovered = anyCovered || filter;

        bool sfxCovered = V_UIManager.Instance.IsHovered(_sfxVolume.gameObject) && !anyCovered;
        _sfxVolume.black.color = sfxCovered ? Color.yellow : Color.black;
        MoveOption(_sfxVolume.parentGameObject, sfxCovered);
        anyCovered = anyCovered || sfxCovered;

        bool musicCovered = V_UIManager.Instance.IsHovered(_musicVolume.gameObject) && !anyCovered;
        _musicVolume.black.color = musicCovered ? Color.yellow : Color.black;
        MoveOption(_musicVolume.parentGameObject, musicCovered);
        anyCovered = anyCovered || musicCovered;

        _returnButton.enabled = !sfxCovered && !musicCovered;

        if (sfxCovered && A_InputManager.GamepadMode && Input.GetAxis("Horizontal") != 0)
        {
            _sfxSlider.value += Input.GetAxis("Horizontal") * 0.01f;
        }
        Current.SFXVolume = _sfxSlider.value;

        if (musicCovered && A_InputManager.GamepadMode && Input.GetAxis("Horizontal") != 0)
        {
            _musicSlider.value += Input.GetAxis("Horizontal") * 0.01f;
        }
        Current.MusicVolume = _musicSlider.value;

        Save(Current);

        ApplyToUI();
    }

    void ApplyToUI()
    {
        if (_screenshake.black == null)
            return;

        _screenshake.text = "ScreenShake : " + (Current.Screenshake ? "On" : "Off");
        _hideClock.text = "Hide Clock : " + (Current.HideClock ? "On" : "Off");
        _hideHearts.text = "Hide Hearts : " + (Current.HideHearts ? "On" : "Off");
        _quickRestart.text = "Quick Restart : " + (Current.QuickRestart ? "On" : "Off");
        _hintMessages.text = "Hint Messages : " + (Current.HintMessages ? "On" : "Off");
        _timer.text = "Timer : " + (Current.Timer ? "On" : "Off");
        _camlock.text = "Camera Lock-On : " + (Current.CamLock ? "On" : "Off");
        _pixelFilter.text = "Pixels : " + (Current.PixelFilter ? "Crunchy" : "Sharp");
        _sfxSlider.value = Current.SFXVolume;
        _musicSlider.value = Current.MusicVolume;
    }

    public void ResetToDefaults()
    {
        Save(_defaults);
        Current = Load();
        ApplyToUI();
    }
}

[System.Serializable]
public struct Settings
{
    public bool Screenshake;
    public bool QuickRestart;
    public float SFXVolume;
    public float MusicVolume;
    public bool HideClock;
    public bool HideHearts;
    public bool HintMessages;
    public bool Timer;
    public bool CamLock;
    public bool PixelFilter;
}
