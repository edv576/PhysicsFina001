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
    public static float FieldWidth;

    //inverted inertia tensor
    private Vector3 inv_InertiaTensor;
    private Random rng = new Random();

    void Start()
    {
        particles = new List<Particle>();
        FieldWidth = 2 * GameObject.Find("Floor").GetComponent<BoxCollider>().bounds.extents.x;

        for (int l = 0; l < 3; l++)
            for (int i = 0; i < ParticleCount; i++)
            {
                float x = (FieldWidth - 1) * (float)rng.NextDouble() - (FieldWidth - 1) / 2f;
                float y = 5f + 1.5f * l;
                float z = (FieldWidth - 1) * (float)rng.NextDouble() - (FieldWidth - 1) / 2f;

                //  x = 0.12f * i;

                var p = new Particle(ParticleRadius, ParticleMass, new Vector3(x, y, z));
                particles.Add(p);

                var GO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                GO.transform.localPosition = particles[i].position;

                //  GO.AddComponent<SphereCollider>();
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

    // Update is called once per frame
    void Update()
    {
        //   float dt = Time.deltaTime;
        //  float ratio = Time.deltaTime / 0.003f;

        float deltaTime = 0.003f; //paper uses 0.3 ms, then scale it to unity's scale r=0.5

        Debug.Log($"dt: {deltaTime}");
        foreach (var particle in particles)
        {
            particle.Update(particles);
            // float scale = 1f;
            //semi-implicit euler
            particle.COMVelocity += deltaTime * (particle.Force_l / particle.mass);
            particle.position += deltaTime * particle.COMVelocity;

            var inv_inertia = particle.inv_inertia;
            var inertia = new Vector3(1.0f / inv_inertia.x, 1.0f / inv_inertia.y, 1.0f / inv_inertia.z);
            GameObject p = particleGOlist[particle.Id];

            //var angVel = particle.mass * Vector3.Cross(particle.AngVelocity,
            //                 new Vector3(1.0f / inv_inertia.x, 1.0f / inv_inertia.y, 1.0f / inv_inertia.z));

            particle.AngVelocity += deltaTime * (particle.Force_t / particle.mass);
            var aV = particle.mass * Vector3.Cross(particle.AngVelocity, inertia);


            Quaternion r = p.transform.rotation;
            //  Vector3 aV = particle.AngVelocity;
            Quaternion avq = new Quaternion(aV.x, aV.y, aV.z, 1);
            float dt2 = 0.5f * deltaTime;
            r = add(r, (scale(dt2, r)) * avq * r);

            p.transform.rotation = r;

            //angVelocityQua = new Quaternion(angVelocity.x, angVelocity.y, angVelocity.z, 0);


        }

        Quaternion scale(float a, Quaternion q)
        {
            return new Quaternion(q.x * a, q.y * a, q.z * a, q.w * a);
        }

        Quaternion add(Quaternion A, Quaternion B)
        {
            return new Quaternion(A.x + B.x, A.y + B.y, A.z + B.z, A.w + B.w);

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
