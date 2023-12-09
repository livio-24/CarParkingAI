using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine.UI;
using TMPro;

public class CarAgent : Agent
{
    private PrometeoCarController carController;
    private WheelCollider[] colliders;
    Collider coll;
    Transform carTransform;
    Rigidbody rBody;
    private float timer = 0.3f;
    public BoxCollider spawnArea1;
    public BoxCollider spawnArea2;
    public Transform parkingEntrance;
    private bool parked;
    private bool inParking;
    public float parkingDistanceTreshold;


    // Start is called before the first frame update
    void Start()
    {
        carController = GetComponent<PrometeoCarController>();
        colliders = new WheelCollider[] { carController.frontRightCollider, carController.frontLeftCollider, carController.rearLeftCollider, carController.rearRightCollider };
        coll = GetComponentInChildren<MeshCollider>();
        carTransform = GetComponent<Transform>();
        rBody = GetComponentInChildren<Rigidbody>();
        //inParking = true;
    }
    void SpawnObject()
    {
        float randomX;
        float randomZ;

        // Genera posizioni casuali nelle due zone ortogonali
        //if (Random.Range(0, 2) == 0)
        
        randomX = Random.Range(spawnArea1.bounds.min.x, spawnArea1.bounds.max.x);
        randomZ = Random.Range(spawnArea1.bounds.min.z, spawnArea1.bounds.max.z);
        
        /*
        else
        {
            randomX = Random.Range(spawnArea2.bounds.min.x, spawnArea2.bounds.max.x);
            randomZ = Random.Range(spawnArea2.bounds.min.z, spawnArea2.bounds.max.z);
        }*/

        Vector3 spawnPosition = new Vector3(randomX, 0.1f, randomZ);
        Vector3 rotationAngles = new Vector3(0f, Random.Range(0,360), 0f); // Modifica questi valori a seconda delle tue esigenze

        // Converti il Vector3 in un Quaternion
        Quaternion rotationQuaternion = Quaternion.Euler(rotationAngles);
        rBody.position = spawnPosition;
        rBody.rotation = rotationQuaternion;
 
    }

    public override void OnEpisodeBegin()
    {
        SpawnObject();
        //parked = false;
        rBody.velocity = Vector3.zero;
        carController.ThrottleOff();

        //spawn random
        // Set the initial position and rotation of the car
        //carTransform.position = new Vector3(0f, 0.09445577f, 0f); // Adjust the position as needed
        // Specifica gli angoli di rotazione attorno agli assi x, y e z
        //Vector3 rotationAngles = new Vector3(0f, -90f, 0f); // Modifica questi valori a seconda delle tue esigenze

        // Converti il Vector3 in un Quaternion
        //Quaternion rotationQuaternion = Quaternion.Euler(rotationAngles);
        //carTransform.rotation = rotationQuaternion; // Set the rotation to the identity (no rotation)
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(carController.carSpeed);
        sensor.AddObservation(carTransform.localPosition);
       
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        carController.move(actions.ContinuousActions[0], actions.ContinuousActions[1]);
        CheckGroundHit();
        checkParkingDistance();
        AddReward(-1.0f / MaxStep);
        //Debug.Log("Cumulative reward: " + GetCumulativeReward());
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Vertical");
        continuousActionsOut[1] = Input.GetAxis("Horizontal");
    }

    private void OnCollisionEnter(Collision collision)
    {
       
        if (collision.gameObject.CompareTag("obstacle"))
        {
            AddReward(-1.0f);
            EndEpisode();
            //Debug.Log(GetCumulativeReward());
        }

    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("obstacle"))
        {
            AddReward(-0.1f);
            //Debug.Log(GetCumulativeReward());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        /*if (other.gameObject.CompareTag("target"))
        {
            AddReward(0.5f);
            //Debug.Log(GetCumulativeReward());
        }
        */
        if(other.gameObject.CompareTag("parking_floor"))
        {
            inParking = true;
            //Debug.Log(inParking);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("target") && (other.bounds.Contains(coll.bounds.max) && other.bounds.Contains(coll.bounds.min)))
        {
            //AddReward(1.0f);
            //parked = true;
            //Debug.Log(GetCumulativeReward());
            //EndEpisode();

            // Decrementa il timer
            
            timer -= Time.deltaTime;
            if(timer <= 0.0f)
            {
                timer = 0.3f;
                AddReward(1.0f);
                parked = true;
                //Debug.Log(GetCumulativeReward());
                EndEpisode();
            }
            
            //Debug.Log("car in");
            
        //else { Debug.Log("car out"); }

        }
    }

    private void OnTriggerExit(Collider other)
    {  /*
        if (other.gameObject.CompareTag("target") && !parked)
        {
            AddReward(-0.5f);
            //Debug.Log(GetCumulativeReward());
            timer = 2.0f;
         }
       */ 

        if (other.gameObject.CompareTag("target") && parked)
        {
            parked = false;
        }

        if (other.gameObject.CompareTag("parking_floor"))
        {
            inParking = false;
            //Debug.Log(inParking);
        }
    }

    private void CheckGroundHit()
    {
        foreach (WheelCollider collider in colliders)
        {
            if (collider.GetGroundHit(out WheelHit hit))
            {
                if (hit.collider.gameObject.CompareTag("floor_obstacle"))
                {
                    AddReward(-0.1f);
                    //Debug.Log(GetCumulativeReward());
                    return;
                }

            }
        }
    }

    private void checkParkingDistance()
    {
        float distance = Vector3.Distance(carTransform.position, parkingEntrance.position);
        if (!inParking)
        {
            AddReward(-0.0001f * distance);
        }
        //Debug.Log(GetCumulativeReward());
        if(distance > parkingDistanceTreshold)
        {
            EndEpisode();
        }
        //Debug.Log(GetCumulativeReward());
    }




}
