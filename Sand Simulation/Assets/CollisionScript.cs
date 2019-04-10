using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CollisionScript : MonoBehaviour
{
    private GameManagerScript Manager;
    // Start is called before the first frame update
    void Start()
    {
        Manager = GameObject.Find("GameManager").GetComponent<GameManagerScript>();
    }

    // Update is called once per frame
    void Update()
    {

    }



    void OnCollisionStay(Collision collision)
    {
        return;
        Debug.Log($"collision with {transform.name}");

        string name = collision.gameObject.name;
        string[] abc = name.Split();
        if (abc.Length < 2) return;

        int id = int.Parse(abc.Last());

        foreach (ContactPoint contact in collision.contacts)
        {
            Particle p = Manager.particles[id];
            Debug.DrawRay(contact.point, contact.normal, Color.red);
            Vector3 force = p.CalcCollisionForce(contact.point, contact.normal);
            Vector3 a = Vector3.zero;
            Vector3 f = p.GetFloorForces(ref a);
            float mass = Manager.particles[id].mass;
            //if (force.magnitude > 0.1)
            Manager.particles[id].COMVelocity += Time.deltaTime * f / mass;

            //  Manager.particles[id].Update(Manager.particles,true);




        }


    }
}
