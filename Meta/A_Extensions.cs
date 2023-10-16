using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class A_Extensions
{
    public static void DestroyChildren(this Transform t)
    {
        foreach(Transform child in t)
        {
            Object.Destroy(child.gameObject);
        }
    }

    public static Vector2 BezierCube(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        float r = 1f - t;
        float f0 = r * r * r;
        float f1 = r * r * t * 3;
        float f2 = r * t * t * 3;
        float f3 = t * t * t;
        return new Vector2(
        f0 * p0.x + f1 * p1.x + f2 * p2.x + f3 * p3.x,
        f0 * p0.y + f1 * p1.y + f2 * p2.y + f3 * p3.y
        );
    }

    public static Vector2 ReverseMidpoint(Vector2 point, Vector2 mid)
    {
        Vector2 ans;
        ans.x = 2 * mid.x - point.x;  
        ans.y = 2 * mid.y - point.y;  
        return ans;
    }

    public static float Pow(this float a, float pow) => Mathf.Pow(a, pow);

    public static float CosCurve(float t) => 0.5f - 0.5f * Mathf.Cos(Mathf.PI * t);
    public static float DecayCurve(float x, float p) => Mathf.Pow(1 - Mathf.Pow(x, p), 1 / p);
    public static float SlowingCurve(float x) => 1.3f - Mathf.Pow(0.16f / Mathf.Pow(x + 0.3f, 2), 0.5f);
    public static float HumpCurve(float t, float peak, float start) => (4 * start - 4 * peak) * Mathf.Pow(t - 0.5f, 2) + peak;
    public static float WackyBubbleGraph(float t) => Mathf.Sin(1 - Mathf.Tan(Mathf.Acos(t)));
    public static float BouncingGraph(float t) => Mathf.Abs(Mathf.Sin(Mathf.Pow(t, t)) / Mathf.Pow(2, (Mathf.Pow(t, t) - Mathf.PI / 2) / 2));

    public static float OverShootCurve(float t)
    {
        return 1 - (float)(Mathf.Sin(t * Mathf.PI * 2) / (Mathf.Exp(1) * t * 2.3));
    }
    public static float FlatCurve(float x)
    {
        float a = 0.15f;
        float c = 21f;

        System.Func<float, float> f = k => 0.5f / (1 + Mathf.Exp(-c * (x - k)));
        return f(a) + f(1 - a);
    }

    public static List<Vector2> PositionsAroundX(Vector2 pos, float range)
    {
        List<Vector2> results = new List<Vector2>();
        results.Add(pos + new Vector2(0, range));
        results.Add(pos - new Vector2(0, range));
        results.Add(pos + new Vector2(range, 0));
        results.Add(pos - new Vector2(range, 0));
        results.Add(pos + new Vector2(range, range));
        results.Add(pos - new Vector2(range, range));
        results.Add(pos + new Vector2(range, -range));
        results.Add(pos - new Vector2(range, -range));
        results.Add(pos);
        return results;
    }

    public static GameObject FindWithPos(this List<GameObject> gos, Vector2 pos)
    {
        foreach (GameObject go in gos)
        {
            if ((Vector2)go.transform.position == pos)
            {
                return go;
            }
        }
        return null;
    }

    public static Vector2 RoundBy(this Vector2 pos, float rounder)
    {
        Vector2 result = pos / rounder;
        return new Vector2(Mathf.Round(result.x), Mathf.Round(result.y));
    }

    public static float RoundToNearest(this float x, float num)
    {
        return Mathf.Round(x / num) * num;
    }

    public static Vector2 RoundToNearest(this Vector2 vec, float num)
    {
        return new Vector2(Mathf.Round(vec.x / num) * num, Mathf.Round(vec.y / num) * num);
    }

    public static float RoundTo(this ref float x, float point) => x = Mathf.Round(x * Mathf.Pow(10, point)) / Mathf.Pow(10, point);

    public static float ChangeDigit(float n, float c)
    {
        float length = Mathf.Floor(Mathf.Log10(c) + 1);
        float asTen = Mathf.Pow(10, length);

        float before = Mathf.Round(n / asTen) * asTen;
        float after = n % asTen;
        return before + c + after;
    }

    // Would find a struct in a list of structs where a given variable is a given value
    public static bool Where<T, V>(this List<T> structs, string varName, V value, out int index) 
        where T : struct 
        where V : System.IComparable
    {
        FieldInfo variable = typeof(T).GetField(varName);

        foreach (T _struct in structs)
        {
            if (((V)variable.GetValue(_struct)).CompareTo(value) == 0)
            {
                index = structs.IndexOf(_struct);
                return true;
            }
        }
        index = -1;
        return false;
    }

    public static float Greater(float a, float b) => a > b ? a : b;

    public static bool Is<T>(this T a, params T[] array)
    {
        foreach (T t in array)
        {
            if (t.Equals(a))
                return true;
        }
        return false;
    }

    public static float AsRange(this Vector2 range) => Random.Range(range.x, range.y);
    public static float AsRange(this float num) => Random.Range(-num, num);
    public static float RandomizeSign(this float x) => x * (Random.value > 0.5f ? 1 : -1);
    public static bool IsEven(this int x) => x % 2 == 0;
    public static bool IsEven(this float x) => x % 2 == 0;
    public static Vector2 InverseLerp(Vector2 v, Vector2 a, Vector2 b)
    {
        float x = (v.x - a.x) / (b.x - a.x);
        float y = (v.y - a.y) / (b.y - a.y);
        return new Vector2(x, y);
    }
    public static Vector2 LerpAxis(Vector2 a, Vector2 b, Vector2 t)
    {
        float x = Mathf.Lerp(a.x, b.x, t.x);
        float y = Mathf.Lerp(a.y, b.y, t.y);
        return new Vector2(x, y);
    }

    public static T RandomItem<T>(this IList<T> list, float seed)
    {
        System.Random random = new System.Random(System.BitConverter.ToInt32(System.BitConverter.GetBytes(seed * A_LevelManager.Instance.Seed), 0));

        if (list.Count == 0)
        {
            return default(T);
        }

        int randomIndex = random.Next(list.Count);
        return list[randomIndex];
    }

    public static T RandomItem<T>(this IList<T> list) => list[Random.Range(0, list.Count)];

    public static float RandomBetween(float min, float max, float seed)
    {
        System.Random random = new System.Random(System.BitConverter.ToInt32(System.BitConverter.GetBytes(seed * A_LevelManager.Instance.Seed), 0));
        return (float)(random.NextDouble() * (max - min) + min);
    }

    public static int RandomBetween(int min, int max, float seed)
    {
        System.Random random = new System.Random(System.BitConverter.ToInt32(System.BitConverter.GetBytes(seed * A_LevelManager.Instance.Seed), 0));
        return random.Next(min, max + 1); // "+1" is to include the upper bound in the range
    }

    public static bool RandomChance(float probability, float seed)
    {
        System.Random random = new System.Random(System.BitConverter.ToInt32(System.BitConverter.GetBytes(seed * A_LevelManager.Instance.Seed), 0));
        double randomValue = random.NextDouble();

        return randomValue < probability;
    }

    public static bool RandomChance(float probability, float seed, out double randomValue)
    {
        System.Random random = new System.Random(System.BitConverter.ToInt32(System.BitConverter.GetBytes(seed * A_LevelManager.Instance.Seed), 0));
        randomValue = random.NextDouble();

        return randomValue < probability;
    }

    public static float RemoveNaN(this float x) => float.IsNaN(x) ? 0 : x;
    public static string ToTime(this float x)
    {
        float min = Mathf.Floor(x / 60);
        float sec = Mathf.Floor(x % 60);

        string secStr = sec < 10 ? "0" + sec.ToString() : sec.ToString();

        return min.ToString() + ":" + secStr;
    }

    public static bool Rand(float seed)
    {
        System.Random random = new System.Random(System.BitConverter.ToInt32(System.BitConverter.GetBytes(seed + A_LevelManager.Instance.Seed), 0));

        return random.NextDouble() < 0.5;
    }

    public static bool Rand() => Random.Range(0, 2) == 0;
    public static void SetAlpha(this UnityEngine.UI.Image img, float value)
    {
        Color newColor = img.color;
        newColor.a = value;
        img.color = newColor;
    }
    public static void SetAlpha(this SpriteRenderer rend, float value)
    {
        Color newColor = rend.color;
        newColor.a = value;
        rend.color = newColor;
    }


    public static bool IsBetween(this float a, float min, float max) => (a <= max && a >= min) || (a >= max && a <= min);

    public static float Sign(this float a) => Mathf.Sign(a);

    public static float GetSinWave(SinWave sin, float t) => sin.Midline + sin.Amplitude * Mathf.Sin(Mathf.PI * sin.Frequency * t);
}
