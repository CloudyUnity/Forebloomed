using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class N_Shopkeep : MonoBehaviour
{
    [SerializeField] List<Dialogue> _greetingDia = new List<Dialogue>();
    [SerializeField] List<Dialogue> _tipDia = new List<Dialogue>();
    [SerializeField] List<Dialogue> _loreDia = new List<Dialogue>();
    [SerializeField] List<Dialogue> _afterDia = new List<Dialogue>();

    [SerializeField] SpriteRenderer _rend;
    [SerializeField] Material _baseMat;
    [SerializeField] Material _glowMat;
    [SerializeField] GameObject _exclamation;

    int _talkCounter;

    bool _selected { get { return Player.Selected == gameObject; } }

    KeyCode _interact;

    private void OnEnable()
    {
        //WriteAllDialogue();
    }

    private void Start()
    {
        _interact = A_InputManager.Instance.Key("Interact");
    }

    void Update()
    {
        _rend.material = _selected ? _glowMat : _baseMat;
        float targetSize = _selected ? 0.85f : 0.8f;
        transform.localScale = Vector2.Lerp(transform.localScale, Vector2.one * targetSize, Time.deltaTime * 5);

        if (_talkCounter > 1)
        {
            _exclamation.transform.localScale = Vector2.Lerp(_exclamation.transform.localScale, Vector2.zero, Time.deltaTime * 10);
        }

        if (!_selected)
            return;

        if (!Input.GetKeyDown(_interact) || Player.Instance.InDialogue || V_DialogueManager.Instance == null)
            return;

        A_EventManager.InvokeUnlock("Extrovert");

        if (_talkCounter == 0)
        {
            _talkCounter += V_DialogueManager.Instance.StartDialogue(_greetingDia);
            A_LevelManager.Instance.ShopkeeperTalks++;
            return;
        }
        if (_talkCounter == 1)
        {
            _talkCounter += V_DialogueManager.Instance.StartDialogue(_tipDia);
            return;
        }
        if (_talkCounter == 2)
        {
            _talkCounter += V_DialogueManager.Instance.StartDialogue(_loreDia);
            return;
        }

        _talkCounter += V_DialogueManager.Instance.StartDialogue(_afterDia);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (Player.Selected != null || collision.gameObject.tag != "Player")
            return;

        Player.Selected = gameObject;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (_selected && collision.gameObject.tag == "Player")
            Player.Selected = null;
    }

    void WriteAllDialogue()
    {
        string str = "====================== Direct Dialogue: =================================\n";
        foreach (var dia in _greetingDia)
        {
            if (dia.CharacterSpecific != -1)
                str += $"Char: {IndexToName(dia.CharacterSpecific)}\n";

            if (dia.WorldSpecific != -1)
                str += $"World: {dia.WorldSpecific}\n";

            if (dia.LoopSpecific != -1)
                str += $"Loop: {dia.LoopSpecific}\n";

            str += "\nText:\n";

            foreach (var text in dia.Text)
            {
                str += text + "\n";
            }
            str += "\n==============================================\n\n";
        }

        str += "====================== After Dialogue: =================================\n\n";

        foreach (var dia in _afterDia)
        {
            str += $"Name: {dia.Name} , Char: {dia.CharacterSpecific} , World: {dia.WorldSpecific}, Loop: {dia.LoopSpecific}\nText:\n";
            foreach (var text in dia.Text)
            {
                str += text + "\n";
            }
            str += "\n==============================================\n\n";
        }

        System.IO.File.WriteAllText(Application.persistentDataPath + @"\dialogue.txt", str);
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
        return "Error";
    }
}
