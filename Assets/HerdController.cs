using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HerdController : MonoBehaviour
{
    public float entranceSpeed = 2;
    public float entranceTime = 3;

    private float entranceCounter = 0;

    void FixedUpdate()
    {
        if(entranceCounter < entranceTime)
        {
            transform.position += Vector3.down * entranceSpeed * Time.fixedDeltaTime;
            entranceCounter += Time.deltaTime;
        }
        else
        {
            foreach (Transform child in transform)
            {
                child.parent = null;
                child.GetComponent<EnemyController>().isActivated = true;
            }

            Destroy(gameObject, .5f);
        }
    }
}
