/********************************************************************
	created:	18:3:2019   11:44
	filename: 	NGUIEditorTools.cs
	author:		disen
	des:		从NGUI中摘取部分工具用代码
	modify::	
*********************************************************************/

using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Reflection;

public static class NGUIEditorTools
{
    static public bool minimalisticLook
    {
        get { return GetBool("NGUI Minimalistic", false); }
        set { SetBool("NGUI Minimalistic", value); }
    }

    #region Draw Header Functions
    /// <summary>
    /// Draw a distinctly different looking header label
    /// </summary>

    static public bool DrawHeader(string text) { return DrawHeader(text, text, false, minimalisticLook); }

    /// <summary>
    /// Draw a distinctly different looking header label
    /// </summary>

    static public bool DrawHeader(string text, string key) { return DrawHeader(text, key, false, minimalisticLook); }

    /// <summary>
    /// Draw a distinctly different looking header label
    /// </summary>

    static public bool DrawHeader(string text, bool detailed) { return DrawHeader(text, text, detailed, !detailed); }

    /// <summary>
    /// Draw a distinctly different looking header label
    /// </summary>

    static public bool DrawHeader(string text, string key, bool forceOn, bool minimalistic)
    {
        bool state = EditorPrefs.GetBool(key, true);

        if (!minimalistic) GUILayout.Space(3f);
        if (!forceOn && !state) GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
        GUILayout.BeginHorizontal();
        GUI.changed = false;

        if (minimalistic)
        {
            if (state) text = "\u25BC" + (char)0x200a + text;
            else text = "\u25BA" + (char)0x200a + text;

            GUILayout.BeginHorizontal();
            GUI.contentColor = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.7f) : new Color(0f, 0f, 0f, 0.7f);
            if (!GUILayout.Toggle(true, text, "PreToolbar2", GUILayout.MinWidth(20f))) state = !state;
            GUI.contentColor = Color.white;
            GUILayout.EndHorizontal();
        }
        else
        {
            text = "<b><size=11>" + text + "</size></b>";
            if (state) text = "\u25BC " + text;
            else text = "\u25BA " + text;
            if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) state = !state;
        }

        if (GUI.changed) EditorPrefs.SetBool(key, state);

        if (!minimalistic) GUILayout.Space(2f);
        GUILayout.EndHorizontal();
        GUI.backgroundColor = Color.white;
        if (!forceOn && !state) GUILayout.Space(3f);
        return state;
    }
    #endregion

    #region Begin Contents Functions
    static bool mEndHorizontal = false;
    
    /// <summary>
    /// Begin drawing the content area.
    /// </summary>

    static public void BeginContents() { BeginContents(minimalisticLook); }
    /// <summary>
    /// Begin drawing the content area.
    /// </summary>
    static public void BeginContents (bool minimalistic)
	{
		if (!minimalistic)
		{
			mEndHorizontal = true;
			GUILayout.BeginHorizontal();
			EditorGUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(10f));
		}
		else
		{
			mEndHorizontal = false;
			EditorGUILayout.BeginHorizontal(GUILayout.MinHeight(10f));
			GUILayout.Space(10f);
		}
		GUILayout.BeginVertical();
		GUILayout.Space(2f);
	}

	/// <summary>
	/// End drawing the content area.
	/// </summary>

	static public void EndContents ()
	{
		GUILayout.Space(3f);
		GUILayout.EndVertical();
		EditorGUILayout.EndHorizontal();

		if (mEndHorizontal)
		{
			GUILayout.Space(3f);
			GUILayout.EndHorizontal();
		}

		GUILayout.Space(3f);
	}
    #endregion

    #region Generic Get and Set methods
    /// <summary>
    /// Save the specified boolean value in settings.
    /// </summary>

    static public void SetBool(string name, bool val) { EditorPrefs.SetBool(name, val); }

    /// <summary>
    /// Save the specified integer value in settings.
    /// </summary>

    static public void SetInt(string name, int val) { EditorPrefs.SetInt(name, val); }

    /// <summary>
    /// Save the specified float value in settings.
    /// </summary>

    static public void SetFloat(string name, float val) { EditorPrefs.SetFloat(name, val); }

    /// <summary>
    /// Save the specified string value in settings.
    /// </summary>

    static public void SetString(string name, string val) { EditorPrefs.SetString(name, val); }

    /// <summary>
    /// Save the specified color value in settings.
    /// </summary>

    static public void SetColor(string name, Color c) { SetString(name, c.r + " " + c.g + " " + c.b + " " + c.a); }

    /// <summary>
    /// Save the specified enum value to settings.
    /// </summary>

    static public void SetEnum(string name, System.Enum val) { SetString(name, val.ToString()); }

    /// <summary>
    /// Save the specified object in settings.
    /// </summary>

    static public void Set(string name, Object obj)
    {
        if (obj == null)
        {
            EditorPrefs.DeleteKey(name);
        }
        else
        {
            if (obj != null)
            {
                string path = AssetDatabase.GetAssetPath(obj);

                if (!string.IsNullOrEmpty(path))
                {
                    EditorPrefs.SetString(name, path);
                }
                else
                {
                    EditorPrefs.SetString(name, obj.GetInstanceID().ToString());
                }
            }
            else EditorPrefs.DeleteKey(name);
        }
    }

    /// <summary>
    /// Get the previously saved boolean value.
    /// </summary>

    static public bool GetBool(string name, bool defaultValue) { return EditorPrefs.GetBool(name, defaultValue); }

    /// <summary>
    /// Get the previously saved integer value.
    /// </summary>

    static public int GetInt(string name, int defaultValue) { return EditorPrefs.GetInt(name, defaultValue); }

    /// <summary>
    /// Get the previously saved float value.
    /// </summary>

    static public float GetFloat(string name, float defaultValue) { return EditorPrefs.GetFloat(name, defaultValue); }

    /// <summary>
    /// Get the previously saved string value.
    /// </summary>

    static public string GetString(string name, string defaultValue) { return EditorPrefs.GetString(name, defaultValue); }

    /// <summary>
    /// Get a previously saved color value.
    /// </summary>

    static public Color GetColor(string name, Color c)
    {
        string strVal = GetString(name, c.r + " " + c.g + " " + c.b + " " + c.a);
        string[] parts = strVal.Split(' ');

        if (parts.Length == 4)
        {
            float.TryParse(parts[0], out c.r);
            float.TryParse(parts[1], out c.g);
            float.TryParse(parts[2], out c.b);
            float.TryParse(parts[3], out c.a);
        }
        return c;
    }

    /// <summary>
    /// Get a previously saved enum from settings.
    /// </summary>

    static public T GetEnum<T>(string name, T defaultValue)
    {
        string val = GetString(name, defaultValue.ToString());
        string[] names = System.Enum.GetNames(typeof(T));
        System.Array values = System.Enum.GetValues(typeof(T));

        for (int i = 0; i < names.Length; ++i)
        {
            if (names[i] == val)
                return (T)values.GetValue(i);
        }
        return defaultValue;
    }

    /// <summary>
    /// Get a previously saved object from settings.
    /// </summary>

    static public T Get<T>(string name, T defaultValue) where T : Object
    {
        string path = EditorPrefs.GetString(name);
        if (string.IsNullOrEmpty(path)) return null;

        T retVal = NGUIEditorTools.LoadAsset<T>(path);

        if (retVal == null)
        {
            int id;
            if (int.TryParse(path, out id))
                return EditorUtility.InstanceIDToObject(id) as T;
        }
        return retVal;
    }
    #endregion

    #region Common Tools
    /// <summary>
    /// Load the asset at the specified path.
    /// </summary>

    static public Object LoadAsset(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        return AssetDatabase.LoadMainAssetAtPath(path);
    }

    /// <summary>
    /// Convenience function to load an asset of specified type, given the full path to it.
    /// </summary>

    static public T LoadAsset<T>(string path) where T : Object
    {
        Object obj = LoadAsset(path);
        if (obj == null) return null;

        T val = obj as T;
        if (val != null) return val;

        if (typeof(T).IsSubclassOf(typeof(Component)))
        {
            if (obj.GetType() == typeof(GameObject))
            {
                GameObject go = obj as GameObject;
                return go.GetComponent(typeof(T)) as T;
            }
        }
        return null;
    }

    /// <summary>
    /// Get the specified object's GUID.
    /// </summary>

    static public string ObjectToGUID(Object obj)
    {
        string path = AssetDatabase.GetAssetPath(obj);
        return (!string.IsNullOrEmpty(path)) ? AssetDatabase.AssetPathToGUID(path) : null;
    }

    static MethodInfo s_GetInstanceIDFromGUID;

    /// <summary>
    /// Convert the specified GUID to an object reference.
    /// </summary>

    static public Object GUIDToObject(string guid)
    {
        if (string.IsNullOrEmpty(guid)) return null;

        if (s_GetInstanceIDFromGUID == null)
            s_GetInstanceIDFromGUID = typeof(AssetDatabase).GetMethod("GetInstanceIDFromGUID", BindingFlags.Static | BindingFlags.NonPublic);

        int id = (int)s_GetInstanceIDFromGUID.Invoke(null, new object[] { guid });
        if (id != 0) return EditorUtility.InstanceIDToObject(id);
        string path = AssetDatabase.GUIDToAssetPath(guid);
        if (string.IsNullOrEmpty(path)) return null;
        return AssetDatabase.LoadAssetAtPath(path, typeof(Object));
    }

    /// <summary>
    /// Convert the specified GUID to an object reference of specified type.
    /// </summary>

    static public T GUIDToObject<T>(string guid) where T : Object
    {
        Object obj = GUIDToObject(guid);
        if (obj == null) return null;

        System.Type objType = obj.GetType();
        if (objType == typeof(T) || objType.IsSubclassOf(typeof(T))) return obj as T;

        if (objType == typeof(GameObject) && typeof(T).IsSubclassOf(typeof(Component)))
        {
            GameObject go = obj as GameObject;
            return go.GetComponent(typeof(T)) as T;
        }
        return null;
    }
    #endregion
}
