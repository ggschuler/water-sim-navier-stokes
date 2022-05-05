using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    private ParticleCollider[] colliders;
    [SerializeField] private List<GameObject> walls = new List<GameObject>();
    
    
    private static float kernelDistance = 16.0f;
    private float kD2 = kernelDistance * kernelDistance;
    private static readonly float POLY6 = 4.0f / (Mathf.PI * Mathf.Pow(kernelDistance, 8.0f));
    private static readonly float SPIKY_GRAD = -10.0f / (Mathf.PI * Mathf.Pow(kernelDistance, 5.0f));
    private static readonly float VISC_LAP = 40.0f / (Mathf.PI * Mathf.Pow(kernelDistance, 5.0f));
    private static readonly float TIMESTEP = 0.0007f;
    private static readonly float BOUND_DAMP = -0.5f;

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
            colliders[i] = currentGo.AddComponent<ParticleCollider>();
            currentGo.transform.localScale = Vector3.one * radius;
            currentGo.transform.position = new Vector3(x, y, z);
            currentParticle.go = currentGo;
            currentParticle.position = currentGo.transform.position;
        }
    }

    private void CalculateDensityPressure()
    {
        foreach (Particle pI in particles)
        {
            pI.density = 0.0f;
            foreach (Particle pJ in particles)
            {
                Vector3 diff = pJ.position -pI.position;
                float dist = diff.sqrMagnitude;

                if (dist < kD2)
                {
                    pI.density += mass * POLY6 * Mathf.Pow(kD2 - dist, 3.0f);
                }
            }
            pI.pressure = gasConst * (pI.density - restDensity);
        }
    }

    private void CalculateForces()
    {
        foreach (Particle pI in particles)
        {
            Vector3 pressureF = Vector3.zero;
            Vector3 viscF = Vector3.zero;
            foreach (Particle pJ in particles)
            {
                if (pI == pJ) continue;
                Vector3 diff = pJ.position - pI.position;
                float dist = diff.magnitude;

                if (dist < kernelDistance)
                {
                    pressureF += -diff.normalized * mass * (pI.pressure + pJ.pressure) / (2.0f * pJ.density) *
                                 SPIKY_GRAD * Mathf.Pow(kernelDistance - dist, 3.0f);

                    viscF += VISC_LAP * mass * (pJ.velocity - pI.velocity) / pJ.density * VISC_LAP *
                             (kernelDistance - dist);
                }
            }

            Vector3 gravF = _gravity * gravityMult * pI.density;
            pI.forces = pressureF + viscF + gravF;
        }
    }

    private void CalculateCollisions()
    {
        foreach (Particle pI in particles)
        {
            foreach (Particle pJ in particles)
            {
                Vector3 penetrationNormal = Vector3.zero;
                Vector3 penetrationPosition = Vector3.zero;
                float penetrationLength = 0.0f;
                //TODO:
                if (Collide())
                {
                    DampVelocity();
                }
            }
        }
    }
    //TODO:
    private bool Collide()
    {
        return true;
    }
    //TODO:
    private void DampVelocity()
    {
        
    }

    private void Integrate()
    {
        foreach (Particle p in particles)
        {
            p.velocity += TIMESTEP * p.forces / p.density;
            p.position += TIMESTEP * p.velocity;
            p.go.transform.position = p.position;
        }
    }


    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        CalculateDensityPressure();
        CalculateForces();
        Integrate();
    }
}
