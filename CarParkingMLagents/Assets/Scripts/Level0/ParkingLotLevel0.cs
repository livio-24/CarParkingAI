using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkingLotLevel0 : MonoBehaviour
{
    public bool IsOccupied { get; set; }
    private Collider fullEndCollider;
    public GameObject street_lot;

    private void Awake()
    {
        fullEndCollider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("agent"))
        {
            if (fullEndCollider.bounds.Intersects(other.bounds))
            {
                float bonusfactor = 0.8f;
                //Allineamento dell'auto con il collider dello slot auto
                float alignment = Vector3.Dot(gameObject.transform.forward,
                                        other.gameObject.transform.forward);
                //Debug.Log(alignment);
                //if (alignment > 0)
                    //bonusfactor = 0.8f;
                float bonus = bonusfactor * Mathf.Abs(alignment);
                StartCoroutine(other.gameObject.transform.parent.parent.GetComponent<CarAgent2Level0>().ParkingReward(bonus));

            }
            
        }
    }

}