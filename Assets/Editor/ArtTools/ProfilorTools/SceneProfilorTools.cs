/********************************************************************
	created:	18:3:2019   11:34
	filename: 	SceneProfilorTools.cs
	author:		disen
	des:		场景数据分析工具
	modify::	
*********************************************************************/

using UnityEditor;
using UnityEngine;
using ED_SceneManagement = UnityEditor.SceneManagement;
using EN_SceneManagement = UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class SceneProfilorTools : EditorWindow
{
    [MenuItem("Tools/分析工具/场景性能分析工具")]
    private static void Show_Profiler_Scene()
    {
        SceneProfilorTools window = (SceneProfilorTools)EditorWindow.GetWindowWithRect(typeof(SceneProfilorTools), new Rect(0, 0, 550, 1000), true, "SceneTools v1.0");
        window.Show();
    }

    class PrefObjectInfo
    {
        public int instance_id = 0;
        public string obj_name = string.Empty;
        public string size_str = string.Empty;
        public int size_count = 0;
        public string size_count_str = string.Empty;
        public int pref_count = 0;
        public bool tex_mipmap = false;

        public List<ObjectInfo> ref_objs_list = new List<ObjectInfo>();
    }

    class SceneGroupObjInfo
    {
        public string scene_name = string.Empty;
        public int tris_all = 0;//总三角面数
        public int verts_all = 0;//总顶点数
        public int mats_all = 0;//材质球数
        public int texs_all = 0;//贴图数
        public int meshs_all = 0;//mesh数

        public int texs_size_all = 0;//贴图总内存

        public List<GroupObjInfo> group_obj_list = new List<GroupObjInfo>();
        public List<int> tex_all_list = new List<int>();
        public List<int> mat_all_list = new List<int>();
        public List<int> mesh_all_list = new List<int>();
        public Dictionary<int, PrefObjectInfo> mesh_pref_dic = new Dictionary<int, PrefObjectInfo>();
        public Dictionary<int, PrefObjectInfo> tex_pref_dic = new Dictionary<int, PrefObjectInfo>();
        public Dictionary<string, int> tex_size_dic = new Dictionary<string, int>();

        public SceneGroupObjInfo(string scene_name)
        {
            this.scene_name = scene_name;
        }

        public void Add_Group_Info(GroupObjInfo groupinfo)
        {
            group_obj_list.Add(groupinfo);
            tris_all += groupinfo.tris_all;
            verts_all += groupinfo.verts_all;
            mats_all = mat_all_list.Count;
            texs_all = tex_all_list.Count;
            meshs_all = mesh_all_list.Count;
        }

        public void ClearData()
        {
            tex_all_list.Clear();
            mat_all_list.Clear();
            mesh_all_list.Clear();
            group_obj_list.Clear();
        }
    }

    class GroupObjInfo
    {
        public SceneGroupObjInfo scene_info = null;
        public string group = string.Empty;
        public GameObject group_obj = null;
        public int tris_all = 0;
        public int verts_all = 0;
        public int mats_all = 0;
        public int texs_all = 0;
        public int meshs_all = 0;


        public List<ObjectInfo> objs_info_all_list = new List<ObjectInfo>();

        public GroupObjInfo(GameObject gobj, SceneGroupObjInfo sceneinfo)
        {
            this.group_obj = gobj;
            this.group = sceneinfo.scene_name;
            this.scene_info = sceneinfo;
        }

        public void Add_Child_Info(ObjectInfo info)
        {
            objs_info_all_list.Add(info);
            tris_all += info.tris;

            //整理图片数据
            if (info.texture != null)
            {
                for (int i = 0; i < info.texture.Length; i++)
                {
                    Texture tex = info.texture[i];
                    int instance_id = tex.GetInstanceID();
                    string assetpath = AssetDatabase.GetAssetPath(instance_id);
                    bool mipmap = false;
                    bool isalpha = false;
                    if (!string.IsNullOrEmpty(assetpath))
                    {
                        TextureImporter ti = (TextureImporter)AssetImporter.GetAtPath(assetpath);
                        if(ti != null)
                        {
                            mipmap = ti.mipmapEnabled;
                            isalpha = ti.alphaIsTransparency;
                        }
                    }
                    if (!scene_info.tex_all_list.Contains(instance_id))
                    {
                        scene_info.tex_all_list.Add(instance_id);
                        //整理收集所有图片的信息,根据分辨率分析占用图的个数
                        string size_str = string.Format("{0}*{1}", tex.width, tex.height);
                        if (!scene_info.tex_size_dic.ContainsKey(size_str))
                        {
                            scene_info.tex_size_dic.Add(size_str, 0);
                        }
                        scene_info.tex_size_dic[size_str] += 1;
                        texs_all += 1;
                        PrefObjectInfo pref_info = null;
                        if (!scene_info.tex_pref_dic.TryGetValue(instance_id, out pref_info))
                        {
                            pref_info = new PrefObjectInfo();
                            pref_info.instance_id = instance_id;
                            pref_info.obj_name = tex.name;
                            pref_info.size_str = size_str;
                            pref_info.tex_mipmap = mipmap;
                            pref_info.size_count = (isalpha ? 2 : 1) * tex.width * tex.height / 2;
                            if (pref_info.tex_mipmap)
                            {
                                pref_info.size_count = Mathf.RoundToInt((float)pref_info.size_count * 1.334375f);
                            }
                            pref_info.size_count_str = ((float)pref_info.size_count / 1024f).ToString("#.##");
                            scene_info.tex_pref_dic.Add(instance_id, pref_info);
                            scene_info.texs_size_all += pref_info.size_count;
                        }

                        pref_info.pref_count++;
                        pref_info.ref_objs_list.Add(info);
                    }
                }
            }
            //整理材质数据
            if (info.material != null)
            {
                int instance_id = info.material.GetInstanceID();
                if (!scene_info.mat_all_list.Contains(instance_id))
                {
                    scene_info.mat_all_list.Add(instance_id);
                    mats_all += 1;
                }
            }
            //整理mesh数据
            if (info.mesh != null)
            {
                int instance_id = info.mesh.GetInstanceID();
                if (!scene_info.mesh_all_list.Contains(instance_id))
                {
                    scene_info.mesh_all_list.Add(instance_id);
                    meshs_all += 1;
                    verts_all += info.verts;
                }
                PrefObjectInfo pref_info = null;
                if (!scene_info.mesh_pref_dic.TryGetValue(instance_id, out pref_info))
                {
                    pref_info = new PrefObjectInfo();
                    pref_info.instance_id = instance_id;
                    pref_info.obj_name = info.mesh.name;
                    pref_info.size_str = info.tris.ToString();
                    scene_info.mesh_pref_dic.Add(instance_id, pref_info);
                }
                pref_info.pref_count ++;
                pref_info.ref_objs_list.Add(info);
            }
        }
    }

    class ObjectInfo
    {
        public GameObject obj;
        public Mesh mesh = null;
        public int tris = 0;
        public int verts = 0;
        public Material material = null;
        public string shaderName = string.Empty;
        public Texture[] texture = null;
        public bool multiMat = false;

        public static ObjectInfo Create_Analyze(GameObject gobj)
        {
            ObjectInfo obj = null;
            //分析mesh
            MeshFilter mesh = gobj.GetComponent<MeshFilter>();
            SkinnedMeshRenderer mesh_renderer = gobj.GetComponent<SkinnedMeshRenderer>();
            Mesh sharedMesh = null;
            if (mesh == null && mesh_renderer == null)
            {
                return obj;
            }
            if (mesh != null)
            {
                sharedMesh = mesh.sharedMesh;
            }
            else if (mesh_renderer != null)
            {
                sharedMesh = mesh_renderer.sharedMesh;
            }
            
            if (sharedMesh == null || sharedMesh.triangles == null || sharedMesh.triangles.Length == 0)
            {
                return obj;
            }

            obj = new ObjectInfo();
            obj.obj = gobj;
            obj.mesh = sharedMesh;
            obj.tris = sharedMesh.triangles.Length / 3;
            obj.verts = (int)UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(sharedMesh);
            //分析Material
            MeshRenderer render = gobj.GetComponent<MeshRenderer>();
            if (render != null)
            {
                if (render.sharedMaterials.Length == 1)
                {
                    obj.material = render.sharedMaterials[0];
                }
                else if (render.sharedMaterials.Length > 1)
                {
                    obj.multiMat = true;
                    obj.material = render.sharedMaterials[0];
                }
            }

            if (obj.material != null)
            {
                obj.shaderName = obj.material.shader.name;
                obj.texture = EditorCommonTools.GetContainMaterialTextures(obj.material);
            }
            return obj;
        }
    }

    /// <summary>
    /// 递归分析组数据
    /// </summary>
    /// <param name="group_info"></param>
    /// <param name="group_tran"></param>
    void AnalyzeGroup(GroupObjInfo group_info, Transform group_tran)
    {
        ObjectInfo obj_info = ObjectInfo.Create_Analyze(group_tran.gameObject);
        if (obj_info != null)
        {
            group_info.Add_Child_Info(obj_info);
        }
        if (group_tran.childCount > 0)
        {
            for (int i = 0; i < group_tran.childCount; i++)
            {
                AnalyzeGroup(group_info, group_tran.GetChild(i));
            }
        }
    }

    /// <summary>
    /// 分析当前场景的数据信息
    /// </summary>
    void AnalyzeCurrentScene()
    {
        string root_name = ED_SceneManagement.EditorSceneManager.GetActiveScene().name;
        EN_SceneManagement.Scene scene = ED_SceneManagement.EditorSceneManager.GetActiveScene();
        GameObject[] gobjs = scene.GetRootGameObjects();
        SceneGroupObjInfo scene_group_info = new SceneGroupObjInfo(scene.name);

        for (int j = 0; j < gobjs.Length; j ++ )
        {
            GameObject root_obj = gobjs[j];
            Transform root_tran = root_obj.transform;

            GroupObjInfo group_obj_info = new GroupObjInfo(null, scene_group_info);

            for (int i = 0; i < root_tran.childCount; i++)
            {
                Transform child = root_tran.GetChild(i);
                if (child.gameObject.activeSelf)
                {
                    AnalyzeGroup(group_obj_info, child);
                }
            }

            scene_group_info.Add_Group_Info(group_obj_info);
        }

        scene_group_obj_info = scene_group_info;
    }

    /// <summary>
    /// 分析当前选中物体的信息
    /// </summary>
    void AnalyzeSelectGameObject()
    {
        //if (Selection.activeGameObject != null)
        //{
        //    GameObject selectObj = Selection.activeGameObject;
        //    SceneGroupObjInfo scene_group_info = new SceneGroupObjInfo(selectObj);
        //    GroupObjInfo group_obj_info = new GroupObjInfo(Selection.activeGameObject, scene_group_info);
        //    AnalyzeGroup(group_obj_info, selectObj.transform);
        //    scene_group_info.Add_Group_Info(group_obj_info);
        //}
    }



    static SceneGroupObjInfo scene_group_obj_info = null;
    private Vector2 mesh_scroll_view_vec2 = Vector2.zero;
    private Vector2 tex_scroll_view_vec2 = Vector2.zero;
    private int tex_list_index = 0;
    private int tex_select_index = 0;//texture引用查询index
    private int mesh_list_index = 0;
    private int mesh_select_index = 0;//mesh引用查询index

    private Color default_font_color = Color.white;
    
    void OnGUI()
    {
        if (GUILayout.Button("分析当前场景"))
        {
            AnalyzeCurrentScene();

            tex_list_index = 0;
            tex_select_index = 0;
            mesh_list_index = 0;
            mesh_select_index = 0;
        }
        default_font_color = GUI.contentColor;
        if (scene_group_obj_info != null)
        {
            GUILayout.BeginVertical(GUILayout.Width(550));
            EditorGUILayout.LabelField("场景名称:", scene_group_obj_info.scene_name);
            EditorGUILayout.LabelField("总三角面数:", scene_group_obj_info.tris_all.ToString());
            //EditorGUILayout.LabelField("总顶点数:", scene_group_obj_info.verts_all.ToString());
            EditorGUILayout.LabelField("总mesh内存占用:", ((float)scene_group_obj_info.verts_all / 1024f).ToString("#.##") + "Kb");
            EditorGUILayout.LabelField("总材质球数:", scene_group_obj_info.mats_all.ToString());
            EditorGUILayout.LabelField("图片内存占用:", ((float)scene_group_obj_info.texs_size_all / 1024f).ToString("#.##") + "Kb");
            EditorGUILayout.Separator();
            NGUIEditorTools.BeginContents();
            EditorGUILayout.LabelField("总的贴图数:", scene_group_obj_info.texs_all.ToString());
            if (NGUIEditorTools.DrawHeader("贴图分析"))
            {
                //GUILayout.Label("");

                List<string> tex_size_keys = new List<string>(scene_group_obj_info.tex_size_dic.Keys);
                for (int i = 0; i < tex_size_keys.Count; i++)
                {
                    EditorGUILayout.LabelField(string.Format("tex-{0}:", tex_size_keys[i]), scene_group_obj_info.tex_size_dic[tex_size_keys[i]].ToString());
                }

                EditorGUILayout.Separator();
                GUILayout.Label("图片数据");

                GUILayout.BeginHorizontal();
                GUILayout.Label("名称", GUILayout.Width(200));
                GUILayout.Label("引用", GUILayout.Width(20));
                GUILayout.Label("尺寸", GUILayout.Width(70));
                GUILayout.Label("大小", GUILayout.Width(100));
                GUILayout.Label("Mip", GUILayout.Width(40));
                GUILayout.Label("引用查询", GUILayout.Width(50));
                GUILayout.EndHorizontal();
                tex_scroll_view_vec2 = GUILayout.BeginScrollView(tex_scroll_view_vec2, GUILayout.MinHeight(550));

                List<int> tex_pref_keys = new List<int>(scene_group_obj_info.tex_pref_dic.Keys);
                for (int i = 0; i < tex_pref_keys.Count; i++)
                {
                    PrefObjectInfo pref_info = scene_group_obj_info.tex_pref_dic[tex_pref_keys[i]];
                    GUILayout.BeginHorizontal();
                    if (tex_list_index == i)
                    {
                        GUI.contentColor = Color.blue;
                    }
                    if (GUILayout.Button(pref_info.obj_name + ":", GUI.skin.label, GUILayout.Width(200)))
                    {
                        Selection.activeObject = AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(pref_info.instance_id), typeof(Object));
                        tex_list_index = i;
                    }

                    GUILayout.Label(pref_info.pref_count.ToString(), GUILayout.Width(20));
                    GUILayout.Label(pref_info.size_str.ToString(), GUILayout.Width(70));
                    GUILayout.Label(pref_info.size_count_str + "Kb", GUILayout.Width(100));
                    GUILayout.Label(string.Format("{0}", pref_info.tex_mipmap), GUILayout.Width(40));
                    if (tex_list_index == i)
                    {
                        if (GUILayout.Button("<", GUI.skin.label, GUILayout.Width(15)))
                        {
                            SelectPrefSelectIndex(ref tex_select_index, pref_info, false);
                        }
                        if (GUILayout.Button(tex_select_index.ToString(), GUI.skin.label, GUILayout.Width(20)))
                        {
                            ObjectInfo obj_info = pref_info.ref_objs_list[tex_select_index];
                            if (obj_info.obj != null)
                            {
                                Selection.activeGameObject = obj_info.obj;
                            }
                        }
                        if (GUILayout.Button(">", GUI.skin.label, GUILayout.Width(15)))
                        {
                            SelectPrefSelectIndex(ref tex_select_index, pref_info, true);
                        }
                    }
                    GUI.contentColor = default_font_color;
                    GUILayout.EndHorizontal();
                    //EditorGUILayout.LabelField(string.Format("{0}:{1}:{2}:{3}", , , pref_info.size_str, pref_info.size_count));
                }
                GUILayout.EndScrollView();
            }
            NGUIEditorTools.EndContents();
            EditorGUILayout.Separator();
            NGUIEditorTools.BeginContents();
            EditorGUILayout.LabelField("总的mesh数:", scene_group_obj_info.meshs_all.ToString());
            if (NGUIEditorTools.DrawHeader("Mesh分析"))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("名称", GUILayout.Width(200));
                GUILayout.Label("引用", GUILayout.Width(20));
                GUILayout.Label("三角面", GUILayout.Width(100));
                GUILayout.Label("引用查询", GUILayout.Width(50));
                GUILayout.EndHorizontal();
                mesh_scroll_view_vec2 = GUILayout.BeginScrollView(mesh_scroll_view_vec2, GUILayout.MinHeight(550));
                List<int> mesh_pref_keys = new List<int>(scene_group_obj_info.mesh_pref_dic.Keys);
                for (int i = 0; i < mesh_pref_keys.Count; i++)
                {
                    PrefObjectInfo pref_info = scene_group_obj_info.mesh_pref_dic[mesh_pref_keys[i]];
                    GUILayout.BeginHorizontal();
                    if (mesh_list_index == i)
                    {
                        GUI.contentColor = Color.blue;
                    }
                    if (GUILayout.Button(pref_info.obj_name, GUI.skin.label, GUILayout.Width(200)))
                    {
                        Selection.activeObject = AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(pref_info.instance_id), typeof(Object));
                        mesh_list_index = i;
                    }
                    GUILayout.Label(pref_info.pref_count.ToString(), GUILayout.Width(20));
                    GUILayout.Label("v:"+pref_info.size_str, GUILayout.Width(100));
                    if (mesh_list_index == i)
                    {
                        if (GUILayout.Button("<", GUI.skin.label, GUILayout.Width(15)))
                        {
                            SelectPrefSelectIndex(ref mesh_select_index, pref_info, false);
                        }
                        if (GUILayout.Button(mesh_select_index.ToString(), GUI.skin.label, GUILayout.Width(20)))
                        {
                            ObjectInfo obj_info = pref_info.ref_objs_list[mesh_select_index];
                            if (obj_info.obj != null)
                            {
                                Selection.activeGameObject = obj_info.obj;
                            }
                        }
                        if (GUILayout.Button(">", GUI.skin.label, GUILayout.Width(15)))
                        {
                            SelectPrefSelectIndex(ref mesh_select_index, pref_info, true);
                        }
                    }
                    GUI.contentColor = default_font_color;
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();
            }
            NGUIEditorTools.EndContents();
            //EditorGUILayout.Separator();
            //if (NGUIEditorTools.DrawHeader("场景层次分析"))
            //{
            //    NGUIEditorTools.BeginContents();
            //    for (int i = 0; i < scene_group_obj_info.group_obj_list.Count; i++)
            //    {
            //        GroupObjInfo gourp_info = scene_group_obj_info.group_obj_list[i];
            //        if (NGUIEditorTools.DrawHeader(string.Format("组:{0}", gourp_info.group)))
            //        {
            //            EditorGUILayout.LabelField("总三角面数:", gourp_info.tris_all.ToString());
            //            EditorGUILayout.LabelField("总材质球数:", gourp_info.mats_all.ToString());
            //            EditorGUILayout.LabelField("总的贴图数:", gourp_info.texs_all.ToString());
            //            EditorGUILayout.LabelField("总的mesh数:", gourp_info.meshs_all.ToString());
            //        }
            //    }
            //    NGUIEditorTools.EndContents();
            //}
            GUILayout.EndVertical();
        }
    }

    void SelectPrefSelectIndex(ref int select_index, PrefObjectInfo pref_info, bool forward)
    {
        int length = pref_info.ref_objs_list.Count;
        if (length == 0)
        {
            return;
        }
        if (select_index > 0 && select_index < length)
        {

        }
        else {
            select_index = 0;
        }
        if (forward)
        {
            select_index += 1;
            if (select_index >= length)
            {
                select_index = 0;
            }
        }
        else {
            select_index -= 1;
            if (select_index < 0)
            {
                select_index = length - 1;
            }
        }

        ObjectInfo obj_info = pref_info.ref_objs_list[select_index];
        if(obj_info.obj != null)
        {
            Selection.activeGameObject = obj_info.obj;
        }
    }
}
