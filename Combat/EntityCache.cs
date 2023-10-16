using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityCache : MonoBehaviour
{
    public static Dictionary<System.Type, object> Cache = new Dictionary<System.Type, object>();
    public class CacheRefs<T>
        where T : Entity
    {        
        public LinkedList<T> list = new LinkedList<T>();
    }

    public static void Push<T>(T entity)
        where T : Entity
    {
        var key = typeof(T);
        CacheRefs<T> refs = null;
        if (Cache[key] == null)
        {
            Cache.Add(key, new CacheRefs<T>());
        }
        else
        {
            refs = (CacheRefs<T>)Cache[key];
        }

        refs.list.AddLast(entity);
    }

    public static bool TryPop<T>(out T val)
        where T : Entity
    {
        val = null;
        var key = typeof(T);
        if (Cache[key] == null)
            return false;

        var list = ((CacheRefs<T>)Cache[key]).list;
        val = list.First.Value;

        if (val == null)
            return false;

        list.RemoveFirst();
        return true;
    }
}
