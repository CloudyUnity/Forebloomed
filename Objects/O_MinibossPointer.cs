using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class O_MinibossPointer : MonoBehaviour
{
    Vector3 scale = Vector2.one * 0.5f;

    public static O_MinibossPointer Instance;

    void Update()
    {
        if (A_LevelManager.Instance == null || A_LevelManager.Instance.MiniBossPos == Vector3.zero)
        {
            transform.localScale = Vector2.zero;
            return;
        }

        Vector3 target = A_LevelManager.Instance.MiniBossPos;

        Vector3 dir = (Player.Instance.transform.position - target).normalized;
        Vector2 targetPos = Player.Instance.transform.position - dir;
        transform.position = Vector2.Lerp(transform.position, targetPos, Time.deltaTime * 4.5f);

        transform.up = -dir;        

        transform.localScale = Vector2.Lerp(transform.localScale, scale, Time.deltaTime * 5);

        float dis = Vector2.Distance(Player.Instance.transform.position, target);
        if (dis < 7)
            scale = Vector2.zero;
    }
}
