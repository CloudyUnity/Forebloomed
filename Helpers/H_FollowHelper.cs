using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class H_FollowHelper : MonoBehaviour
{
    [SerializeField] O_Follower _follow;

    private void Update()
    {
        if (Player.Instance == null)
            return;

        if (!Player.Instance.FollowItems.Contains(_follow))
            Player.Instance.FollowItems.Add(_follow);

        int index = Player.Instance.FollowItems.IndexOf(_follow);
        if (index == 0)
        {
            _follow.Target = Player.Instance.transform;
            return;
        }

        _follow.Target = Player.Instance.FollowItems[index - 1].transform;
    }
}
