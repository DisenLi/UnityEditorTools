using System.Reflection;
using UnityEngine;
using System.Collections.Generic;

public class ParticleSystemProfilor : MonoBehaviour
{
    private static ParticleSystemProfilor instance = null;

    public GameObject root_gobj;
    public ParticleSystem[] m_ParticleSystems;
    public int ps_count = 0;
    public int ps_alive_count = 0;

    public int ps_max_count = 0;

    public string[] ps_name_arr = null;
    public int[] ps_count_arr = null;

    public static ParticleSystemProfilor Create(GameObject ps_gobj, ParticleSystem[] ps_arr)
    {
        if (instance != null)
        {
            DestroyImmediate(instance.gameObject);
        }

        GameObject gobj = new GameObject("particlesystem_profilor");
        ParticleSystemProfilor psp = gobj.AddComponent<ParticleSystemProfilor>();
        psp.root_gobj = ps_gobj;
        psp.m_ParticleSystems = ps_arr;

        instance = psp;
        return psp;
    }

    public void RefreshData()
    {
        ps_count = 0;
        ps_max_count = 0;
        for (int i = 0; i < ps_count_arr.Length; i++)
        {
            ps_count_arr[i] = 0;   
        }
    }

    void Start()
    {
        ps_count_arr = new int[m_ParticleSystems.Length];
        ps_name_arr = new string[m_ParticleSystems.Length];

        for (int i = 0; i < m_ParticleSystems.Length; i++)
        {
            ps_name_arr[i] = m_ParticleSystems[i].name;
        }
    }

    private void Update()
    {
        ps_count = 0;
        ps_alive_count = 0;
        for (int i = 0; i < m_ParticleSystems.Length; i ++ )
        {
            ParticleSystem ps = m_ParticleSystems[i];
            int count = ps.particleCount;
            int alive_count = GetAliveParticles(ps);

            if (count > ps_count_arr[i])
            {
                ps_count_arr[i] = count;
            }

            ps_count += count;
        }

        if (ps_count > ps_max_count)
        {
            ps_max_count = ps_count;
        }
    }

    int GetAliveParticles(ParticleSystem ps)
    {
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ps.particleCount];
        return ps.GetParticles(particles);
    }

    private void OnGUI()
    {
        GUILayout.Label(string.Format("<size=25>粒子:{0}</size>", root_gobj.name));
        GUILayout.Label(string.Format("<size=25>实时最大数量:{0}</size>", ps_count));
        GUILayout.Label(string.Format("<size=25>峰值数量:{0}</size>", ps_max_count));
    }
}