using ImstkUnity;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace ImstkEditor
{
    public class EditorTools
    {
        static public List<T> ListField<T>(string title, List<T> items) where T : UnityEngine.Object {
            List<T> result;
            if (items == null)
            {
                result = new List<T>();
            } 
            else
            {
                result = new List<T>(items);
            }
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(title);
            for (int i = 0; i<items.Count; ++i)
            {
                result[i] = (T)EditorGUILayout.ObjectField(items[i], typeof(T), true);
            }
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
            {
                result.Add(null);
            }

            bool enabled = GUI.enabled;
            GUI.enabled = result.Count > 0;
            if (GUILayout.Button("-"))
            {
              
                result.RemoveAt(result.Count-1);
            }
            GUI.enabled =enabled;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            return result;
        }
    }
}
