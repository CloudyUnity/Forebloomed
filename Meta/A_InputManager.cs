using Steamworks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class A_InputManager : MonoBehaviour
{
    public static A_InputManager Instance;

    KeyBind[] _keyBinds;
    [SerializeField] KeyBind[] _defaults;
    [SerializeField] KeyBind[] _gamepadDefaults;

    public static bool GamepadMode { get; private set; }
    static bool WasGamepadMode;
    public static bool ChangedModeThisFrame => GamepadMode != WasGamepadMode;
    public static bool GamepadIsPlaystation => Gamepad.current != null && Gamepad.current.layout.Contains("Dual");
    public static bool GamepadIsNintendo => Gamepad.current != null && Gamepad.current.layout.Contains("Switch");

    string[] _lastJoystickNames = new string[0];

    private KeyCode[] gamepadKeyCodes = new KeyCode[]
    {
        KeyCode.JoystickButton0,  // A button on Xbox controller, Cross button on PlayStation controller
        KeyCode.JoystickButton1,  // B button on Xbox controller, Circle button on PlayStation controller
        KeyCode.JoystickButton2,  // X button on Xbox controller, Square button on PlayStation controller
        KeyCode.JoystickButton3,  // Y button on Xbox controller, Triangle button on PlayStation controller
        KeyCode.JoystickButton4,  // Left bumper (LB) button on Xbox controller
        KeyCode.JoystickButton5,  // Right bumper (RB) button on Xbox controller
        KeyCode.JoystickButton6,  // Back button on Xbox controller, Share button on PlayStation controller
        KeyCode.JoystickButton7,  // Start button on Xbox controller, Options button on PlayStation controller
        KeyCode.JoystickButton8,  // Left stick button (LS) on Xbox controller
        KeyCode.JoystickButton9,  // Right stick button (RS) on Xbox controller
        KeyCode.JoystickButton10, // Xbox button on Xbox controller, PS button on PlayStation controller
        KeyCode.JoystickButton11, // D-pad up button on Xbox controller
        KeyCode.JoystickButton12, // D-pad down button on Xbox controller
        KeyCode.JoystickButton13, // D-pad left button on Xbox controller
        KeyCode.JoystickButton14, // D-pad right button on Xbox controller
        KeyCode.JoystickButton15, // Custom button, can vary by controller
        KeyCode.JoystickButton16, // Custom button, can vary by controller
        // Add more as needed
    };

    public static readonly bool DEBUG_INPUTS = true;

    private void Awake()
    {
        Instance = this;
        _keyBinds = new KeyBind[_defaults.Length];
        GetKeys();
    }

    void GetKeys()
    {
        for (int i = 0; i < _defaults.Length; i++)
        {
            KeyBind def = _defaults[i];

            if (!PlayerPrefs.HasKey(def.Name))
                PlayerPrefs.SetString(def.Name, def.Code.ToString());

            _keyBinds[i].Code = GetKey(def.Name);
        }
    }

    KeyCode GetKey(string name) => (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString(name));

    public void AssignKey(int n) => StartCoroutine(C_AssignKey(n));

    IEnumerator C_AssignKey(int n)
    {
        _defaults[n].Waiting = true;

        KeyCode newKey = KeyCode.None;
        while (newKey == KeyCode.None)
        {
            foreach (KeyCode vKey in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKey(vKey))
                {
                    newKey = vKey;
                    break;
                }
            }
            yield return null;
        }

        _defaults[n].Waiting = false;
        _keyBinds[n].Code = newKey;
        PlayerPrefs.SetString(_defaults[n].Name, newKey.ToString());
    }

    public void ResetKeys()
    {
        foreach (var bind in _defaults)
            PlayerPrefs.SetString(bind.Name, bind.Code.ToString());

        GetKeys();
    }

    public KeyCode Key(string name)
    {
        if (GamepadMode && (A_LevelManager.Instance == null || A_LevelManager.Instance.SceneTime > 0.05f || A_LevelManager.Instance.CurrentLevel == -1))
        {
            for (int i = 0; i < _gamepadDefaults.Length; i++)
            {
                if (_gamepadDefaults[i].Name == name)
                {
                    //Debug.Log("Assigned " + name + " to " + _gamepadDefaults[i].Code);
                    return _gamepadDefaults[i].Code;
                }                    
            }
            return KeyCode.None;
        }

        for (int i = 0; i < _keyBinds.Length; i++)
        {
            if (_defaults[i].Name == name)
                return _keyBinds[i].Code;
        }
        return KeyCode.None;
    }

    bool IsGamepadKey(KeyCode keyCode)
    {
        return System.Array.Exists(gamepadKeyCodes, element => element == keyCode);
    }

    bool deSelected = true;
    private void Update()
    {
        string[] currentJoysticks = Input.GetJoystickNames();

        bool disconnected = currentJoysticks.Length == 0 || currentJoysticks.Length != _lastJoystickNames.Length;
        if (!disconnected)
        {
            for (int i = 0; i < _lastJoystickNames.Length; i++)
            {
                if (_lastJoystickNames[i] != currentJoysticks[i])
                {
                    disconnected = true;
                    break;
                }                    
            }
        }
        _lastJoystickNames = currentJoysticks;

        if (disconnected && A_LevelManager.Instance.SceneTime > 0.2f)
            GamepadMode = false;
        else if (Input.anyKeyDown)
        {
            foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(key))
                {
                    if (DEBUG_INPUTS)
                        Debug.Log("Key Down: " + key.ToString());

                    if (IsGamepadKey(key))
                    {
                        GamepadMode = true;
                        if (Gamepad.current != null)
                            Debug.Log("Controller connected: " + Gamepad.current.layout);
                        else
                            Debug.Log("Controller detected but not connected");
                        break;
                    }
                    GamepadMode = false;
                }
            }
        }

        if (!GamepadMode && EventSystem.current.currentSelectedGameObject != null && !deSelected)
        {
            EventSystem.current.SetSelectedGameObject(null);
            deSelected = true;
        }
        else if (GamepadMode)
            deSelected = false;

        if (Input.GetKeyDown(KeyCode.Joystick1Button10))
        {
            SteamFriends.ActivateGameOverlay("Achievements");
            GamepadMode = false;
        }

        _shootNow = GamepadShoot();

        if (V_HUDManager.Instance != null && !V_HUDManager.Instance.IsPaused)
            return;

        for (int i = 0; i < _defaults.Length; i++)
        {
            KeyBind bind = _defaults[i];
            _defaults[i].Text.text = bind.Name + " : " + (bind.Waiting ? "" : _keyBinds[i].Code.ToString());

            bool covered = V_UIManager.Instance.IsHovered(bind.Text.gameObject);
            bind.Text.black.color = covered ? Color.yellow : Color.black;
            MoveOption(bind.Text.parentGameObject, covered);
            bind.Text.parentGameObject.transform.localScale = Vector2.one * (covered ? 1.1f : 1);
        }

        // Emergency Reset:
        if (Input.GetKey(KeyCode.P) && Input.GetKey(KeyCode.Keypad2) && Input.GetKeyDown(KeyCode.Space))
        {
            ResetKeys();
        }
    }

    private void LateUpdate()
    {
        WasGamepadMode = GamepadMode;

        _shootLast = _shootNow;
    }

    public void MoveOption(GameObject go, bool covered)
    {
        float x = covered ? 50 : 0;
        Vector2 newPos = go.transform.localPosition;
        newPos.x = Mathf.Lerp(newPos.x, x, Time.unscaledDeltaTime * 5);
        go.transform.localPosition = newPos;
    }

    public bool GamepadRT()
    {
        if (!GamepadMode)
            return false;

        return Input.GetAxis("Fire1") > 0.5f;
    }

    public bool GamepadLT()
    {
        if (!GamepadMode)
            return false;

        return Input.GetAxis("Fire1") < -0.5f;
    }

    public bool GamepadShoot() => GamepadRT() || Input.GetKey(KeyCode.Joystick1Button5) || Input.GetKey(KeyCode.Joystick1Button2);

    bool _shootLast;
    bool _shootNow;
    public bool GamepadShootDown() => _shootNow && !_shootLast;
    public bool GamepadAbility() => GamepadLT() || Input.GetKey(KeyCode.Joystick1Button3) || Input.GetKey(KeyCode.Joystick1Button4) || Input.GetKey(KeyCode.Joystick1Button8);
}

[System.Serializable]
public struct KeyBind
{
    public string Name;
    public KeyCode Code;
    public TextPair Text;
    public bool Waiting;
}
