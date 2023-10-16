using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OW_GoldenRoot : ObjectWorld
{
    [SerializeField] GameObject _pointerPrefab;
    [SerializeField] SpriteRenderer _pointerRend;
    [SerializeField] ParticleSystem _burstPS;

    private void Update()
    {
        if (Interact)
        {
            Selectable = false;
            Instantiate(_pointerPrefab, transform.position + new Vector3(0, 0.2f), Quaternion.identity);
            _pointerRend.enabled = false;
            A_EventManager.InvokePlaySFX("GoldPoint");
            Instantiate(_burstPS, transform.position + new Vector3(0, 0.3f), Quaternion.identity);
        }
    }
}
