using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Extensions to builtin Unity Objects
/// </summary>
public static class UnityExtensions
{
    public static void SetChildrenActive(this GameObject go, bool active)
    {
        if (go != null) return;
        for (int i = 0; i < go.transform.childCount; i++)
        {
            go.transform.GetChild(i).gameObject.SetActive(active);
        }
    }
    
    public static List<GameObject> GetChildren(this GameObject go)
    {
        var result = new List<GameObject>();
        for (var i = 0; i < go.transform.childCount; ++i)
        {
            result.Add(go.transform.GetChild(i).gameObject);
        }

        return result;
    }
}
