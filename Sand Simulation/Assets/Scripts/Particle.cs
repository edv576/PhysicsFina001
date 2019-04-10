using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Particle
{
    public int Id = 0;

    private static int newID = 0;   //only call once to increment id number

    private GameObject manager;
    private GameManagerScript managerScript;

    private float radius;
    public float mass;


    public Vector3 position;
    public Quaternion orientation;

    public Vector3 COMVelocity;    //linear velocity
    private Vector3 AngVelocity;
    private Vector3 inv_inertia;

    public Vector3 Force_l;
    private Vector3 Force_t;
    // public List<Particle> Particles;

    public float Diameter => radius * 2;

    float kn = 50;
    float ys = 3;
    float u = 0.05f;
    float ks = 20;
    float alpha = 0.5f;
    float beta = 1.5f;

    public Particle(float r, float m, Vector3 pos)
    {
        Id = newID++;
        position = pos;

        radius = r;
        mass = m;
        //COMVelocity = linV;
        //AngVelocity = angV;

        float inertia = 1.0f / ((2.0f / 5.0f) * mass * radius * radius);
        inv_inertia = new Vector3(inertia, inertia, inertia);
        orientation = new Quaternion(1, 1, 1, 1);
        manager = GameObject.Find("GameManager");
        managerScript = manager.GetComponent<GameManagerScript>();
        //   Particles = manager.GetComponent<GameManagerScript>().particles;
    }

    public void Update(List<Particle> particles, bool fromCollisionCall = false)
    {




        Vector3 sumF_n = Vector3.zero;
        Vector3 sumF_t = Vector3.zero;

        Force_l = new Vector3(0, 0, 0);
        Force_t = new Vector3(0, 0, 0);


        //set constraint y = 0 as floor
        // sumF_n = GetFloorForces(kn, ys, alpha, beta, sumF_n);
        GetStaticCollisionForces(ref sumF_n, ref sumF_n, kn, ys, u, ks, alpha, beta);

        //collision with other particles
        GetParticleCollisionForces(particles, kn, ys, u, ks, alpha, beta, ref sumF_n, ref sumF_t);

        //add gravity and sum all the forces into linear force
        Vector3 gravity = new Vector3(0, -9.81f, 0) * mass;
        Force_l += gravity;
        Force_l += sumF_n;
    }
    public Vector3 CalcCollisionForce(Vector3 HitPoint, Vector3 hitNormal)
    {
        Vector3 sumF_n = Vector3.zero;

        var distance = Vector3.Distance(HitPoint, position);

        Vector3 relativeVelocity = COMVelocity;

        float E = Mathf.Max(0, radius - distance);
        float E2 = Vector3.Dot(relativeVelocity, hitNormal);

        Vector3 F_n = -(kn * Mathf.Pow(E, beta) + ys * E2 * Mathf.Pow(E, alpha)) * hitNormal;

        sumF_n += F_n;


        return sumF_n;
    }


    private void GetStaticCollisionForces(ref Vector3 sumFN, ref Vector3 vector3,
        float kn, float ys, float u, float ks, float alpha, float beta)
    {
        int mask = LayerMask.GetMask("CollisionObject");
        var coll = Physics.OverlapSphere(position, radius, mask);
        Vector3 fn = Vector3.zero;
        foreach (var collider in coll) Debug.Log($"coll with {collider.gameObject.name}");

        foreach (var collider in coll)
        {
            var hitP = collider.ClosestPointOnBounds(position);
            var contactNormal = (hitP - position).normalized;
            if (contactNormal == Vector3.zero) contactNormal = -Vector3.up;

            var distance = Vector3.Distance(hitP, position);
            //Vector3 contactNormal = new Vector3(0, -1, 0);
            Vector3 relativeVelocity = COMVelocity;

            float E = Mathf.Max(0, radius - distance);
            float E2 = Vector3.Dot(relativeVelocity, contactNormal);

            Vector3 F_n = -(kn * Mathf.Pow(E, beta) + ys * E2 * Mathf.Pow(E, alpha)) * contactNormal;

            sumFN += F_n;

        }

        Vector3 sumFN2 = Vector3.zero;
        // GetFloorForces(ref  sumFN) ;


    }

    public Vector3 GetFloorForces(ref Vector3 sumF_n)
    {
        if (position.y < radius+1)
        {
            Debug.Log("y<0!");
            var distance = Vector3.Distance(new Vector3(position.x, 0, position.z), position);
            Vector3 contactNormal = new Vector3(0, -1, 0);
            Vector3 relativeVelocity = COMVelocity;

            float E = Mathf.Max(0, radius - distance);
            float E2 = Vector3.Dot(relativeVelocity, contactNormal);

            Vector3 F_n = -(kn * Mathf.Pow(E, beta) + ys * E2 * Mathf.Pow(E, alpha)) * contactNormal;

            sumF_n += F_n;
        }

        return sumF_n;
    }

    private void GetParticleCollisionForces(List<Particle> particles, float kn, float ys, float u, float ks, float alpha, float beta, ref Vector3 sumF_n, ref Vector3 sumF_t)
    {
        foreach (var other in particles)
        {
            if (other.Id == Id) continue;

            Vector3 pos = other.position;
            float d = Vector3.Distance(position, pos);

            if (d <= 2 * radius && d > 0)
            {
                Vector3 contactNormal = Vector3.Normalize(pos - position);
                contactNormal.Normalize();

                //relative velocity
                Vector3 V = (COMVelocity - other.COMVelocity);

                //xi values from paper
                float E = Mathf.Max(radius - d, 0);
                float E2 = Vector3.Dot(V, contactNormal);

                Vector3 tV = V - E2 * contactNormal;
                Vector3 F_n = -(kn * Mathf.Pow(E, beta) + ys * E2 * Mathf.Pow(E, alpha)) * contactNormal;

                Vector3 cross = Vector3.Cross(0.5f * contactNormal, tV).normalized;
                Vector3 F_t = -Mathf.Min(u * F_n.magnitude, ks * tV.magnitude) * cross;

                sumF_n += F_n;
                sumF_t += F_t;

            }
        }
    }


}
