using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class N_GeneralNPC : MonoBehaviour
{
    [SerializeField] List<Dialogue> _dialogue = new List<Dialogue>();

    [SerializeField] SpriteRenderer _rend;
    [SerializeField] Material _baseMat;
    [SerializeField] Material _glowMat;
    [SerializeField] string _unlock;

    bool _selected { get { return Player.Selected == gameObject; } }

    KeyCode _interact;

    void Update()
    {
        _rend.material = _selected ? _glowMat : _baseMat;
        float targetSize = _selected ? 0.85f : 0.8f;
        transform.localScale = Vector2.Lerp(transform.localScale, Vector2.one * targetSize, Time.deltaTime * 5);

        if (!_selected)
            return;

        _interact = A_InputManager.Instance.Key("Interact");

        if (!Input.GetKeyDown(_interact) || Player.Instance.InDialogue || V_DialogueManager.Instance == null)
            return;

        V_DialogueManager.Instance.StartDialogue(_dialogue);
        if (_unlock != null && _unlock != "")
            A_EventManager.InvokeUnlock(_unlock);
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
