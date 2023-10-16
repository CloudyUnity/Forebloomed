using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;

public class V_HUDManager : MonoBehaviour
{
    public static V_HUDManager Instance;

    [SerializeField] GameObject _statsUI;
    [SerializeField] GameObject _clockUI;
    [SerializeField] GameObject _deathUI;
    [SerializeField] GameObject _pauseUI;
    [SerializeField] GameObject _itemInvUI;
    [SerializeField] GameObject _optionsUI;
    [SerializeField] GameObject _keyBindsUI;
    [SerializeField] GameObject _warningUI;
    [SerializeField] GameObject _warningUIRetry;
    [SerializeField] GameObject _bossIntroUI;
    [SerializeField] GameObject _bossHealthUI;
    [SerializeField] GameObject _timerUI;
    [SerializeField] GameObject _percentUI;
    [SerializeField] GameObject _adsUI;
    [SerializeField] GameObject _itemDesc;
    [SerializeField] GameObject _gameInfoUI;
    [SerializeField] GameObject _hardModeUI;
    [SerializeField] GameObject _solanaAmmoUI;

    [SerializeField] TextPair _seedTxt;
    [SerializeField] GameObject _seeded;

    [SerializeField] TextPair _helpMessage;

    [SerializeField] GameObject _globalVolume;
    [SerializeField] VolumeProfile _profile;

    UnityEngine.Rendering.Universal.ColorAdjustments _colorPP;
    UnityEngine.Rendering.Universal.Vignette _vignettePP;
    UnityEngine.Rendering.Universal.ChromaticAberration _chromaPP;

    [SerializeField] Image _activeItemIMG;

    [SerializeField] TextPair _levelClock;
    [SerializeField] TextPair _timeClock;
    [SerializeField] TextPair _goldClock;
    [SerializeField] GameObject _clockPointer;

    List<GameObject> _hearts = new List<GameObject>();
    List<GameObject> _bonusHearts = new List<GameObject>();
    List<bool> _heartFilled = new List<bool>();
    [SerializeField] GameObject _heartPrefab;
    [SerializeField] GameObject _bonusHeartPrefab;
    [SerializeField] GameObject _flashHeart;

    //Death Screen
    [SerializeField] Image _greyScreen;
    [SerializeField] Image _whiteScreen;
    [SerializeField] Image _deathCharacter;
    [SerializeField] TextPair _killsDeath;
    [SerializeField] TextPair _goldDeath;
    [SerializeField] TextPair _whoDeath;
    [SerializeField] TextPair _levelDeath;
    [SerializeField] TextPair _timeDeath;
    [SerializeField] TextPair _deathMessage;
    [SerializeField] List<GameObject> _deathStats = new List<GameObject>();
    string _lastDamage;

    // Pause Screen
    [SerializeField] TextPair _killsPause;
    [SerializeField] TextPair _levelPause;
    [SerializeField] TextPair _loopPause;
    [SerializeField] GameObject _pauseMainText;
    [SerializeField] List<GameObject> _pauseStats = new List<GameObject>();
    [SerializeField] float _plingDur;
    [SerializeField] float _plingCooldown;
    [SerializeField] float _pauseOpenDur;
    [SerializeField] float _pauseCloseDur;
    [SerializeField] TextPair _statDMG, _statFR, _statAMNT, _statHMNG, _statLCK, _statPRCE, _statBSPD, _statSPD, _statSZE, _statBSZE, _statSPRD, _statRNG, _statMGNT;
    [System.Serializable]
    public struct StatName
    {
        public Image Img;
        public string Name;
    }
    [SerializeField] List<StatName> statNames = new List<StatName>();
    [SerializeField] TextPair _statName;

    [SerializeField] TextPair _descName;
    [SerializeField] TextPair _descDesc;
    [SerializeField] ImageCollection _descIcon;
    [SerializeField] GameObject _descFlasher;

    [SerializeField] ImageCollection _bossMainIcons;
    [SerializeField] TextPair _bossName;
    [SerializeField] GameObject _bossHealthBar;
    [SerializeField] GameObject _bossHealthBarWhite;
    [SerializeField] Transform _bossIntroIconBGParent;
    [SerializeField] GameObject _bossIntroIconPrefab;

    [SerializeField] GameObject _achievementPopUp;
    [SerializeField] TextPair _achMessage;

    [SerializeField] GameObject _abilityUI;
    [SerializeField] GameObject _abilityAmount;
    [SerializeField] Image _abilityIcon;
    [SerializeField] TextPair _abilityTXT;
    [SerializeField] GameObject _abilityHelp;

    [SerializeField] GameObject _keyBug;
    [SerializeField] TextPair _timerText;
    [SerializeField] TextPair _percentText;

    [SerializeField] GameObject[] _burs;

    [SerializeField] GameObject _pauseFrst, _optFrst, _bindFrst, _invFrst, _warnFrst1, _warnFrst2, _deathFrst;

    [SerializeField] GameObject _gamepadKeybinds;
    [SerializeField] GameObject _gamepadKeybindsXBOX;
    [SerializeField] GameObject _gamepadKeybindsPS;
    [SerializeField] GameObject _gamepadKeybindsNin;
    [SerializeField] GameObject _keyBindInteractable;
    [SerializeField] GameObject _keyBindResetBttn;

    EBoss _curBoss;

    KeyCode _pauseKey;
    KeyCode _abilityKey;

    public bool IsPaused;
    public bool IsBossIntroing;
    bool _bossHealthBarUp;
    bool _blockPause;

    bool _changingScene;

    int _startPostExposure;
    float _vignetteMax;

    public bool AllowTimeScaleChanges = false;
    public bool OverPortalLine = false;

    string _errorHelpMessage;
    float _errorMsgTimer;

    string _abilityText;

    float _pausedTimer;

    bool _lastGamepadMode;

    Player _player;

    [SerializeField] V_ItemInvManager _invManager;

    GameObject _currentOpened;

    bool _forcePauseOverlay;
    public void ForcePauseOverlay() => _forcePauseOverlay = true;

    void OnEnable()
    {
        A_EventManager.OnDealtDamage += ChromaBurst;
        A_EventManager.OnPlayerDied += PlayerDied;
        A_EventManager.OnCollectItem += ItemPopUp;
        A_EventManager.OnStartBossIntro += StartBossIntro;
        A_EventManager.OnBossSpawn += BossSpawned;
        A_EventManager.OnFlashWhiteScreen += FlashWhiteScreen;
        A_EventManager.OnShowUnlock += AchPopUp;
        A_EventManager.OnPlayerRevived += PlayerRevived;
    }
    void OnDisable()
    {
        A_EventManager.OnDealtDamage -= ChromaBurst;
        A_EventManager.OnPlayerDied -= PlayerDied;
        A_EventManager.OnCollectItem -= ItemPopUp;
        A_EventManager.OnStartBossIntro -= StartBossIntro;
        A_EventManager.OnBossSpawn -= BossSpawned;
        A_EventManager.OnFlashWhiteScreen -= FlashWhiteScreen;
        A_EventManager.OnShowUnlock -= AchPopUp;
        A_EventManager.OnPlayerRevived -= PlayerRevived;
    }

    void Awake()
    {
        Instance = this;
        _globalVolume.SetActive(true);

        if (!_profile.TryGet(out _colorPP))
            throw new System.NullReferenceException(nameof(_colorPP));
        _colorPP.postExposure.Override(_startPostExposure);
        _colorPP.saturation.Override(0);
        if (!_profile.TryGet(out _vignettePP))
            throw new System.NullReferenceException(nameof(_vignettePP));
        _vignettePP.intensity.Override(0);
        _vignettePP.color.Override(Color.black);
        if (!_profile.TryGet(out _chromaPP))
            throw new System.NullReferenceException(nameof(_chromaPP));
        _chromaPP.intensity.Override(0);
    }

    private void Start()
    {
        if (A_LevelManager.Instance == null)
            return;

        _startPostExposure = A_LevelManager.Instance.CurrentLevel.Is(13, 15) ? -1 : 0;
        _vignetteMax = !A_LevelManager.Instance.CurrentLevel.IsEven() && A_LevelManager.Instance.WorldIndex() == 4 ? 0.05f : 0.4f;
        _colorPP.postExposure.Override(_startPostExposure);

        _lastGamepadMode = A_InputManager.GamepadMode;        

        if (A_LevelManager.Instance.BossLevel && !A_LevelManager.Instance.CurrentLevel.IsEven())
        {
            _clockPointer.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -67));
            _timeClock.text = "!!!";
            _colorPP.postExposure.Override(-1);
            _colorPP.saturation.Override(-10);
            _vignettePP.intensity.Override(0.3f);
            return;
        }

        if (A_LevelManager.Instance.CurrentLevel.IsEven())
        {
            _clockPointer.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 67));
            _timeClock.text = "-";
            _colorPP.postExposure.Override(-0.25f);
            _colorPP.saturation.Override(-2.5f);
            _vignettePP.intensity.Override(0.1f);

#if USING_ADS
    _adsUI.SetActive(true);
#endif
            return;
        }
    }

    Vector2 _baseHelpM = new Vector2(0, -400);
    Vector2 _raisedHelpM = new Vector2(0, -250);
    static bool _djTrig;
    float _pauseInputBuffer = 999;
    float _helpMesTimer;
    float _helpMesCooldown = 0.01f;
    void Update()
    {
        if (_player == null)
            _player = Player.Instance;

        _clockUI.SetActive(!A_OptionsManager.Instance.Current.HideClock);
        _statsUI.SetActive(!A_OptionsManager.Instance.Current.HideHearts);
        _timerUI.SetActive(A_OptionsManager.Instance.Current.Timer);
        _abilityHelp.SetActive(A_OptionsManager.Instance.Current.HintMessages);

        if (V_TransitionManager.Instance.Transitioning || A_OptionsManager.Instance == null
            || A_LevelManager.Instance == null || A_InputManager.Instance == null || ItemManager.Instance == null)
        {
            //Debug.Log("MISSING REFS");
            return;
        }

        if (_levelClock.text == "")
        {
            _levelClock.text = A_LevelManager.Instance.GetCurLevelName();
            _seedTxt.text = A_LevelManager.Instance.Seed.ToString();
            _seeded.SetActive(A_LevelManager.Instance.SeededRun);
            _hardModeUI.SetActive(A_LevelManager.Instance.HardMode);
            MakeMap();

            if (Player.Instance.CharacterIndex == 7)
                _solanaAmmoUI.SetActive(true);
        }

        _pauseKey = A_InputManager.Instance.Key("Pause");
        _abilityKey = A_InputManager.Instance.Key("Ability");

        _goldClock.text = _player.SoftStats.GoldAmount.ToString();

        if (!AllowTimeScaleChanges)
            Time.timeScale = IsPaused || IsBossIntroing ? 0 : 1;

        string oldMessage = _helpMessage.text;
        HelpMessages();
        string newMessage = _helpMessage.text;
        _helpMesTimer += Time.deltaTime;
        if (oldMessage != newMessage && _helpMesTimer > _helpMesCooldown)
        {
            _helpMessage.text = ReplaceCharactersUntilEqual(oldMessage, newMessage);
            _helpMesTimer = 0;
        }
        else
            _helpMessage.text = oldMessage;

        SetUIHearts();
        _errorMsgTimer += Time.deltaTime;        

        _timerText.text = A_LevelManager.Instance.GlobalTime.ToTime();

        if (_curBoss != null && !IsBossIntroing)
            BossHealthBar();

        if (_curBoss == null && _bossHealthBarUp)
        {
            StartCoroutine(C_MoveBossBar(new Vector2(0, -450), new Vector2(0, -650)));
            _bossHealthBarUp = false;
        }

        AbilityIcon();

        if (IsPaused || Player.Instance.Dead)
            HoverStatNames();
        else if (_gameInfoUI.activeSelf)
            _gameInfoUI.SetActive(false);

        if (IsPaused)
        {
            _pausedTimer += Time.unscaledDeltaTime;
            if (_pausedTimer >= 120 && !_djTrig)
            {
                A_EventManager.InvokeUnlock("DJ");
                _djTrig = true;
            }

            int gamepadMode = A_InputManager.GamepadMode ? A_InputManager.GamepadIsPlaystation ? 1 : A_InputManager.GamepadIsNintendo ? 2 : 3 : 0;
            _gamepadKeybinds.SetActive(gamepadMode != 0);
            _gamepadKeybindsXBOX.SetActive(gamepadMode == 3);
            _gamepadKeybindsPS.SetActive(gamepadMode == 1);
            _gamepadKeybindsNin.SetActive(gamepadMode == 2);
            _keyBindInteractable.SetActive(gamepadMode == 0);
            _keyBindResetBttn.SetActive(gamepadMode == 0);
        }

        if (_player is P_Swain aussie)
        {
            _percentUI.SetActive(true);
            _percentText.text = Mathf.Round(aussie.Percent).ToString() + "%";
        }

        if (A_LevelManager.Instance.CurrentLevel == 24)
        {
            _levelClock.text = A_LevelManager.Instance.GetCurLevelName();
            _levelPause.text = _levelClock.text;
            _killsPause.text = _levelClock.text;
        }

        if (_solanaAmmoUI.activeSelf)
            SolanaAmmo();

        Vector2 target = _bossHealthBarUp || _itemDesc.activeSelf ? _raisedHelpM : _baseHelpM;
        _helpMessage.parent.transform.localPosition = Vector2.Lerp(_helpMessage.parent.transform.localPosition, target, Time.deltaTime * 6);

        bool gamepadConnect = !_lastGamepadMode;
        bool nullRef = _lastGamepadMode && EventSystem.current.currentSelectedGameObject == null;
        bool wrongMenuFailsafe = EventSystem.current.currentSelectedGameObject == _pauseFrst && _currentOpened != _pauseUI;
        if (IsPaused && A_InputManager.GamepadMode && _currentOpened != null && _currentOpened.activeSelf && (gamepadConnect || nullRef || wrongMenuFailsafe))
        {
            if (_currentOpened != null && _currentOpened != _pauseUI)
            {
                StartCoroutine(C_OpenClose(_pauseUI, _currentOpened));
                if (!_gameInfoUI.activeSelf)
                    StartCoroutine(C_MoveGameInfo(true));
                StartCoroutine(C_PlingStat(_pauseStats));
            }
            else
                SetGamepadSelected(_pauseFrst);
        }

        bool input = Input.GetKeyDown(_pauseKey) || (IsPaused && Input.GetKeyDown(A_InputManager.Instance.Key("Return")));
        bool forcePause = !IsPaused && _lastGamepadMode != A_InputManager.GamepadMode;
        if (input || forcePause || _forcePauseOverlay)
            _pauseInputBuffer = 0;
        _pauseInputBuffer += Time.unscaledDeltaTime;
        if (_pauseInputBuffer < 1.2f && A_LevelManager.Instance.SceneTime > 1f && _player.CanPause && !_blockPause)
        {
            _pauseInputBuffer = 999;

            if (_keyBindsUI.activeSelf)
            {
                CloseKeyBinds();
                return;
            }

            if (_optionsUI.activeSelf)
            {
                CloseSettings();
                return;
            }

            if (_itemInvUI.activeSelf)
            {
                CloseItemInv();
                return;
            }

            if (_warningUI.activeSelf)
            {
                CloseWarning();
                return;
            }

            if (_warningUIRetry.activeSelf)
            {
                CloseWarningRetry();
                return;
            }

            if (IsPaused)
            {
                ClosePause();
                return;
            }

            StartCoroutine(C_ShowPauseScreen());
            SetStatMenu();
            IsPaused = true;
        }
        _lastGamepadMode = A_InputManager.GamepadMode;

        if (A_LevelManager.Instance.CurrentLevel.IsEven() || A_LevelManager.Instance.BossLevel || A_TimeManager.Instance == null)
            return;

        float angle = Mathf.Lerp(67, -67, A_TimeManager.Instance.TimePercent);
        _clockPointer.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        _timeClock.text = Mathf.Ceil(A_TimeManager.Instance.TimeRemaining).ToString();

        if (A_TimeManager.Instance == null)
            return;

        PostProcessing();
    }

    void SetStatMenu()
    {
        int r = 2;
        _statDMG.text = _player.CurStats.Damage.RoundTo(r).ToString();
        _statBSZE.text = _player.CurStats.BulletSize.RoundTo(r).ToString();
        _statAMNT.text = _player.CurStats.Amount.RoundTo(r).ToString();
        _statFR.text = _player.CurStats.FireRate.RoundTo(r).ToString();
        _statHMNG.text = _player.CurStats.Homing.RoundTo(r).ToString();
        _statLCK.text = _player.CurStats.Luck.RoundTo(r).ToString();
        _statRNG.text = _player.CurStats.Range.RoundTo(r).ToString();
        _statSPD.text = _player.CurStats.Speed.RoundTo(r).ToString();
        _statSPRD.text = (-_player.CurStats.Spread.RoundTo(r)).ToString();
        _statSZE.text = _player.CurStats.Size.RoundTo(r).ToString();
        _statPRCE.text = _player.CurStats.Piercing.RoundTo(r).ToString();
        _statBSPD.text = _player.CurStats.BulletSpeed.RoundTo(r).ToString();
        _statMGNT.text = _player.CurStats.MagnetRange.RoundTo(r).ToString();

        _seedTxt.text = A_LevelManager.Instance.Seed.ToString();
    }

    void PostProcessing()
    {
        float t = A_TimeManager.Instance.TimePercent;

        bool atOneHP = Player.Instance.SoftStats.CurHealth + Player.Instance.SoftStats.BonusHealth == 1;
        if (Player.Instance.CharacterIndex == 7)
            atOneHP = Player.Instance.SoftStats.GoldAmount <= 15;

        Color vigCol = atOneHP ? Color.red : Color.black;
        if (atOneHP && t > 0.5f)
            vigCol = Color.Lerp(Color.red, Color.black, 2 * t - 1);
        vigCol = Color.Lerp(_vignettePP.color.value, vigCol, Time.deltaTime * 8);
        _vignettePP.color.Override(vigCol);

        float value3 = Mathf.Lerp(0, _vignetteMax, 2 * t - 1);
        if (atOneHP && value3 < 0.2f)
            value3 = 0.2f;
        value3 = Mathf.Lerp(_vignettePP.intensity.value, value3, Time.deltaTime * 8);
        _vignettePP.intensity.Override(value3);

        if (t < 0.5f)
            return;

        float value = Mathf.Lerp(_startPostExposure, -2, 2 * t - 1);
        _colorPP.postExposure.Override(value);
        float value2 = Mathf.Lerp(0, -25, 2 * t - 1);
        _colorPP.saturation.Override(value2);
    }

    void HoverStatNames()
    {
        foreach (var pair in statNames)
        {
            if (V_UIManager.Instance.IsHovered(pair.Img.transform.parent.gameObject))
            {
                _statName.text = pair.Name;
                return;
            }
        }
        _statName.text = "Hover for name";
    }

    List<Item> _popUpQueue = new List<Item>();
    public void ItemPopUp(Item item)
    {
        _popUpQueue.Add(item);

        if (_popping)
            return;

        StartCoroutine(C_ItemPopUp());
    }

    bool _popping;
    IEnumerator C_ItemPopUp()
    {
        _itemDesc.SetActive(true);

        _popping = true;

        float elapsed = 0;
        float dur = 0.4f;

        _descDesc.text = _popUpQueue[0].Data.GetDescription();
        _descName.text = _popUpQueue[0].Data.Name;
        _descIcon.sprite = _popUpQueue[0].Data.Icon;

        while (elapsed < dur)
        {
            float curved = A_Extensions.CosCurve(elapsed / dur);
            _itemDesc.transform.localPosition = Vector2.Lerp(new Vector2(0, -650), new Vector2(0, -400), curved);
            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(1.5f / _popUpQueue.Count);
        StartCoroutine(C_FlashItemDesc(1 / _popUpQueue.Count));
        yield return new WaitForSeconds(1.5f / _popUpQueue.Count);

        StartCoroutine(C_ItemPopDown());
    }

    IEnumerator C_FlashItemDesc(float dur)
    {
        float elapsed = 0;
        Vector2 start = new Vector2(-500, 80);
        Vector2 end = -start;

        while (elapsed < dur)
        {
            float curved = A_Extensions.CosCurve(elapsed / dur);
            _descFlasher.transform.localPosition = Vector2.Lerp(start, end, curved);
            elapsed += Time.deltaTime;
            yield return null;
        }
        _descFlasher.transform.localPosition = start;
    }

    IEnumerator C_ItemPopDown()
    {
        float elapsed = 0;
        float dur = 1;

        while (elapsed < dur)
        {
            float curved = A_Extensions.CosCurve(elapsed / dur);
            _itemDesc.transform.localPosition = Vector2.Lerp(new Vector2(0, -400), new Vector2(0, -650), curved);
            elapsed += Time.deltaTime;
            yield return null;
        }
        _itemDesc.transform.localPosition = new Vector2(0, -700);

        _popUpQueue.RemoveAt(0);

        if (_popUpQueue.Count > 0)
        {
            StartCoroutine(C_ItemPopUp());
            yield break;
        }
        _popping = false;
        _itemDesc.SetActive(false);
    }

    public void SetUIHearts()
    {
        if (Player.Instance.CharacterIndex == 7)
            return;

        for (int i = _hearts.Count; i < Player.Instance.SoftStats.MaxHealth && i < 15; i++)
        {
            GameObject go = Instantiate(_heartPrefab, _statsUI.transform);
            _hearts.Add(go);
            _heartFilled.Add(i >= Player.Instance.SoftStats.CurHealth);
        }

        for (int i = _bonusHearts.Count; i < Player.Instance.SoftStats.BonusHealth && i < 15 - Player.Instance.SoftStats.MaxHealth; i++)
        {
            GameObject go = Instantiate(_bonusHeartPrefab, _statsUI.transform);
            _bonusHearts.Add(go);
            StartCoroutine(C_AddHeart(go));
        }

        for (int i = Player.Instance.SoftStats.MaxHealth; i < _hearts.Count; i++)
        {
            Destroy(_hearts[i]);
            _hearts.RemoveAt(i);
            _heartFilled.RemoveAt(i);
        }

        for (int i = Player.Instance.SoftStats.BonusHealth; i < _bonusHearts.Count; i++)
        {
            StartCoroutine(C_FlashHeart(_bonusHearts[i].transform.localPosition));
            Destroy(_bonusHearts[i]);
            _bonusHearts.RemoveAt(i);
        }

        for (int i = 0; i < _hearts.Count; i++)
        {
            if (i < Player.Instance.SoftStats.CurHealth && !_heartFilled[i])
            {
                _hearts[i].transform.Find("Heart").gameObject.SetActive(true);
                _heartFilled[i] = true;
                StartCoroutine(C_AddHeart(_hearts[i]));
                continue;
            }

            if (i >= Player.Instance.SoftStats.CurHealth && _heartFilled[i])
            {
                _hearts[i].transform.Find("Heart").gameObject.SetActive(false);
                _heartFilled[i] = false;
                StartCoroutine(C_FlashHeart(_hearts[i].transform.localPosition));
            }
        }

        for (int i = 0; i < _hearts.Count; i++)
        {
            _hearts[i].transform.localPosition = new Vector2(-100 + 75 * i, 700);
        }
        for (int i = 0; i < _bonusHearts.Count; i++)
        {
            _bonusHearts[i].transform.localPosition = new Vector2(-100 + 75 * (i + _hearts.Count), 700);
        }
    }

    IEnumerator C_FlashHeart(Vector2 pos)
    {
        if (A_LevelManager.Instance.SceneTime < 1)
            yield break;

        float elapsed = 0;
        float dur = 0.2f;
        GameObject go = Instantiate(_flashHeart, _statsUI.transform);
        go.transform.localPosition = pos;
        Image img = go.GetComponent<Image>();

        while (elapsed < dur)
        {
            img.color = new Color(255, 255, 255, Mathf.Lerp(1, 0, elapsed / dur));
            elapsed += Time.deltaTime;
            yield return null;
        }
        Destroy(go);
    }

    IEnumerator C_AddHeart(GameObject heart)
    {
        float elapsed = 0;
        float dur = 0.3f;
        Vector2 startSize = heart.transform.localScale * 1.3f;
        Vector2 endSize = heart.transform.localScale;

        while (elapsed < dur)
        {
            if (heart == null)
                yield break;

            float curved = A_Extensions.CosCurve(elapsed / dur);
            heart.transform.localScale = Vector2.Lerp(startSize, endSize, curved);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (heart == null)
            yield break;

        heart.transform.localScale = endSize;
    }

    void ChromaBurst(int dmg, string name)
    {
        _lastDamage = name;
        StartCoroutine(C_Chroma());
    }

    IEnumerator C_Chroma()
    {
        float elapsed = 0;
        float dur = 0.3f;

        while (elapsed < dur)
        {
            float value = Mathf.Sin(Mathf.PI * elapsed / dur);
            _chromaPP.intensity.Override(value);
            elapsed += Time.deltaTime;
            yield return null;
        }
        _chromaPP.intensity.Override(0);
    }

    List<string> _allDeathMessages = new List<string>()
    {
        "May you find peace",
        "Nature's cycle perpetuates",
        "Rest in the celestial sea",
        "The garden falls",
        "A fitting end",
        "A noble attempt",
        "Enter the abyss",
        "Lost to the abyss",
        "A fruitless sacrifice",
        "Fertilize, Alkalize, Fossilize",
        "Someone tends the garden",
        "Find your roots",
        "Fate takes its toll",
        "Food for the garden",
        "Another falls",
        "A valiant effort",
        "Next time",
        "Fade into darkness",
        "If dreams can't come true",
        "Don't stop dancing",
        "What a terrible night",
        "Wake up",
    };

    void PlayerDied()
    {
        if (_changingScene)
            return;

        if (_lastDamage == null)
            _lastDamage = "Higher Powers";

        _deathMessage.text = _allDeathMessages.RandomItem() + "...";

        _killsDeath.text = A_LevelManager.Instance.Kills.ToString();
        string loopName = A_LevelManager.Instance.DifficultyModifier > 1 ? "C" + A_LevelManager.Instance.DifficultyModifier : "";
        _levelDeath.text = loopName + " " + A_LevelManager.Instance.GetCurLevelName();
        _goldDeath.text = Player.Instance.SoftStats.GoldAmount.ToString();
        _whoDeath.text = _lastDamage;
        _timeDeath.text = A_LevelManager.Instance.GlobalTime.ToTime();
        _clockUI.SetActive(false);
        _statsUI.SetActive(false);
        _deathCharacter.sprite = Player.Instance.DeathImg;

        A_EventManager.InvokeLowerMusic(2.4f, false);
        A_EventManager.InvokeDeleteSave();

        if (A_LevelManager.Instance.Kills >= 100)
            A_EventManager.InvokeUnlock("Pesticide");

        if (A_LevelManager.Instance.Kills >= 500)
            A_EventManager.InvokeUnlock("Ultra Pesticide");

        if (A_LevelManager.Instance.Kills >= 5000)
            A_EventManager.InvokeUnlock("Pestocide");

        StartCoroutine(C_ShowDeathScreen());
    }

    IEnumerator C_ShowDeathScreen()
    {
        _blockPause = true;
        float elapsed = 0;
        float dur = 2f;
        _greyScreen.gameObject.SetActive(true);

        while (elapsed < dur)
        {
            _greyScreen.SetAlpha(Mathf.Lerp(0, 0.8f, elapsed / dur));

            elapsed += Time.deltaTime;
            yield return null;
        }

        _deathUI.SetActive(true);
        elapsed = 0;
        A_EventManager.InvokePlaySFX("Menu Open");
        while (elapsed < 0.3f)
        {
            _deathUI.transform.localScale = Vector2.one * Mathf.Lerp(0, 1, elapsed / _pauseOpenDur);

            elapsed += Time.deltaTime;
            yield return null;
        }
        _deathUI.transform.localScale = Vector3.one;

        if (_infos == null)
        {
            _infos = new List<Vector2>();
            for (int i = 0; i < _gameInfoUI.transform.childCount; i++)
            {
                _infos.Add(_gameInfoUI.transform.GetChild(i).transform.localPosition);
            }
        }
        for (int i = 0; i < _gameInfoUI.transform.childCount; i++)
        {
            _gameInfoUI.transform.GetChild(i).localPosition = new Vector2(0, _infos[i].y);
        }
        SetStatMenu();

        SetGamepadSelected(_deathFrst);

        StartCoroutine(C_MoveGameInfo(true));
        StartCoroutine(C_PlingStat(_deathStats));
        //_blockPause = false;
    }

    public void PlayerRevived() => StartCoroutine(C_Revived());

    IEnumerator C_Revived()
    {
        A_EventManager.InvokeLowerMusic(2, true);

        _blockPause = true;
        float elapsed = 0;
        float dur = 2f;
        _greyScreen.gameObject.SetActive(true);

        while (elapsed < dur)
        {
            _greyScreen.SetAlpha(Mathf.Lerp(0, 0.8f, elapsed / dur));

            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        elapsed = 0;
        dur = 0.2f;
        while (elapsed < dur)
        {
            _greyScreen.SetAlpha(Mathf.Lerp(0.8f, 0, elapsed / dur));

            elapsed += Time.deltaTime;
            yield return null;
        }
        _greyScreen.gameObject.SetActive(false);
        _blockPause = false;
    }

    IEnumerator C_ShowPauseScreen()
    {
        if (_changingScene || Player.Instance.Stopped)
        {
            _blockPause = false;
            IsPaused = false;
            yield break;
        }

        _blockPause = true;
        float elapsed = 0;
        Vector2 endPos = _pauseMainText.transform.localPosition;
        Vector2 startPos = endPos - new Vector2(0, 50);

        _killsPause.text = A_LevelManager.Instance.Kills.ToString();
        _levelPause.text = A_LevelManager.Instance.GetCurLevelName();
        _loopPause.text = "C" + A_LevelManager.Instance.DifficultyModifier;
        _pauseMainText.transform.localPosition = startPos;
        _pauseUI.SetActive(true);

        A_EventManager.InvokePlaySFX("Menu Open");

        while (elapsed < _pauseOpenDur)
        {
            _greyScreen.SetAlpha(Mathf.Lerp(0, 0.8f, elapsed / _pauseOpenDur));
            _pauseUI.transform.localScale = Vector2.one * Mathf.Lerp(0, 1, elapsed / _pauseOpenDur);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        _pauseUI.transform.localScale = Vector2.one;

        if (_infos == null)
        {
            _infos = new List<Vector2>();
            for (int i = 0; i < _gameInfoUI.transform.childCount; i++)
            {
                _infos.Add(_gameInfoUI.transform.GetChild(i).transform.localPosition);
            }
        }
        for (int i = 0; i < _gameInfoUI.transform.childCount; i++)
        {
            _gameInfoUI.transform.GetChild(i).localPosition = new Vector2(0, _infos[i].y);
        }

        _blockPause = false;

        SetGamepadSelected(_pauseFrst);

        elapsed = 0;
        while (elapsed < _plingDur)
        {
            float curved = A_Extensions.CosCurve(elapsed / _plingDur);
            _pauseMainText.transform.localPosition = Vector2.Lerp(startPos, endPos, curved);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        _pauseMainText.transform.localPosition = endPos;

        StartCoroutine(C_MoveGameInfo(true));

        StartCoroutine(C_PlingStat(_pauseStats));
    }

    IEnumerator C_HidePauseScreen()
    {
        _blockPause = true;
        float elapsed = 0;

        if (_gameInfoUI.activeSelf)
            StartCoroutine(C_MoveGameInfo(false));

        while (elapsed < _pauseCloseDur)
        {
            _greyScreen.SetAlpha(Mathf.Lerp(0.8f, 0, elapsed / _pauseCloseDur));
            _pauseUI.transform.localScale = Vector2.one * Mathf.Lerp(1, 0, elapsed / _pauseCloseDur);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        _greyScreen.SetAlpha(0);
        _pauseUI.SetActive(false);

        foreach (var stat in _pauseStats)
        {
            stat.SetActive(false);
            stat.transform.Find("Num").gameObject.SetActive(false);
        }
        _blockPause = false;
    }

    IEnumerator C_PlingStat(List<GameObject> stats)
    {
        foreach (var stat in stats)
        {
            if (!Player.Instance.Stopped)
            {
                break;
            }

            if (stat.name == "Loop" && A_LevelManager.Instance.DifficultyModifier == 1)
                continue;

            stat.SetActive(true);
            Transform num = stat.transform.Find("Num");
            num.gameObject.SetActive(true);

            Vector2 startPos = new Vector2(0, -50);
            A_EventManager.InvokePlaySFX("Pling");

            float elapsed = 0;
            while (elapsed < _plingDur && Player.Instance.Stopped)
            {
                float curved = A_Extensions.CosCurve(elapsed / _plingDur);
                num.transform.localPosition = Vector2.Lerp(startPos, Vector2.zero, curved);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            A_EventManager.InvokePlaySFX("Pling");
            num.transform.localPosition = Vector2.zero;
            yield return new WaitForSecondsRealtime(_plingCooldown);
        }
    }

    List<Vector2> _infos;
    IEnumerator C_MoveGameInfo(bool show)
    {
        float elapsed = 0;
        float dur = 0.15f;
        float seed = Random.Range(0, 100f);

        _gameInfoUI.SetActive(true);

        while (elapsed < dur)
        {
            if (!Player.Instance.Stopped)
            {
                _gameInfoUI.SetActive(false);
                yield break;
            }

            float t = elapsed / dur;
            float c = A_Extensions.CosCurve(t);
            for (int i = 0; i < _gameInfoUI.transform.childCount; i++)
            {
                float x = A_Extensions.Rand(seed + i) ? t : c;

                if (show)
                    _gameInfoUI.transform.GetChild(i).localPosition = Vector2.Lerp(new Vector2(0, _infos[i].y), _infos[i], x);
                else
                    _gameInfoUI.transform.GetChild(i).localPosition = Vector2.Lerp(_infos[i], new Vector2(0, _infos[i].y), x);
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        for (int i = 0; i < _gameInfoUI.transform.childCount; i++)
        {
            _gameInfoUI.transform.GetChild(i).localPosition = _infos[i];
        }
        if (!show)
            _gameInfoUI.SetActive(false);
    }

    IEnumerator C_OpenClose(GameObject open, GameObject close)
    {
        if (_changingScene || (_blockPause && !Player.Instance.Dead))
            yield break;

        _currentOpened = null;

        yield return new WaitForSecondsRealtime(0.15f);

        _blockPause = true;
        float elapsed = 0;
        float dur = 0.1f;

        while (elapsed < dur)
        {
            close.transform.localScale = Vector2.one * Mathf.Lerp(1, 0, elapsed / dur);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        close.SetActive(false);

        open.SetActive(true);
        elapsed = 0;

        if (open == _optionsUI)
            SetGamepadSelected(_optFrst);
        if (open == _pauseUI)
            SetGamepadSelected(_pauseFrst);
        if (open == _warningUI)
            SetGamepadSelected(_warnFrst1);
        if (open == _warningUIRetry)
            SetGamepadSelected(_warnFrst2);
        if (open == _keyBindsUI)
            SetGamepadSelected(_bindFrst);
        if (open == _itemInvUI)
            SetGamepadSelected(_invFrst);

        A_EventManager.InvokePlaySFX("Menu Open");
        while (elapsed < dur)
        {
            open.transform.localScale = Vector2.one * Mathf.Lerp(0, 1, elapsed / dur);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        open.transform.localScale = Vector2.one;
        _blockPause = false;
        _currentOpened = open;
    }

    public void NewGame()
    {
        if (_changingScene || V_TransitionManager.Instance.Transitioning)
            return;

        _changingScene = true;

        IsPaused = false;
        Time.timeScale = 1;
        if (A_LevelManager.Instance.SeededRun)
            A_EventManager.InvokeMakeSave(Player.Instance.CharacterIndex, A_LevelManager.Instance.Seed.ToString(), A_LevelManager.Instance.HardMode);
        else
            A_EventManager.InvokeMakeSave(Player.Instance.CharacterIndex, "", A_LevelManager.Instance.HardMode);
        A_EventManager.InvokeLoadWorld(1);
    }

    public void ClosePause()
    {
        if (Player.Instance.Dead || _blockPause)
            return;

        _pauseKey = A_InputManager.Instance.Key("Pause");
        _abilityKey = A_InputManager.Instance.Key("Ability");
        _abilityText = _abilityKey.ToString();
        if (_abilityText == "Mouse0") _abilityText = "LC";
        if (_abilityText == "Mouse1") _abilityText = "RC";
        _abilityTXT.text = _abilityText;

        StartCoroutine(C_HidePauseScreen());
        IsPaused = false;
    }
    public void OpenSettings()
    {
        StartCoroutine(C_MoveGameInfo(false));
        StartCoroutine(C_OpenClose(_optionsUI, _pauseUI));
    }

    public void CloseSettings()
    {
        StartCoroutine(C_MoveGameInfo(true));
        StartCoroutine(C_OpenClose(_pauseUI, _optionsUI));
    }

    public void OpenKeyBinds() => StartCoroutine(C_OpenClose(_keyBindsUI, _optionsUI));
    public void CloseKeyBinds() => StartCoroutine(C_OpenClose(_optionsUI, _keyBindsUI));
    public void OpenWarning() => StartCoroutine(C_OpenClose(_warningUI, _pauseUI));
    public void CloseWarning() => StartCoroutine(C_OpenClose(_pauseUI, _warningUI));
    public void OpenWarningRetry() => StartCoroutine(C_OpenClose(_warningUIRetry, _pauseUI));
    public void CloseWarningRetry() => StartCoroutine(C_OpenClose(_pauseUI, _warningUIRetry));
    public void OpenItemInv()
    {
        StartCoroutine(C_MoveGameInfo(false));
        _invManager.LoadItems();

        if (_warningUIRetry.activeSelf)
            StartCoroutine(C_OpenClose(_itemInvUI, _warningUIRetry));
        else if (_warningUI.activeSelf)
            StartCoroutine(C_OpenClose(_itemInvUI, _warningUI));
        else if (_deathUI.activeSelf)
            StartCoroutine(C_OpenClose(_itemInvUI, _deathUI));
        else
            StartCoroutine(C_OpenClose(_itemInvUI, _pauseUI));
    }
    public void CloseItemInv()
    {
        StartCoroutine(C_MoveGameInfo(true));
        if (Player.Instance.Dead)
        {
            StartCoroutine(C_OpenClose(_deathUI, _itemInvUI));
            SetGamepadSelected(_deathFrst);
        }
        else
        {
            StartCoroutine(C_OpenClose(_pauseUI, _itemInvUI));
            SetGamepadSelected(_pauseFrst);
        }
            
    }

    public void QuitToMainMenu()
    {
        if (_changingScene)
            return;

        _changingScene = true;
        IsPaused = false;
        Time.timeScale = 1;
        A_EventManager.InvokeGoToMainMenu();
    }

    public void Click() => A_EventManager.InvokePlaySFX("Click");

    void StartBossIntro(Sprite main, Sprite sub, Color color, string name)
    {
        _bossIntroUI.SetActive(true);
        _bossName.text = name;
        _bossName.color = color;
        _bossMainIcons.sprite = main;
        IsBossIntroing = true;
        MakeBGIcons(sub);
        StartCoroutine(C_BossIntro());
    }

    void MakeBGIcons(Sprite icon)
    {
        for (int x = -950; x < 950; x += 200)
            for (int y = -650; y < 650; y += 200)
            {
                GameObject go = Instantiate(_bossIntroIconPrefab, _bossIntroIconBGParent);
                go.transform.localPosition = new Vector2(x, y);
                go.GetComponent<Image>().sprite = icon;
            }
    }

    IEnumerator C_BossIntro()
    {
        _blockPause = true;

        float elapsed = 0;
        float dur = 2;
        _greyScreen.gameObject.SetActive(true);

        while (elapsed < dur && !Input.GetKeyDown(A_InputManager.Instance.Key("Interact")))
        {
            _greyScreen.SetAlpha(Mathf.Lerp(0, 0.8f, elapsed / dur));
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        float timer = 0;
        while (!Input.GetKeyDown(A_InputManager.Instance.Key("Interact")) && timer < 0.1f)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        A_EventManager.InvokePlaySFX("Menu Open");
        elapsed = 0;
        dur = 5;
        while (elapsed < dur && !Input.GetKeyDown(A_InputManager.Instance.Key("Interact")))
        {
            float curved = A_Extensions.FlatCurve(elapsed / dur);
            _bossIntroUI.transform.localPosition = Vector2.Lerp(new Vector2(2100, 0), new Vector2(-2100, 0), curved);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        _bossIntroUI.transform.localPosition = new Vector2(2100, 0);
        IsBossIntroing = false;

        elapsed = 0;
        dur = 0.1f;
        while (elapsed < dur && !Input.GetKeyDown(A_InputManager.Instance.Key("Interact")))
        {
            _greyScreen.SetAlpha(Mathf.Lerp(0.8f, 0, elapsed / dur));
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        IsBossIntroing = false;
        _greyScreen.SetAlpha(0);
        _blockPause = false;
        _bossIntroUI.SetActive(false);
        _greyScreen.gameObject.SetActive(false);
    }

    void BossSpawned(EBoss boss) => _curBoss = boss;

    float _whiteTimer = 0;
    float _lastPercent = 0;
    void BossHealthBar()
    {
        if (A_BossManager.HideUIDebug)
            return;

        _bossHealthUI.SetActive(true);
        _bossHealthBar.transform.localPosition = Vector2.Lerp(new Vector2(-1325, 0), Vector2.zero, _curBoss.HealthPercent);

        if (_lastPercent != _curBoss.HealthPercent)
            _whiteTimer = 0;

        _whiteTimer += Time.deltaTime;
        if (_whiteTimer > 0.3f)
            _bossHealthBarWhite.transform.localPosition = Vector3.Lerp(_bossHealthBarWhite.transform.localPosition, _bossHealthBar.transform.localPosition, Time.deltaTime * 8);

        _lastPercent = _curBoss.HealthPercent;

        if (!_bossHealthBarUp)
        {
            StartCoroutine(C_MoveBossBar(new Vector2(0, -650), new Vector2(0, -450)));
            _bossHealthBarUp = true;
        }
    }

    IEnumerator C_MoveBossBar(Vector2 from, Vector2 to)
    {
        float elapsed = 0;
        float dur = 2f;

        while (elapsed < dur)
        {
            float curved = A_Extensions.CosCurve(elapsed / dur);
            _bossHealthUI.transform.localPosition = Vector2.Lerp(from, to, curved);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    void FlashWhiteScreen(float dur) => StartCoroutine(C_FlashWhiteScreen(dur));

    IEnumerator C_FlashWhiteScreen(float dur)
    {
        float elapsed = 0;
        _whiteScreen.gameObject.SetActive(true);

        while (elapsed < dur / 2)
        {
            float curved = A_Extensions.CosCurve(elapsed / dur / 2);
            _whiteScreen.SetAlpha(Mathf.Lerp(0, 1, curved));
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0;
        while (elapsed < dur / 2)
        {
            float curved = A_Extensions.CosCurve(elapsed / dur / 2);
            _whiteScreen.SetAlpha(Mathf.Lerp(1, 0, curved));
            elapsed += Time.deltaTime;
            yield return null;
        }
        _whiteScreen.SetAlpha(0);
        _whiteScreen.gameObject.SetActive(false);
    }

    O_Chest _cachedChest = null;
    O_Recycler _cachedRecycler = null;
    string _lastHelpMessage;
    float _timeSinceLastMessage;
    float _timeToDropMessage = 2;
    void HelpMessages()
    {        
        if (!A_OptionsManager.Instance.Current.HintMessages)
        {
            _helpMessage.text = "";
            return;
        }

        _timeSinceLastMessage += Time.deltaTime;

        string interact = KeyCodeString("Interact");
        string ability = KeyCodeString("Ability");
        _abilityTXT.text = ability;

        if (OverPortalLine)
        {
            _helpMessage.text = "Press " + interact + " to jump?";
            _helpMessage.parent.localScale = Vector2.Lerp(_helpMessage.parent.localScale, Vector2.one, Time.deltaTime * 3);
            _timeSinceLastMessage = 0;
            return;
        }

        if (_errorMsgTimer < 2 && _errorHelpMessage != null)
        {
            _helpMessage.text = _errorHelpMessage;
            _helpMessage.parent.localScale = Vector2.Lerp(_helpMessage.parent.localScale, Vector2.one * 1.15f, Time.deltaTime * 5);
            _timeSinceLastMessage = 0;
            return;
        }

        if (Player.Selected != null)
        {
            _helpMessage.parent.localScale = Vector2.Lerp(_helpMessage.parent.localScale, Vector2.one, Time.deltaTime * 5);
            _timeSinceLastMessage = 0;

            if (Player.Selected.name.StartsWith("Priced"))
            {
                _helpMessage.text = "Press " + interact + " to buy!";
                return;
            }

            if (Player.Selected.tag == "Item")
            {
                if (Player.Instance.CharacterIndex == 1 && !A_LevelManager.Instance.CurrentLevel.IsEven())
                {
                    _helpMessage.text = "Press " + interact + " to take item! \n" +
                        " Press " + ability + " to duplicate!";
                    return;
                }

                _helpMessage.text = "Press " + interact + " to take item!";
                return;
            }

            if (Player.Selected.name.StartsWith("Stone"))
            {
                _helpMessage.text = "Press " + interact + " to insert emblems!";
                return;
            }

            if (Player.Selected.tag == "NPC")
            {
                _helpMessage.text = "Press " + interact + " to talk!";
                return;
            }

            if (Player.Selected.tag == "BloodPool")
            {
                _helpMessage.text = "Press " + interact + " to sacrifice 1HP";
                return;
            }

            if (Player.Selected.name == "Exit" || Player.Selected.name == "Light Exit")
            {
                _helpMessage.text = "Press " + interact + " to leave!";
                return;
            }

            if (Player.Selected.name.Contains("Tele"))
            {
                _helpMessage.text = "Press " + interact + " to travel!";
                return;
            }

            if (Player.Selected.name.StartsWith("Chest"))
            {
                if (Player.Selected != _cachedChest)
                    _cachedChest = null;
                O_Chest chest = _cachedChest ?? Player.Selected.GetComponent<O_Chest>();
                _cachedChest = chest;
                _helpMessage.text = "Press " + interact + " to open! (" + chest.Cost + "g)";
                return;
            }

            if (Player.Selected.name.StartsWith("Bin"))
            {
                if (_cachedRecycler == null || _cachedRecycler.gameObject != Player.Selected)
                    _cachedRecycler = Player.Selected.GetComponent<O_Recycler>();
                _helpMessage.text = "Press " + interact + " to recycle " + _cachedRecycler.Item.Name + "!";
                return;
            }

            _helpMessage.text = "Press " + interact + " to Interact!";
            return;
        }

        if (A_TimeManager.Instance == null)
        {
            if (A_LevelManager.Instance.CurrentLevel == 2)
            {
                _helpMessage.text = "Buy items! Use the beam to leave";
                _helpMessage.parent.localScale = Vector2.Lerp(_helpMessage.parent.localScale, Vector2.one, Time.deltaTime * 5);
                _timeSinceLastMessage = 0;
                return;
            }

            if (_timeSinceLastMessage > _timeToDropMessage)
                _helpMessage.parent.localScale = Vector2.Lerp(_helpMessage.parent.localScale, Vector2.zero, Time.deltaTime * 5);
            return;
        }

        if (A_TimeManager.Instance.TimePercent >= 0.9f && A_LevelManager.Instance.CurrentLevel == 1)
        {
            _helpMessage.parent.localScale = Vector2.Lerp(_helpMessage.parent.localScale, Vector2.one, Time.deltaTime * 5);
            _helpMessage.text = "Get back to the start! Avoid sharks!";
            _timeSinceLastMessage = 0;
            return;
        }

        if (A_TimeManager.Instance.TimePercent <= 0.1f && A_LevelManager.Instance.CurrentLevel == 1)
        {
            _helpMessage.parent.localScale = Vector2.Lerp(_helpMessage.parent.localScale, Vector2.one, Time.deltaTime * 5);
            _timeSinceLastMessage = 0;

            if (Player.Instance.CharacterIndex == 5)
            {
                _helpMessage.text = " Press " + A_InputManager.Instance.Key("Ability").ToString() + " to launch rocket!";
                return;
            }

            _helpMessage.text = "Explore, Shoot Hedges, Open chests, Survive!";
            return;
        }

        if (_timeSinceLastMessage > _timeToDropMessage)
            _helpMessage.parent.localScale = Vector2.Lerp(_helpMessage.parent.localScale, Vector2.zero, Time.deltaTime * 5);
        return;
    }

    string ReplaceCharactersUntilEqual(string str1, string str2)
    {
        if (str1.Length > str2.Length)
            str1 = str1.Substring(0, str1.Length - 1);

        if (str1.Length < str2.Length)
            str1 += str2[str1.Length];

        for (int i = 0; i < str1.Length && i < str2.Length; i++)
        {
            if (str1[i] != str2[i])
            {
                str1 = str1.Remove(i, 1).Insert(i, str2[i].ToString());
                break;
            }
        }

        return str1;
    }

    public string KeyCodeString(string keyName)
    {
        bool ps = A_InputManager.GamepadIsPlaystation;
        bool nin = A_InputManager.GamepadIsNintendo;

        string keycode = A_InputManager.Instance.Key(keyName).ToString();
        if (keycode == "JoystickButton0")
            return ps ? "S" : nin ? "B" : "A";
        if (keycode == "JoystickButton1")
            return ps ? "X" : nin ? "A" : "B";
        if (keycode == "JoystickButton4")
            return "LB";
        if (keycode == "JoystickButton5")
            return "RB";
        if (keycode == "Mouse0")
            return "LC";
        if (keycode == "Mouse1")
            return "RC";
        if (keycode == "Mouse2")
            return "MC";

        return keycode;
    }

    public void AssignErrorHelpMessage(string msg)
    {
        _errorHelpMessage = msg;
        _errorMsgTimer = 0;
    }

    List<string> _achQueue = new List<string>();
    bool _achOngoing;
    public void AchPopUp(string name)
    {
        _achQueue.Add(name);

        if (_achOngoing)
            return;

        StartCoroutine(C_AchPopUp());
    }

    IEnumerator C_AchPopUp()
    {
        _achOngoing = true;
        if (_achQueue[0].Is("Blossom", "Leaf", "Twigs", "Shrooms", "Wings", "Wing", "Drone", "Wildfrost", "Stem", "Psycho", "Cult", "Angry", "Suspicious", "King",
            "Horns", "Musical", "Landlord", "DJ", "Eyepatch", "Blind", "Mutant", "Peas", "Bad Idea", "Expression", "Bullet"))
            _achMessage.text = "Cosmetic Unlocked: " + _achQueue[0];
        else
            _achMessage.text = "Achievement Unlocked: " + _achQueue[0];
        _achievementPopUp.SetActive(true);

        float elapsed = 0;
        float dur = 1f;

        while (elapsed < dur)
        {
            float curved = A_Extensions.CosCurve(elapsed / dur);
            _achievementPopUp.transform.localPosition = Vector2.Lerp(new Vector2(0, -900), new Vector2(0, -450), curved);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(5f / _achQueue.Count);

        elapsed = 0;
        while (elapsed < dur)
        {
            float curved = A_Extensions.CosCurve(elapsed / dur);
            _achievementPopUp.transform.localPosition = Vector2.Lerp(new Vector2(0, -450), new Vector2(0, -900), curved);
            elapsed += Time.deltaTime;
            yield return null;
        }
        _achievementPopUp.transform.localPosition = new Vector2(0, -600);

        _achQueue.RemoveAt(0);

        if (_achQueue.Count > 0)
        {
            StartCoroutine(C_AchPopUp());
            yield break;
        }
        _achOngoing = false;
        _achievementPopUp.SetActive(false);
    }

    float _lastAbPercent;
    [SerializeField] Image _abilityRing;
    [SerializeField] Color _abilityYellow;
    [SerializeField] Color _abilityGreen;
    static bool _abilityDisabledDebug;
    void AbilityIcon()
    {
        if (_abilityDisabledDebug)
        {
            _abilityUI.SetActive(false);
            return;
        }

        if (Player.Instance.AbilityIcon != null && !_abilityUI.activeSelf)
        {
            _abilityIcon.sprite = Player.Instance.AbilityIcon;
            _abilityUI.SetActive(true);
        }

        Vector2 abilityTarget = new Vector2(0, Mathf.Lerp(-100, 0, Player.Instance.AbilityPercent));
        Vector2 pos = Vector2.Lerp(_abilityAmount.transform.localPosition, abilityTarget, Time.deltaTime * 15);
        _abilityAmount.transform.localPosition = pos;

        _abilityRing.color = Player.Instance.AbilityPercent >= 1 ? _abilityGreen : _abilityYellow;

        if (Player.Instance.AbilityPercent >= 1 && _lastAbPercent < 1)
        {
            if (A_LevelManager.Instance.SceneTime >= 3)
                A_EventManager.InvokePlaySFX("Ability");
        }
        _lastAbPercent = Player.Instance.AbilityPercent;
    }

    public void FlipAbilityIcon(bool disabled) => _abilityDisabledDebug = disabled;

    public void SolanaAmmo()
    {
        for (int i = 0; i < 5; i++)
        {
            _burs[i].SetActive(i < PlayerKnives.Instance.AmmoLeft);
        }
    }

    public void ClickedAd()
    {
        CrazyGames.CrazyAds.Instance.beginAdBreakRewarded(RewardAd);
    }

    [SerializeField] MakeInfo _goldInfo;
    void RewardAd()
    {
        int cost = Random.Range(15, 25) * A_LevelManager.Instance.DifficultyModifier + A_LevelManager.Instance.CurrentLevel * 6;
        cost += (A_LevelManager.Instance.DifficultyModifier - 1) * 220;
        _goldInfo.Amount = new Vector2(cost, cost);
        A_Factory.Instance.MakeBasic(Vector3.zero, _goldInfo);
        A_EventManager.InvokePlaySFX("Key");
        _adsUI.SetActive(false);
    }

    [SerializeField] RectTransform _line;
    [SerializeField] RectTransform _line2;
    [SerializeField] Image[] _levelIconImages;
    [SerializeField] Color _onColor;
    [SerializeField] Color _beenColor;

    void MakeMap()
    {
        int index = A_LevelManager.Instance.CurrentLevel;

        index = Mathf.CeilToInt(index / 2f);
        if (index > 0)
            index--;

        float bottom = 345 - (50 * index) - (50 * Mathf.FloorToInt(index / 3));
        _line.offsetMin = new Vector2(_line.offsetMin.x, bottom);

        float top = 295 - (50 * index) - (50 * Mathf.FloorToInt(index / 3));
        _line2.offsetMax = new Vector2(_line2.offsetMax.x, top);

        for (int i = index; i >= 0; i--)
        {
            _levelIconImages[i].color = _beenColor;
        }
        _levelIconImages[index].color = _onColor;
    }

    public void SetGamepadSelected(GameObject go)
    {
        if (!A_InputManager.GamepadMode)
            return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(go);
        Debug.Log("Gamepad set to: " + EventSystem.current.currentSelectedGameObject);
    }
}

[System.Serializable]
public struct TextPair
{
    public TMP_Text black;
    public TMP_Text grey;

    public string text
    {
        get
        {
            return black.text;
        }
        set
        {
            black.text = value;
            grey.text = value;
        }
    }
    public Color color
    {
        set
        {
            black.color = new Color(value.r, value.g, value.b, black.color.a);
            grey.color = new Color(value.r, value.g, value.b, grey.color.a);
        }
    }

    public float fontSize
    {
        set
        {
            black.fontSize = value;
            grey.fontSize = value;
        }
    }

    public int maxChar
    {
        set
        {
            black.maxVisibleCharacters = value;
            grey.maxVisibleCharacters = value;
        }
    }

    public Transform transform => black.transform;
    public GameObject gameObject => black.gameObject;
    public GameObject parentGameObject => black.transform.parent.gameObject;
    public Transform parent => black.transform.parent;
}

[System.Serializable]
public struct ImageCollection
{
    public Image[] array;

    public Sprite sprite
    {
        set
        {
            foreach (var item in array)
                item.sprite = value;
        }
    }
}
