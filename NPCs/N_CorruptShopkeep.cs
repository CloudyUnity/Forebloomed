using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class N_CorruptShopkeep : MonoBehaviour
{
    [SerializeField] List<Dialogue> _loreDia = new List<Dialogue>();
    [SerializeField] List<Dialogue> _afterDia = new List<Dialogue>();

    [SerializeField] SpriteRenderer _rend;
    [SerializeField] Material _baseMat;
    [SerializeField] Material _glowMat;

    int _talkCounter;

    bool _selected { get { return Player.Selected == gameObject; } }

    KeyCode _interact;

    private void Start()
    {
        _interact = A_InputManager.Instance.Key("Interact");
    }

    void Update()
    {
        _rend.material = _selected ? _glowMat : _baseMat;
        float targetSize = _selected ? 0.85f : 0.8f;
        transform.localScale = Vector2.Lerp(transform.localScale, Vector2.one * targetSize, Time.deltaTime * 5);

        if (!_selected)
            return;

        if (!Input.GetKeyDown(_interact) || Player.Instance.InDialogue || V_DialogueManager.Instance == null)
            return;

        A_EventManager.InvokeUnlock("Extrovert");

        if (_talkCounter == 0)
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
}
