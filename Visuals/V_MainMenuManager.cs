using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;

public class V_MainMenuManager : MonoBehaviour
{
    [SerializeField] GameObject _startMenu;
    [SerializeField] GameObject _mainMenu;

    [SerializeField] GameObject _continueButton;
    [SerializeField] GameObject _continueButtonGrey;
    [SerializeField] GameObject _quitButton;
    [SerializeField] TMP_Text _seedInputTXT;
    [SerializeField] TMP_InputField _seedInputField;
    [SerializeField] GameObject _seedDisabled;

    [SerializeField] float _moveDur;
    [SerializeField] GameObject _square1, _square2;
    [SerializeField] Vector2 _amount;
    [SerializeField] Vector2 _camSize;
    [SerializeField] Transform _squareParent;
    [SerializeField] GameObject _globalVolume;
    [SerializeField] GameObject _pressAnyKey;

    [SerializeField] TextPair _statName;
    [SerializeField] TextPair _statMain;
    [SerializeField] TextPair _statFar;
    [SerializeField] TextPair _statFast;
    [SerializeField] TextPair _winStreakValue;
    List<CharacterStats> _allStats;
    CharacterStats _totals;
    int _farChar;
    int _fastChar;

    [SerializeField] GameObject _sociableCover;

    [SerializeField] GameObject _fullscreenMenu;
    [SerializeField] TextPair _fullscreenTXT;
    [SerializeField] TextPair _percTXT1, _percTXT2, _percTXT3;

    [SerializeField] AudioSource _musicSource, _sfxSourceA, _sfxSourceB;
    int _secretCounter;

    [SerializeField] GameObject _hardModeCover;
    bool _hardModeOn = false;

    bool _changingScene;

    [System.Serializable]
    struct Openable
    {
        public string Name;
        public GameObject Cover;
    }

    [SerializeField] List<Openable> _openables = new List<Openable>();

    [SerializeField] List<Challenge> _challenges = new List<Challenge>();

    [SerializeField] VolumeProfile _profile;
    UnityEngine.Rendering.Universal.ColorAdjustments _colorPP;
    UnityEngine.Rendering.Universal.Vignette _vignettePP;
    UnityEngine.Rendering.Universal.ChromaticAberration _chromaPP;

    [SerializeField] GameObject _mainFrst, _extFrst, _charFrst;
    [SerializeField] GameObject _extUI, _achUI, _achUI2, _achUI3, _challUI, _credUI, _statUI, _hatUI, _charUI, _opUI, _bindUI;

    [SerializeField] GameObject _gamepadKeybinds;
    [SerializeField] GameObject _gamepadKeybindsXBOX;
    [SerializeField] GameObject _gamepadKeybindsPS;
    [SerializeField] GameObject _gamepadKeybindsNin;
    [SerializeField] GameObject _keyBindInteractable;
    [SerializeField] GameObject _keyBindResetBttn;

    GameObject _currentOpened;

    private void OnEnable()
    {
        A_EventManager.OnOpenMeta += OpenMeta;
        A_EventManager.OnDisplayPerc += DisplayPercentage;
    }
    private void OnDisable()
    {
        A_EventManager.OnOpenMeta -= OpenMeta;  
        A_EventManager.OnDisplayPerc -= DisplayPercentage;
    }

    private void Awake()
    {
        if (!_profile.TryGet(out _colorPP))
            throw new System.NullReferenceException(nameof(_colorPP));
        _colorPP.postExposure.Override(0);
        _colorPP.saturation.Override(0);
        if (!_profile.TryGet(out _vignettePP))
            throw new System.NullReferenceException(nameof(_vignettePP));
        _vignettePP.intensity.Override(0);
        if (!_profile.TryGet(out _chromaPP))
            throw new System.NullReferenceException(nameof(_chromaPP));
        _chromaPP.intensity.Override(0);
        _globalVolume.SetActive(true);

        MakeGrid();
        GetStats();

        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            _quitButton.SetActive(false);
        }

        if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXPlayer)
        {
            _fullscreenMenu.SetActive(true);

            _fullscreenTXT.text = "Fullscreen\n\n" + (Application.platform == RuntimePlatform.WindowsPlayer ? "Alt + Enter" : "Command + F");
        }

        _currentOpened = _startMenu;

        string filePath = Application.persistentDataPath + "/level.json";
        if (!File.Exists(filePath))
        {
            _continueButton.SetActive(false);
            _continueButtonGrey.SetActive(true);
            return;
        }

        string data = File.ReadAllText(filePath);
        LevelData levelData = JsonUtility.FromJson<LevelData>(data);
        if (levelData.Time == 0 || levelData.Version != A_SaveManager.Version)
        {
            _continueButton.SetActive(false);
            _continueButtonGrey.SetActive(true);
        }
    }

    private void Start()
    {
        if (_totals.GoldSpent >= 10000)
            A_EventManager.InvokeUnlock("Moneybags");

        if (_totals.Runs - _totals.Wins >= 24)
            A_EventManager.InvokeUnlock("Immortal");
    }

    bool _wasGamepadMode = false;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F6))
        {
            Close(_startMenu);
            return;
        }

        if (Input.anyKeyDown && _startMenu.activeSelf)
        {
            StartCoroutine(C_Open(_mainMenu));
            StartCoroutine(C_Close(_startMenu));
            SetGamepadSelected(_mainFrst);
        }

        int gamepadMode = A_InputManager.GamepadMode ? A_InputManager.GamepadIsPlaystation ? 1 : A_InputManager.GamepadIsNintendo ? 2 : 3 : 0;
        _gamepadKeybinds.SetActive(gamepadMode != 0);
        _seedDisabled.SetActive(gamepadMode != 0);
        _seedInputField.enabled = gamepadMode == 0;
        _gamepadKeybindsXBOX.SetActive(gamepadMode == 3);
        _gamepadKeybindsPS.SetActive(gamepadMode == 1);
        _gamepadKeybindsNin.SetActive(gamepadMode == 2);
        _keyBindInteractable.SetActive(gamepadMode == 0);
        _keyBindResetBttn.SetActive(gamepadMode == 0);

        bool gamepadConnect = !_wasGamepadMode;
        bool nullRef = _wasGamepadMode && EventSystem.current.currentSelectedGameObject == null;
        bool wrongMenuFailsafe = EventSystem.current.currentSelectedGameObject == _mainFrst && _currentOpened != _mainMenu;
        if (A_InputManager.GamepadMode && _currentOpened != _startMenu && (gamepadConnect || nullRef || wrongMenuFailsafe))
        {
            if (_currentOpened != _mainMenu)
            {
                OpenClose(_mainMenu, _currentOpened);
            }
            else
                SetGamepadSelected(_mainFrst);
        }
        _wasGamepadMode = A_InputManager.GamepadMode;

        if (A_InputManager.GamepadMode && Input.GetKeyDown(A_InputManager.Instance.Key("Return")))
        {
            SwitchCharacterCard(0);

            if (_mainMenu.activeSelf)
                OpenClose(_startMenu, _mainMenu);

            if (_extUI.activeSelf)
                OpenClose(_mainMenu, _extUI);

            if (_challUI.activeSelf)
                OpenClose(_mainMenu, _challUI);

            if (_achUI.activeSelf)
                OpenClose(_extUI, _achUI);

            if (_achUI2.activeSelf)
                OpenClose(_extUI, _achUI2);

            if (_achUI3.activeSelf)
                OpenClose(_extUI, _achUI3);

            if (_credUI.activeSelf)
                OpenClose(_extUI, _credUI);

            if (_statUI.activeSelf)
                OpenClose(_extUI, _statUI);

            if (_charUI.activeSelf)
                OpenClose(_mainMenu, _charUI);

            if (_hatUI.activeSelf)
                OpenClose(_charUI, _hatUI);

            if (_opUI.activeSelf)
                OpenClose(_mainMenu, _opUI);

            if (_bindUI.activeSelf)
                OpenClose(_opUI, _bindUI);
        }        

        _pressAnyKey.SetActive(Mathf.Repeat(Time.time, 1) > 0.5f);

        _hardModeCover.SetActive(_hardModeOn);

        #region SECRET_CODE
        if (Input.GetKeyDown(KeyCode.R) && _secretCounter.Is(0, 4))
        {
            _secretCounter++;
            return;
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            _secretCounter = 1;
            return;
        }
        else if (Input.anyKeyDown && _secretCounter.Is(0, 4))
            _secretCounter = 0;

        if (Input.GetKeyDown(KeyCode.E) && _secretCounter.Is(1, 3, 6))
        {
            _secretCounter++;
            return;
        }
        else if (Input.anyKeyDown && _secretCounter.Is(1, 3, 6))
            _secretCounter = 0;

        if (Input.GetKeyDown(KeyCode.V) && _secretCounter == 2)
        {
            _secretCounter++;
            return;
        }
        else if (Input.anyKeyDown && _secretCounter == 2)
            _secretCounter = 0;

        if (Input.GetKeyDown(KeyCode.S) && _secretCounter == 5)
        {
            _secretCounter++;
            return;
        }
        else if (Input.anyKeyDown && _secretCounter == 5)
            _secretCounter = 0;

        if (_secretCounter == 7)
        {
            _musicSource.pitch = -1;
            _sfxSourceA.pitch = -1;
            _sfxSourceB.pitch = -1;
            _secretCounter = -999;
            A_EventManager.InvokeUnlock("searcher");
        }
        #endregion
    }

    public void OpenClose(GameObject open, GameObject close)
    {
        if (open == _extUI)
            SetGamepadSelected(_extFrst);
        else if (open == _mainMenu)
            SetGamepadSelected(_mainFrst);
        else
            SetGamepadSelected(_charFrst);

        StartCoroutine(C_Open(open));
        StartCoroutine(C_Close(close));
    }

    public void NewGame(int index)
    {
        if (_changingScene || V_TransitionManager.Instance.Transitioning)
            return;

        _changingScene = true;
        A_EventManager.InvokeDeleteSave();
        A_EventManager.InvokeMakeSave(index, _seedInputTXT.text, _hardModeOn);
        A_EventManager.InvokeLoadWorld(1);
    }

    public void NewChallenge(int index)
    {
        if (_changingScene || V_TransitionManager.Instance.Transitioning)
            return;

        Challenge challenge = _challenges[index];
        _changingScene = true;
        A_EventManager.InvokeDeleteSave();
        A_EventManager.InvokeStartChallenge(challenge);
        A_EventManager.InvokeLoadWorld(challenge.StartLevel);
    }

    public void Continue()
    {
        if (_changingScene || V_TransitionManager.Instance.Transitioning)
            return;

        _changingScene = true;
        A_EventManager.InvokeReloadSave();
    }

    public void Open(GameObject open) => StartCoroutine(C_Open(open));
    public void Close(GameObject close) => StartCoroutine(C_Close(close));

    public void DeleteData()
    {
        if (_changingScene)
            return;

        _changingScene = true;
        A_EventManager.InvokeDeleteSave();
        A_EventManager.InvokeDeleteMeta();
        _continueButton.SetActive(false);
        _continueButtonGrey.SetActive(true);
        A_OptionsManager.Instance.ResetToDefaults();

        A_EventManager.InvokeLoadWorld(-1);
    }

    [SerializeField] List<GameObject> _characterCards = new List<GameObject>();
    int _currentCard = -1;

    public void SwitchCharacterCard(int i)
    {
        if (_currentCard == -1)
            _currentCard = 0;

        if (i == _currentCard)
            return;

        StartCoroutine(C_Open(_characterCards[i]));
        StartCoroutine(C_Close(_characterCards[_currentCard]));
        _currentCard = i;
    }

    public void Exit() => Application.Quit();

    void MakeGrid()
    {
        Vector2 size = _camSize / _amount;

        for (int x = -5; x < _amount.x + 5; x++)
            for (int y = -5; y < _amount.y + 5; y++)
            {
                GameObject square = (x + y).IsEven() ? _square1 : _square2;
                GameObject go = Instantiate(square, ConvertPos(x, y, size), Quaternion.identity, _squareParent);
                go.transform.localScale = size;
            }
    }

    Vector2 ConvertPos(float x, float y, Vector2 size) => (new Vector2(x, y) * size) + (size / 2) - (_camSize / 2);

    IEnumerator C_Open(GameObject open)
    {
        _currentOpened = open;

        float dur = 0.15f;

        yield return new WaitForSecondsRealtime(dur);

        open.SetActive(true);
        float elapsed = 0;

        A_EventManager.InvokePlaySFX("Menu Open");
        while (elapsed < dur)
        {
            float curved = A_Extensions.CosCurve(elapsed / dur);
            open.transform.localScale = Vector2.one * Mathf.LerpUnclamped(0, 1, curved);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        open.transform.localScale = Vector2.one;
    }

    IEnumerator C_Close(GameObject close)
    {
        float elapsed = 0;
        float dur = 0.15f;

        while (elapsed < dur)
        {
            float curved = A_Extensions.CosCurve(elapsed / dur);
            close.transform.localScale = Vector2.one * Mathf.Lerp(1, 0, curved);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        close.SetActive(false);
    }

    public void Click() => A_EventManager.InvokePlaySFX("Click");

    void OpenMeta(string name)
    {
        bool opened = false;

        foreach (Openable open in _openables)
        {
            if (name == open.Name)
            {
                open.Cover.SetActive(!open.Cover.activeSelf);
                opened = true;
            }
        }

        if (opened)
            return;

        throw new System.Exception("No achievement or character found to open " + name);
    }

    public void CustomSFX(string name) => A_EventManager.InvokePlaySFX(name);

    void GetStats()
    {
        string filePath = Application.persistentDataPath + "/meta.json";

        if (!File.Exists(filePath))
        {
            Debug.Log("Meta Data not found");
            return;
        }

        string data = File.ReadAllText(filePath);
        
        MetaData metaData = JsonUtility.FromJson<MetaData>(data);

        _allStats = metaData.AllStats;
        _totals = CombineStats(_allStats, out _farChar, out _fastChar);
        _winStreakValue.text = metaData.WinStreak.ToString();

        Apply(_totals, true);
    }

    public void ChangeStats(int i)
    {
        if (i == -1)
        {
            Apply(_totals, true);
            return;
        }

        foreach (var stat in _allStats)
        {
            if (stat.Character == i)
            {
                Apply(stat, false);
                return;
            }
        }

        A_EventManager.InvokePlaySFX("Error");
    }

    CharacterStats CombineStats(List<CharacterStats> all, out int furthestChar, out int fastestChar)
    {
        CharacterStats combined = new CharacterStats();
        combined.FastestTime = 9999;
        furthestChar = -1;
        fastestChar = -1;

        foreach (var stat in all)
        {
            combined += stat;

            if ((stat.FurthestLevel.x >= combined.FurthestLevel.x && stat.FurthestLevel.y > combined.FurthestLevel.y) || (stat.FurthestLevel.x > combined.FurthestLevel.x))
            {
                combined.FurthestLevel = stat.FurthestLevel;
                furthestChar = stat.Character;
            }
            if (stat.FastestTime < combined.FastestTime)
            {
                combined.FastestTime = stat.FastestTime;
                fastestChar = stat.Character;
            }
        }
        return combined;
    }

    void Apply(CharacterStats stats, bool totals)
    {
        _statName.text = totals ? "Totals" : IndexToName(stats.Character);

        _statMain.text = stats.Kills + "\n" + stats.Time.ToTime() + "\n" + stats.Runs +
            "\n" + stats.Wins + "\n" + stats.GoldEarned + "\n" + stats.GoldSpent + "\n" + stats.DamageTaken;

        if (_farChar == -1)
            _statFar.text = "--";
        else
            _statFar.text = (totals ? IndexToName(_farChar) + " " : "") + "C" + stats.FurthestLevel.x + " " + Mathf.CeilToInt(stats.FurthestLevel.y / 6) + "-" + Mathf.FloorToInt((stats.FurthestLevel.y / 2 % 3) + 1);

        if (stats.FastestTime > 3000 || _fastChar == -1)
        {
            _statFast.text = "--";
            return;
        }

        _statFast.text = (totals ? IndexToName(_fastChar) + " " : "") + stats.FastestTime.ToTime();
    }

    string IndexToName(int i)
    {
        if (i == 0) return "Thicket";
        if (i == 1) return "Camellia";
        if (i == 2) return "Springleaf";
        if (i == 3) return "Cholla";
        if (i == 4) return "Alocasia";
        if (i == 5) return "Jack";
        if (i == 6) return "Swain";
        if (i == 7) return "Solana";
        return "Error";
    }

    public void UnlockSociable()
    {
        A_EventManager.InvokeUnlock("Sociable");
        _sociableCover.SetActive(false);
    }

    void DisplayPercentage(int perc)
    {
        _percTXT1.text = perc.ToString() + "%";
        _percTXT2.text = perc.ToString() + "%";
        _percTXT3.text = perc.ToString() + "%";
    }

    public void LoadWorld(int i)
    {
        A_EventManager.InvokeLoadWorld(i);
    }

    public void ToggleHardMode() => _hardModeOn = !_hardModeOn;

    public void SetGamepadSelected(GameObject go)
    {
        if (!A_InputManager.GamepadMode)
            return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(go);
        Debug.Log("Gamepad set to: " + EventSystem.current.currentSelectedGameObject);
    }
}
