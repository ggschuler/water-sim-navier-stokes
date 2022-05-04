using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationSPH : MonoBehaviour
{
    [Header("Particle parameters")]
    [SerializeField] private GameObject particlePrefab;
    [SerializeField] private float radius;
    [SerializeField] private float mass = 2.0f;
    [SerializeField] private float restDensity = 300.0f;
    [SerializeField] private float viscosity = 200.0f;
    [SerializeField] private float drag;
    [Tooltip("gas constant dependent on system's temperature."),
     SerializeField] private float gasConst = 2000.0f;

    [Header("Simulation size parameters")]
    [SerializeField] private int amount = 100;
    [SerializeField, Range(1,1000)] private int perRow = 6;

    private float _smoothingRadius = 1.0f;
    private Vector3 _gravity = new Vector3(0.0f, -9.81f, 0.0f);
    [SerializeField] private float gravityMult = 2000.0f;
    
    private Particle[] particles;
    private static float kernelDistance = 16.0f;
    private float kD2 = kernelDistance * kernelDistance;
    private static readonly float POLY6 = 4.0f / (Mathf.PI * Mathf.Pow(kernelDistance, 8.0f));
    private static readonly float SPIKY_GRAD = 0.0f;
    private static readonly float VISC_LAP = 0.0f;

    private void Init()
    {
        particles = new Particle[amount];
        for (int i = 0; i < amount; i++)
        {
            // 3x3 matrix-like instantiation;
            float x = (i % perRow) + Random.Range(-0.1f, 0.1f);
            float y = (2 * radius) + ((i / perRow) / perRow) * 1.1f;
            float z = ((i / perRow) % perRow) + Random.Range(-0.1f, 0.1f);
            
            GameObject currentGo = Instantiate(particlePrefab);
            Particle currentParticle = currentGo.AddComponent<Particle>();
            
            particles[i] = currentParticle;
            currentGo.transform.localScale = Vector3.one * radius;
            currentGo.transform.position = new Vector3(x, y, z);
            currentParticle.go = currentGo;
            currentParticle.position = currentGo.transform.position;
        }
    }

    private void CalculateDensity()
    {
        foreach (Particle pI in particles)
        {
            pI.pressure = 0.0f;
            foreach (Particle pJ in particles)
            {
                Vector3 diff = pJ.position -pI.position;
                float dist = diff.magnitude;

                if (dist < kD2)
                {
                    pI.density += mass * POLY6 * Mathf.Pow(kD2 - dist, 3.0f);
                }
            }
            pI.pressure = gasConst * (pI.density - restDensity);
        }
    }
    

    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
