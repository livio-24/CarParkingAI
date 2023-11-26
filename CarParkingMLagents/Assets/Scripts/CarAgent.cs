using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Threading;

public class CarAgent : Agent
{
    private PrometeoCarController carController;
    private WheelCollider[] colliders;
    Collider coll;
    Transform carTransform;
    Rigidbody rBody;
    private float timer = 2f;

    // Start is called before the first frame update
    void Start()
    {
        carController = GetComponent<PrometeoCarController>();
        colliders = new WheelCollider[] { carController.frontRightCollider, carController.frontLeftCollider, carController.rearLeftCollider, carController.rearRightCollider };
        coll = GetComponentInChildren<MeshCollider>();
        carTransform = GetComponent<Transform>();
        rBody = GetComponentInChildren<Rigidbody>();
    }
    
    public override void OnEpisodeBegin()
    {
        rBody.velocity = Vector3.zero;
        carController.ThrottleOff();

        //spawn random
        // Set the initial position and rotation of the car
        carTransform.position = new Vector3(0f, 0.09445577f, 0f); // Adjust the position as needed
        // Specifica gli angoli di rotazione attorno agli assi x, y e z
        Vector3 rotationAngles = new Vector3(0f, -90f, 0f); // Modifica questi valori a seconda delle tue esigenze

        // Converti il Vector3 in un Quaternion
        Quaternion rotationQuaternion = Quaternion.Euler(rotationAngles);
        carTransform.rotation = rotationQuaternion; // Set the rotation to the identity (no rotation)
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(carController.carSpeed);
       
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        carController.move(actions.ContinuousActions[0], actions.ContinuousActions[1]);
        CheckGroundHit();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Vertical");
        continuousActionsOut[1] = Input.GetAxis("Horizontal");
    }

    private void OnCollisionEnter(Collision collision)
    {
       
        if (collision.gameObject.layer == 3)
        {
            AddReward(-1.0f);
            Debug.Log(GetCumulativeReward());
        }

    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer == 3)
        {
            AddReward(-0.2f);
            Debug.Log(GetCumulativeReward());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "target")
        {
            AddReward(0.5f);
            Debug.Log(GetCumulativeReward());
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("target"))
        {
            if (other.bounds.Contains(coll.bounds.max) && other.bounds.Contains(coll.bounds.min))
            {
                // Decrementa il timer
                timer -= Time.deltaTime;
                if(timer <= 0.0f)
                {
                    timer = 2.0f;
                    AddReward(1.0f);
                    EndEpisode();
                }
                Debug.Log("car in");
            }
            else { Debug.Log("car out"); }

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("target"))
        {
            AddReward(-0.5f);
            timer = 2.0f;
        }
    }

    private void CheckGroundHit()
    {
        foreach (WheelCollider collider in colliders)
        {
            if (collider.GetGroundHit(out WheelHit hit))
            {
                if (hit.collider.gameObject.layer == 3)
                {
                    AddReward(-1f);
                    Debug.Log(GetCumulativeReward());
                    return;
                }

            }
        }
    }


}
