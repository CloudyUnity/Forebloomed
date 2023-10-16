using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Tile Preset", menuName = "New Tile Preset")]
public class TilePreset : ScriptableObject
{
    public Sprite DefaultTop;
    public Sprite DefaultBottom;
    public Sprite[] TopSprites;
    public Sprite[] BottomSprites;
    public float VariantSpriteChance;

    public float MinHeight;
    public float MinDifficulty;
    public float MinAlt;

    public Tile TilePrefab;
    public float HitPoints;

    public List<TileDecor> Decorations = new List<TileDecor>();
    public List<WorldObjFreq> Props = new List<WorldObjFreq>();

    [System.Serializable]
    public struct WorldObjFreq
    {
        public MakeInfo makeInfo;
        public float Freq;
    }

    public List<WorldObjFreq> WorldObjects = new List<WorldObjFreq>();
    public float decorFreq;
    public float propFreq;

    public (Sprite, Sprite) GetSprites()
    {
        if (Random.Range(0, 100f) > VariantSpriteChance)
            return (DefaultTop, DefaultBottom);

        Sprite top = TopSprites[Random.Range(0, TopSprites.Length)];
        Sprite bttm = BottomSprites[Random.Range(0, BottomSprites.Length)];
        return (top, bttm);
    }

    public bool MatchCondition(float height, float difficulty, float danger)
    {
        return height >= MinHeight && difficulty >= MinDifficulty && danger >= MinAlt;
    }

    public TileDecor GetDecor()
    {
        if (Random.Range(0, 100) > decorFreq || Decorations.Count == 0)
            return null;

        int rand = Random.Range(0, Decorations.Count);
        
        return Decorations[rand];
    }

    public void MakeProps(Vector2 pos, GameObject tile)
    {
        if (Random.Range(0, 100) > propFreq || Props.Count == 0)
            return;

        A_Factory.Instance.MakeBasic(pos, Props.RandomItem().makeInfo, tile.transform);
    }

    static string Last_WO = null;
    public void MakeWorldObjects(Vector2 pos)
    {
        if (WorldObjects.Count == 0)
            return;

        float seed = (Mathf.Abs(pos.x) + 6) * (Mathf.Abs(pos.y) + 3) * A_LevelManager.QuickSeed;

        float total = 0;
        foreach (WorldObjFreq ow in WorldObjects)
        {
            total += ow.Freq;
        }

        for (int i = 1; i < 300; i++)
        {
            for (int j = 0; j < WorldObjects.Count; j++)
            {
                WorldObjFreq ow = WorldObjects[j];
                if (ow.makeInfo.Name == Last_WO)
                    continue;

                if (A_Extensions.RandomChance(ow.Freq / total, seed))
                {
                    if (ow.makeInfo.Prefab == null)
                        return;

                    A_Factory.Instance.MakeBasic(pos, ow.makeInfo);
                    Last_WO = ow.makeInfo.Name;
                    Debug.Log("Spawned: " + ow.makeInfo.Name);
                    return;
                }
                total -= ow.Freq;
            }
        }
        throw new System.Exception("World objects failed to spawn!");
    }
}
