using System;
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
    public Vector3 AngVelocity;
    public Vector3 inv_inertia;

    public Vector3 Force_l;
    public Vector3 Force_t;
    // public List<Particle> Particles;

    public float Diameter => radius * 2;
    public float Ratio => Time.deltaTime / 0.003f;

    // private float kn => OGkn * Ratio;
    // private float ys => OGys * Ratio;
    // private float u => OGu * Ratio;
    // private float ks => OGks * Ratio;

    public Vector3Int GetVoxelIdx()
    {
        float maxx = managerScript.ParticleGrid.GetLength(0);
        float maxy = managerScript.ParticleGrid.GetLength(1);
        float maxz = managerScript.ParticleGrid.GetLength(2);

        float x = position.x + maxx / 4;
        float y = position.y + maxy / 4;
        float z = position.z + maxz / 4;

        if (x < 0) x = 0;
        if (y < 0) y = 0;
        if (z < 0) z = 0;

        if (x > maxx) x = maxx - 1;
        if (y > maxy) y = maxy - 1;
        if (z > maxz) z = maxz - 1;

        int xi = (int)(x /*/ radius * 2*/);
        int yi = (int)(y /*/ radius * 2*/);
        int zi = (int)(z /*/ radius * 2*/);
        return new Vector3Int(xi, yi, zi);
    }

    float kn = 500f;
    float ys = 10;
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

        float ratio = Time.deltaTime / 0.003f;


        Vector3 sumF_n = Vector3.zero;
        Vector3 sumF_t = Vector3.zero;

        Force_l = new Vector3(0, 0, 0);
        Force_t = new Vector3(0, 0, 0);


        //set constraint y = 0 as floor
        // sumF_n = GetFloorForces(kn, ys, alpha, beta, sumF_n);
        GetStaticCollisionForces(ref sumF_n, ref sumF_n, kn, ys, u, ks, alpha, beta);

        //collision with other particles
        GetParticleCollisionForces(particles, ref sumF_n, ref sumF_t);

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
        // foreach (var collider in coll) 

        foreach (var collider in coll)
        {
            Debug.Log($"coll with {collider.gameObject.name}");
            var hitP = collider.ClosestPointOnBounds(position);

            var contactNormal = (hitP - position).normalized;
            // if (hitP == position)

            contactNormal = (collider.transform.position - position).normalized;

            //  if (contactNormal == Vector3.zero) contactNormal = -Vector3.up;

            var distance = Vector3.Distance(hitP, position);
            //Vector3 contactNormal = new Vector3(0, -1, 0);
            Vector3 relativeVelocity = COMVelocity;

            float E = Mathf.Max(0, radius - distance);
            float E2 = Vector3.Dot(relativeVelocity, contactNormal);

            Vector3 F_n = -(kn * Mathf.Pow(E, beta) + ys * E2 * Mathf.Pow(E, alpha)) * contactNormal;

            sumFN += F_n;

        }



        GetWallForces(ref sumFN);
        GetFloorForces(ref sumFN);
        //if (position.y < radius) position.y = radius;

    }

    public void GetWallForceX(ref Vector3 sumF_n)
    {
        float L = GameManagerScript.FieldWidth / 2f;
        float left = position.x < 0 ? -1 : 1;
        float wallX = left * L;
        float distance = Mathf.Abs(position.x - wallX);

        if (!(distance < radius)) return;
        Debug.Log("WallX!");

        Vector3 contactNormal = new Vector3(left * 1, 0, 0);
        Vector3 relativeVelocity = COMVelocity;

        float E = Mathf.Max(0, radius - distance);
        float E2 = Vector3.Dot(relativeVelocity, contactNormal);

        Vector3 F_n = -(kn * Mathf.Pow(E, beta) + ys * E2 * Mathf.Pow(E, alpha)) * contactNormal;

        sumF_n += F_n;
    }

    public void GetWallForceZ(ref Vector3 sumF_n)
    {
        float L = GameManagerScript.FieldWidth / 2f;

        float deep = position.z < 0 ? -1 : 1;
        float wallX = deep * L;
        float distance = Mathf.Abs(position.z - wallX);

        if (!(distance < radius)) return;
        Debug.Log("WallZ!");

        Vector3 contactNormal = new Vector3(0, 0, deep * 1);
        Vector3 relativeVelocity = COMVelocity;

        float E = Mathf.Max(0, radius - distance);
        float E2 = Vector3.Dot(relativeVelocity, contactNormal);

        Vector3 F_n = -(kn * Mathf.Pow(E, beta) + ys * E2 * Mathf.Pow(E, alpha)) * contactNormal;

        sumF_n += F_n;

    }

    public void GetWallForces(ref Vector3 sumF_n)
    {
        GetWallForceX(ref sumF_n);
        GetWallForceZ(ref sumF_n);



    }


    public Vector3 GetFloorForces(ref Vector3 sumF_n)
    {
        if (position.y <= radius)
        {
            Debug.Log("y<0!");
            var distance = Vector3.Distance(new Vector3(position.x, 0, position.z), position);
            Vector3 contactNormal = new Vector3(0, -1, 0);
            Vector3 relativeVelocity = COMVelocity;

            //  float E = Mathf.Max(0, radius - distance);
            float E = 0.5f - distance;
            float E2 = Vector3.Dot(relativeVelocity, contactNormal);
            float ys2 = 10;

            Vector3 F_n1 = -(kn * Mathf.Pow(E, beta) + ys2 * E2 * Mathf.Pow(E, alpha)) * contactNormal;

            sumF_n += F_n1;
        }

        return sumF_n;
    }

    private void GetParticleCollisionForces(List<Particle> particles, ref Vector3 sumF_n, ref Vector3 sumF_t)
    {
        var nearbyParticles = new List<int>();

        Vector3Int myIdx = GetVoxelIdx();
        int maxX = managerScript.ParticleGrid.GetLength(0);
        int maxY = managerScript.ParticleGrid.GetLength(1);
        int maxZ = managerScript.ParticleGrid.GetLength(2);

        int startX = Math.Max(0, myIdx.x - 1);
        int endX = Math.Min(maxX, myIdx.x + 1);

        int startY = Math.Max(0, myIdx.y - 1);
        int endY = Math.Min(maxY, myIdx.y + 1);

        int startZ = Math.Max(0, myIdx.z - 1);
        int endZ = Math.Min(maxZ, myIdx.z + 1);

        for (int x = startX; x < endX; x++)
            for (int y = startY; y < endY; y++)
                for (int z = startZ; z < endZ; z++)
                {
                    HashSet<int> nb = managerScript.ParticleGrid[x, y, z];
                    foreach (var nbId in nb)
                    {
                        nearbyParticles.Add(nbId);
                    }
                }





        foreach (int idx in nearbyParticles)
        {
            Particle other = particles[idx];

            if (other.Id == Id) continue;

            Vector3 pos = other.position;
            float d = Vector3.Distance(position, pos);

            if (d <= radius && d > 0)
            {
                Vector3 contactNormal = Vector3.Normalize(pos - position);
                contactNormal.Normalize();

                //relative velocity
                Vector3 V = (COMVelocity - other.COMVelocity).normalized;

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
