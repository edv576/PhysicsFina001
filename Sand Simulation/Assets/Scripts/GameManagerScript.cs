using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using Random = System.Random;

public class GameManagerScript : MonoBehaviour
{
    public List<Particle> particles;

    public List<GameObject> particleGOlist;

    // Start is called before the first frame update
    public float ParticleMass = 0.15f;
    public float ParticleRadius = 0.5f;

    public int ParticleCount = 10;

    //inverted inertia tensor
    private Vector3 inv_InertiaTensor;
    private Random rng = new Random();

    void Start()
    {
        particles = new List<Particle>();

        for (int i = 0; i < ParticleCount; i++)
        {
            float x = 9 * (float)rng.NextDouble() - 4.5f;
            float y = 9 * i;
            float z = 0;

            x = 0.12f * i;

            var p = new Particle(ParticleRadius, ParticleMass, new Vector3(x, y, z));
            particles.Add(p);

            var GO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GO.transform.localPosition = particles[i].position;

            GO.AddComponent<SphereCollider>();
            GO.GetComponent<SphereCollider>().isTrigger = true;

            //Rigidbody rbody = new Rigidbody { isKinematic = false, useGravity = false };

            // GO.AddComponent<Rigidbody>();
            // GO.GetComponent<Rigidbody>().isKinematic = false;
            // GO.GetComponent<Rigidbody>().useGravity = false;

            // GO.GetComponent<SphereCollider>().isTrigger = true;
            GO.name = "Sphere " + p.Id;

            particleGOlist.Add(GO);


        }
    }

    void Init()
    {

    }


    // Update is called once per frame
    void Update()
    {
        float dt = Time.deltaTime;

        foreach (var particle in particles)
        {
            particle.Update(particles);

            //semi-implicit euler
            particle.COMVelocity += dt * (particle.Force_l / particle.mass);
            particle.position += dt * particle.COMVelocity;

        }

        for (int i = 0; i < particles.Count; i++)
        {
            try
            {
                particleGOlist[i].transform.localPosition = particles[i].position;
                if (particles[i].position.y < 0)
                {
                    //particles[i].position.y = 0.5f;
                    // particleGOlist[i].transform.localPosition = particles[i].position;
                }
            }
            catch (Exception e)
            {
                Debug.Log("tset");
                int asdas = 1;
            }
        }
    }
}
