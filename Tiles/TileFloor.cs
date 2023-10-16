using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileFloor : Tile
{
    float _timer;

    protected override void Update()
    {
        base.Update();
        _timer += Time.deltaTime;
    }

    public override void MarkExplored()
    {
        base.MarkExplored();

        Bottom.SetActive(true);
        Top.SetActive(false);

        if (A_LevelManager.Instance.SceneTime >= 3 && _timer > 0.5f)
        {
            Instantiate(GroundBurstPS, transform.position, Quaternion.identity);
            Instantiate(LeafDropPS, transform.position, Quaternion.identity);
        }
    }
}
