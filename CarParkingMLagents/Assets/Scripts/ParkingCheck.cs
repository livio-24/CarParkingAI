using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkingCheck : MonoBehaviour
{
    Collider coll;
    // Start is called before the first frame update
    void Start()
    {
        coll = GetComponent<Collider>();    
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("agent")) 
        {
            if(coll.bounds.Contains(other.bounds.max) && coll.bounds.Contains(other.bounds.min)) 
            {
                Debug.Log("car in");
            }
            else { Debug.Log("car out"); }

        }
    }
}
