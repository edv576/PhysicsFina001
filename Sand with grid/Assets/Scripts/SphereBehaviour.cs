using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereBehaviour : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 scaleMovement = new Vector3(0.2f, 0.2f, 0.2f);

        if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            transform.position += new Vector3(Vector3.up.x * scaleMovement.x, Vector3.up.y * scaleMovement.y, Vector3.up.z * scaleMovement.z);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            transform.position += new Vector3(Vector3.down.x * scaleMovement.x, Vector3.down.y * scaleMovement.y, Vector3.down.z * scaleMovement.z);
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            transform.position += new Vector3(Vector3.left.x * scaleMovement.x, Vector3.left.y * scaleMovement.y, Vector3.left.z * scaleMovement.z);
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            transform.position += new Vector3(Vector3.right.x * scaleMovement.x, Vector3.right.y * scaleMovement.y, Vector3.right.z * scaleMovement.z);
        }
    }
}
