using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class V_DialogueManager : MonoBehaviour
{
    public static V_DialogueManager Instance;

    [SerializeField] TextPair _textMain;
    [SerializeField] TextPair _textName;
    [SerializeField] GameObject _textBox;
    [SerializeField] GameObject _arrow;
    [SerializeField] SinWave _arrowWave;
    [SerializeField] Image _icon;
    [SerializeField] Vector2 _open, _closed;

    bool _sentenceTyped;
    bool _frameOfNext;
    bool _blockInput;

    Dialogue _currentDia;

    KeyCode _interact, _shoot;

    public List<string> UsedDialogue;

    [SerializeField] TMPro.TMP_FontAsset _base, _swain;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _interact = A_InputManager.Instance.Key("Interact");
        _shoot = A_InputManager.Instance.Key("Shoot");
    }

    private void Update()
    {
        if (Player.Instance == null || !Player.Instance.InDialogue)
            return;

        Vector2 targetSize = _sentenceTyped ? Vector2.one : Vector2.zero;
        _arrow.transform.localScale = Vector2.Lerp(_arrow.transform.localScale, targetSize, Time.deltaTime * 4);
        _arrow.transform.localPosition = new Vector2(_arrow.transform.localPosition.x, A_Extensions.GetSinWave(_arrowWave, Time.time));

        if (_blockInput)
            return;

        _frameOfNext = false;
        if (Player.Instance.InDialogue && _sentenceTyped && (Input.GetKeyDown(_interact) || Input.GetKeyDown(_shoot)))
        {
            _frameOfNext = true;
            Next();
        }
    }

    Dialogue GetDialogue(List<Dialogue> dialogueList)
    {
        for (int i = 0; i < 100; i++)
        {
            Dialogue dia = dialogueList.RandomItem(A_LevelManager.QuickSeed * (i + 1));

            if (UsedDialogue.Contains(dia.Identifier))
                continue;

            if (dia.CharacterSpecific != -1 && dia.CharacterSpecific != Player.Instance.CharacterIndex)
                continue;

            if (dia.WorldSpecific != -1 && dia.WorldSpecific != A_LevelManager.Instance.WorldIndex())
                continue;

            if (dia.LoopSpecific != -1 && dia.LoopSpecific != A_LevelManager.Instance.DifficultyModifier)
                continue;

            if (dia.LevelSpecific != -1 && dia.LevelSpecific != A_LevelManager.Instance.CurrentLevel)
                continue;

            UsedDialogue.Add(dia.Identifier);

            return dia;
        }

        Debug.Log("ERROR DIALOGUE RAN OUT");

        return dialogueList[0];
    }

    public int StartDialogue(List<Dialogue> diaList)
    {
        if (Player.Instance.InDialogue || _frameOfNext || _blockInput)
            return 0;

        Player.Instance.InDialogue = true;

        Dialogue newDia = GetDialogue(diaList);
        _currentDia = newDia;
        _currentDia.Text = new List<string>(newDia.Text);

        if (_currentDia.Name == "Swain")
        {
            _textMain.black.font = _swain;
            _textMain.grey.font = _swain;
        }
        else
        {
            _textMain.black.font = _base;
            _textMain.grey.font = _base;
        }

        _textMain.text = "";
        _textName.text = _currentDia.Name;
        _icon.sprite = _currentDia.Icon;

        _textBox.SetActive(true);
        StartCoroutine(C_MoveTextBox(_closed, _open, true));

        return 1;
    }

    void Next()
    {
        if (_currentDia.Text.Count == 0)
        {
            End();
            return;
        }

        string text = _currentDia.Text[0];
        _currentDia.Text.RemoveAt(0);

        StartCoroutine(C_TypeSentence(text));
    }

    const float DIALOGUE_SPEED = 0.025f;
    IEnumerator C_TypeSentence(string sentence)
    {
        _sentenceTyped = false;

        _textMain.text = sentence;

        float elapsed = 0;
        int i = 0;

        while (i < sentence.Length)
        {
            if (!_frameOfNext && Input.GetKeyDown(A_InputManager.Instance.Key("Interact")))
                break;

            float dur = DIALOGUE_SPEED;
            if (i >= 1 && _textMain.text[i - 1].Is('.', '?', '!', ','))
                dur *= 10;
            if (_currentDia.Name == "CorruptKeep")
                dur *= 2f;

            _textMain.maxChar = i;

            if (elapsed >= dur)
            {
                A_EventManager.InvokePlaySFX(_currentDia.SFX);
                i++;
                elapsed = 0;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        _textMain.maxChar = sentence.Length;
        _sentenceTyped = true;
    }

    void End()
    {
        _currentDia = default(Dialogue);
        StartCoroutine(C_MoveTextBox(_open, _closed, false));
        _frameOfNext = false;
        Player.Instance.InDialogue = false;        
    }


    IEnumerator C_MoveTextBox(Vector2 from, Vector2 to, bool nextOnFinish)
    {
        _blockInput = true;
        float elapsed = 0;
        float dur = .5f;

        while (elapsed < dur)
        {
            float curved = A_Extensions.CosCurve(elapsed / dur);
            _textBox.transform.localPosition = Vector2.Lerp(from, to, curved);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        _textBox.transform.localPosition = to;

        _blockInput = false;
        if (!nextOnFinish)
            yield break;

        _frameOfNext = true;
        Next();
    }
}

[System.Serializable]
public struct Dialogue
{
    public string Identifier;

    public string Name;
    public Sprite Icon;
    [TextArea] public List<string> Text;

    public int CharacterSpecific;
    public int WorldSpecific;
    public int LoopSpecific;
    public int LevelSpecific;

    public string SFX;
}
