using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;

public static class EditorCommonTools
{
    /// <summary>
    /// 获得材质球所有的图片
    /// </summary>
    /// <param name="mat"></param>
    /// <returns></returns>
    public static Texture[] GetContainMaterialTextures(Material mat)
    {
        List<Texture> tex_list = new List<Texture>();

        Shader shader = mat.shader;
        for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); i++)
        {
            if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
            {
                string propertyName = ShaderUtil.GetPropertyName(shader, i);
                Texture tex = mat.GetTexture(propertyName);
                if (tex != null)
                {
                    tex_list.Add(tex);
                }
            }
        }

        return tex_list.ToArray();
    }
}