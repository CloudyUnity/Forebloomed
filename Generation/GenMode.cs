using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GenMode", menuName = "Gen/GenMode", order = 1)]
public class GenMode : ScriptableObject
{
    public GenLoop[] AllGenLoops;
}
