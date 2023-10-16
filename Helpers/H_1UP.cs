using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class H_1UP : MonoBehaviour
{
    public static bool DestroyedOne;
    public ItemData _extraLife;
    [SerializeField] ParticleSystem _ps;

    private void OnEnable()
    {
        A_EventManager.Destroy1Ups += DestroyOne;
    }

    void OnDisable()
    {
        A_EventManager.Destroy1Ups -= DestroyOne;
    }

    public void DestroyOne()
    {
        if (DestroyedOne)
            return;

        DestroyedOne = true;

        // SFX
        Player.Instance.FollowItems.Remove(GetComponent<O_Follower>());
        ItemManager.Instance.AllItems.Remove(_extraLife);
        Instantiate(_ps, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
