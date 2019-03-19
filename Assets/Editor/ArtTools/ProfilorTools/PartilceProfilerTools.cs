/********************************************************************
	created:	18:3:2019   16:57
	filename: 	PartilceProfilorTools.cs
	author:		disen
	des:		粒子分析工具
	modify::	
*********************************************************************/

using UnityEngine;
using UnityEditor;

using System.Collections.Generic;

public class PartilceProfilerTools : EditorWindow
{

    private static PartilceProfilerTools window = null;
    [MenuItem("Tools/分析工具/粒子分析工具")]
    private static void Show_Profiler_Particle()
    {
        if (window == null)
        {
            Rect rect = new Rect(0, 0, 550, 800);
            window = (PartilceProfilerTools)EditorWindow.GetWindow(typeof(PartilceProfilerTools));
            window.minSize = new Vector2(rect.width, rect.height);
            window.maxSize = new Vector2(rect.width, rect.height);
        }
        window.Init();
        window.Show();
    }
    [MenuItem("GameObject/分析工具/粒子分析工具", false, 0)]
    private static void Show_Profiler_Particle_Select()
    {
        if (window == null)
        {
            window = (PartilceProfilerTools)EditorWindow.GetWindow(typeof(PartilceProfilerTools), false, "ParticleTools v1.0");
        }
        window.Init();
        window.Show();
    }

    void OnEnable()
    {
        EditorApplication.update += OnEditorUpdate;
    }

    void OnDisable()
    {
        EditorApplication.update -= OnEditorUpdate;

        if (particle_mono_profilor != null)
        {
            DestroyImmediate(particle_mono_profilor.gameObject);
        }
        window = null;
    }

    GUISkin this_skin;
    SingleParticleInfo particle_info = null;
    GameObject single_gobj = null;

    class TextureInfo
    {
        public Texture tex = null;
        public int size = 0;

        public string tex_wh_str = string.Empty;
        public string tex_size_str = string.Empty;

        public void Init(Texture t)
        {
            this.tex = t;

            int instance_id = tex.GetInstanceID();
            string assetpath = AssetDatabase.GetAssetPath(instance_id);
            bool mipmap = false;
            bool isalpha = false;
            if (!string.IsNullOrEmpty(assetpath))
            {
                TextureImporter ti = (TextureImporter)AssetImporter.GetAtPath(assetpath);
                if (ti != null)
                {
                    mipmap = ti.mipmapEnabled;
                    isalpha = ti.alphaIsTransparency;
                }
            }
            //图片分辨率
            this.tex_wh_str = string.Format("{0}*{1}", tex.width, tex.height);
            //计算图片内存占用大小
            int temp_size = (isalpha?2:1)*tex.width * tex.height / 2;
            if (mipmap)
            {
                temp_size = Mathf.RoundToInt((float)temp_size * 1.334375f);
            }
            this.tex_size_str = ((float)temp_size / 1024f).ToString("#.##Kb");
        }
    }

    class MaterialInfo {
        public Material mat = null;
        public bool has_tex = false;
    }

    class MeshInfo {
        public Mesh mesh = null;
        public int mesh_tris = 0;
        public int mesh_size = 0;
    }

    class ParticleObjectInfo
    {
        public ParticleSystem ps = null;
        public MeshInfo mesh_info = new MeshInfo();
        public MaterialInfo material_info = new MaterialInfo();
        public List<TextureInfo> tex_info_list = null;

        public static ParticleObjectInfo Create(GameObject gobj)
        {
            ParticleSystem particle = gobj.GetComponent<ParticleSystem>();
            if (particle == null)
            {
                return null;
            }

            ParticleObjectInfo obj = new ParticleObjectInfo();
            obj.Init(particle);
            return obj;
        }

        void Init(ParticleSystem r_ps)
        {
            this.tex_info_list = new List<TextureInfo>();
            this.ps = r_ps;

            ParticleSystemRenderer render = ps.GetComponent<ParticleSystemRenderer>();

            if (render.mesh != null)
            {
                this.mesh_info.mesh = render.mesh;
                this.mesh_info.mesh_tris = this.mesh_info.mesh.triangles.Length / 3;
                this.mesh_info.mesh_size = (int)UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(this.mesh_info.mesh);
            }

            this.material_info.mat = render.sharedMaterial;
            if (this.material_info.mat != null)
            {
                Texture[] tex_arr = EditorCommonTools.GetContainMaterialTextures(this.material_info.mat);
                for (int i = 0; i < tex_arr.Length; i++)
                {
                    Texture tex = tex_arr[i];
                    TextureInfo tex_info = new TextureInfo();
                    tex_info.Init(tex_arr[i]);
                    this.tex_info_list.Add(tex_info);
                }
                if (tex_arr.Length > 0)
                {
                    this.material_info.has_tex = true;
                }
            }
        }
    }

    class ModelObjectInfo
    {
        public GameObject gobj = null;
        public MeshInfo mesh_info = new MeshInfo();
        public MaterialInfo material_info = new MaterialInfo();
        public List<TextureInfo> tex_info_list = null;

        public static ModelObjectInfo Create(GameObject gobj)
        {
            MeshFilter meshfilter = gobj.GetComponent<MeshFilter>();
            SkinnedMeshRenderer skin_mesh_render = gobj.GetComponent<SkinnedMeshRenderer>();
            if (meshfilter == null && skin_mesh_render == null)
            {
                return null;
            }

            ModelObjectInfo obj = new ModelObjectInfo();
            obj.tex_info_list = new List<TextureInfo>();
            if (skin_mesh_render != null)
            {
                obj.mesh_info.mesh = skin_mesh_render.sharedMesh;
                obj.material_info.mat = skin_mesh_render.sharedMaterial;
            }
            else if(meshfilter != null){
                obj.mesh_info.mesh = meshfilter.sharedMesh;
                MeshRenderer mesh_render = gobj.GetComponent<MeshRenderer>();
                if (mesh_render != null)
                {
                    obj.material_info.mat = mesh_render.sharedMaterial;
                }
            }

            if (obj.mesh_info.mesh != null)
            {
                obj.mesh_info.mesh_tris = obj.mesh_info.mesh.triangles.Length / 3;
                obj.mesh_info.mesh_size = (int)UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(obj.mesh_info.mesh);
            }

            if (obj.material_info.mat != null)
            {
                Texture[] tex_arr = EditorCommonTools.GetContainMaterialTextures(obj.material_info.mat);
                for (int i = 0; i < tex_arr.Length; i++)
                {
                    Texture tex = tex_arr[i];
                    TextureInfo tex_info = new TextureInfo();
                    tex_info.Init(tex);
                    obj.tex_info_list.Add(tex_info);                    
                }

                if (tex_arr.Length > 0)
                {
                    obj.material_info.has_tex = true;
                }
            }

            return obj;
        }
    }

    class SingleParticleInfo
    {
        public GameObject single_gobj = null;

        public int total_tex_count = 0;
        public int total_mat_count = 0;
        public int total_mesh_count = 0;

        public List<ParticleObjectInfo> single_particle_list = null;
        public List<ModelObjectInfo> single_model_list = null;
        public List<TextureInfo> single_tex_list = null;
        public List<MeshInfo> single_mesh_list = null;
        public List<MaterialInfo> single_material_list = null;

        public void Init(GameObject gobj)
        {
            if (gobj == null)
            {
                return;
            }
            this.total_tex_count = 0;
            this.total_mat_count = 0;
            this.total_mesh_count = 0;

            this.single_particle_list = new List<ParticleObjectInfo>();
            this.single_model_list = new List<ModelObjectInfo>();
            this.single_tex_list = new List<TextureInfo>();
            this.single_material_list = new List<MaterialInfo>();
            this.single_mesh_list = new List<MeshInfo>();
            this.single_gobj = null;
            if(gobj != null)
            {
                this.single_gobj = gobj;
                RecursiveFind(this.single_gobj);
            }
            this.total_tex_count = single_tex_list.Count;
            this.total_mesh_count = single_mesh_list.Count;
            this.total_mat_count = single_material_list.Count;
        }

        void AddToList(List<TextureInfo> tex_list, MeshInfo mesh_info, MaterialInfo mat_info)
        {
            for (int i = 0; i < tex_list.Count; i++)
            {
                TextureInfo tex_info = tex_list[i];
                if (single_tex_list.Find((TextureInfo ti) => ti.tex == tex_info.tex) == null)
                {
                    single_tex_list.Add(tex_info);
                }
            }
            if (single_material_list.Find((MaterialInfo mi) => mi.mat == mat_info.mat) == null)
            {
                single_material_list.Add(mat_info);
            }
            if (single_mesh_list.Find((MeshInfo mi) => mi.mesh == mesh_info.mesh) == null)
            {
                single_mesh_list.Add(mesh_info);
            }
        }

        void RecursiveFind(GameObject gobj)
        {
            ParticleObjectInfo poi = ParticleObjectInfo.Create(gobj);
            ModelObjectInfo moi = ModelObjectInfo.Create(gobj);

            if (poi != null)
            {
                single_particle_list.Add(poi);
                AddToList(poi.tex_info_list, poi.mesh_info, poi.material_info);
            }

            if (moi != null)
            {
                single_model_list.Add(moi);
                AddToList(moi.tex_info_list, moi.mesh_info, moi.material_info);
            }

            Transform tobj = gobj.transform;
            for (int i = 0; i < tobj.childCount; i++)
            {
                RecursiveFind(tobj.GetChild(i).gameObject);
            }
        }
    }
    
    void Init()
    {
        this_skin = AssetDatabase.LoadAssetAtPath<GUISkin>("Assets/Editor/ArtTools/ProfilorTools/PartilceProfilorSkin.guiskin");
        SelectionGobjSet();
        AnylizeGobj();
    }

    void SelectionGobjSet()
    {
        if (Selection.activeGameObject != null)
        {
            single_gobj = Selection.activeGameObject;
        }
    }

    void AnylizeGobj()
    {
        if (particle_info == null)
        {
            particle_info = new SingleParticleInfo();
        }
        particle_info.Init(single_gobj);

        if (particle_mono_profilor != null)
        {
            DestroyImmediate(particle_mono_profilor.gameObject);
        }
    }

    int GetAliveParticles(ParticleSystem ps)
    {
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ps.particleCount];
        return ps.GetParticles(particles);
    }

    Vector2 tex_scroll_view_vec2 = Vector2.zero;
    Vector2 mat_scroll_view_vec2 = Vector2.zero;
    Vector2 mesh_scroll_view_vec2 = Vector2.zero;
    Vector2 particle_scroll_view_vec2 = Vector2.zero;

    ParticleSystemProfiler particle_mono_profilor = null;

    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("刷新粒子数据", GUILayout.Width(150)))
        {
            if (single_gobj != null)
            {
                AnylizeGobj();
            }
        }
        if (GUILayout.Button("切换至新选中粒子", GUILayout.Width(150)))
        {
            SelectionGobjSet();
            AnylizeGobj();
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(20);
        if (single_gobj == null || particle_info == null)
        {
            return;
        }
        if (!single_gobj.activeSelf)
        {
            single_gobj.SetActive(true);
        }
        if (GUILayout.Button("重新播放粒子", GUILayout.Width(150), GUILayout.Height(40)))
        {
            if (single_gobj != null)
            {
                single_gobj.SetActive(false);
            }
            if(particle_mono_profilor != null)
            {
                particle_mono_profilor.RefreshData();
            }
        }
        GUILayout.Label("粒子名称:" + particle_info.single_gobj.name);
        GUILayout.Label("贴图总数量:" + particle_info.total_tex_count);
        GUILayout.Label("材质总数量:" + particle_info.total_mat_count);
        GUILayout.Label("mesh总数量:" + particle_info.total_mesh_count);

        if(EditorApplication.isPlaying)
        {
            if (particle_info != null && particle_mono_profilor != null)
            {
                NGUIEditorTools.BeginContents();
                GUILayout.BeginVertical();
                int particle_count = 0;
                for (int i = 0; i < particle_mono_profilor.m_ParticleSystems.Length; i++)
                {
                    GUILayout.BeginHorizontal();
                    if(GUILayout.Button(particle_mono_profilor.m_ParticleSystems[i].name, this_skin.label, GUILayout.Width(100)))
                    {
                        Selection.activeGameObject = particle_mono_profilor.m_ParticleSystems[i].gameObject;
                    }
                    GUILayout.Label("峰值数量:" + particle_mono_profilor.ps_count_arr[i], GUILayout.Width(100));
                    GUILayout.EndHorizontal();
                }
                GUILayout.Space(20);
                GUILayout.Label("粒子峰值总数:" + particle_mono_profilor.ps_max_count);
                GUILayout.EndVertical();
                NGUIEditorTools.EndContents();
                window.Repaint();
            }
        }
        particle_scroll_view_vec2 = GUILayout.BeginScrollView(particle_scroll_view_vec2);
        #region texture anylize
        if (NGUIEditorTools.DrawHeader("贴图分析"))
        {
            int height = particle_info.single_tex_list.Count * 50+30;
            tex_scroll_view_vec2 = GUILayout.BeginScrollView(tex_scroll_view_vec2, GUILayout.MinHeight(height));
            GUILayout.BeginVertical();
            for (int i = 0; i < particle_info.single_tex_list.Count; i++)
            {
                TextureInfo tex_info = particle_info.single_tex_list[i];

                GUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent(tex_info.tex), this_skin.box, GUILayout.Width(50), GUILayout.Height(50)))
                {
                    Selection.activeObject = tex_info.tex;
                }
                GUILayout.Label(tex_info.tex.name, GUILayout.Width(200));
                GUILayout.Label(tex_info.tex_wh_str, GUILayout.Width(70));
                GUILayout.Label(tex_info.tex_size_str, GUILayout.Width(100));
                GUILayout.EndHorizontal();

                
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }
        #endregion

        #region material anylize
        if (NGUIEditorTools.DrawHeader("材质分析"))
        {
            int height = particle_info.single_material_list.Count * 20 + 30;
            mat_scroll_view_vec2 = GUILayout.BeginScrollView(mat_scroll_view_vec2, GUILayout.MinHeight(height));
            GUILayout.BeginVertical();
            for (int i = 0; i < particle_info.single_material_list.Count; i++)
            {
                MaterialInfo info = particle_info.single_material_list[i];
                if (info.mat != null)
                {
                    GUILayout.BeginHorizontal();
                    info.mat = (Material)EditorGUILayout.ObjectField(new GUIContent(""), info.mat, typeof(Material), true, GUILayout.Width(70));
                    if (GUILayout.Button(info.mat.name, this_skin.label, GUILayout.Width(200)))
                    {
                        Selection.activeObject = info.mat;
                    }
                    if (info.mat.shader.name == "Hidden/InternalErrorShader")
                    {
                        GUILayout.Label("shader错误!", GUILayout.Width(100));
                    }
                    if (!info.has_tex)
                    {
                        GUILayout.Label("没有贴图!", GUILayout.Width(100));
                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }
        #endregion

        #region mesh anylize
        if (NGUIEditorTools.DrawHeader("mesh分析"))
        {
            int height = particle_info.single_mesh_list.Count * 20 + 30;
            mesh_scroll_view_vec2 = GUILayout.BeginScrollView(mesh_scroll_view_vec2, GUILayout.MinHeight(height));
            GUILayout.BeginVertical();
            for (int i = 0; i < particle_info.single_mesh_list.Count; i++)
            {
                MeshInfo info = particle_info.single_mesh_list[i];
                if(info.mesh != null)
                {
                    GUILayout.BeginHorizontal();
                    info.mesh = (Mesh)EditorGUILayout.ObjectField(new GUIContent(""), info.mesh, typeof(Mesh), true, GUILayout.Width(70));
                    if (GUILayout.Button(info.mesh.name, this_skin.label, GUILayout.Width(200)))
                    {
                        Selection.activeObject = info.mesh;
                    }
                    GUILayout.Label(string.Format("面数:{0} 大小:{1}", info.mesh_tris, info.mesh_size), GUILayout.Width(200));
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }
        #endregion

        GUILayout.EndScrollView();
    }

    void OnEditorUpdate()
    {
        if (EditorApplication.isPlaying)
        {
            if (particle_info != null)
            {
                if (particle_mono_profilor == null)
                {
                    ParticleSystem[] ps_arr = new ParticleSystem[particle_info.single_particle_list.Count];
                    for(int i = 0 ;  i < particle_info.single_particle_list.Count ; i ++)
                    {
                        ps_arr[i] = particle_info.single_particle_list[i].ps;
                    }
                    particle_mono_profilor = ParticleSystemProfiler.Create(particle_info.single_gobj, ps_arr);
                    window.Repaint();
                }
            }

            if (window == null)
            {
                Show_Profiler_Particle_Select();
            }
        }
    }
}