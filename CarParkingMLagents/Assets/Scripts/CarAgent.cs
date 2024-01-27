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
using UnityEditor.VersionControl;
using System.Text;
using Unity.Barracuda;

//TODO: aggiungere reward in base alla distanza del raggio centrale dal marciapiede
public class CarAgent : Agent
{
    private PrometeoCarController carController;
    Transform carTransform;
    Rigidbody rBody;
    //private float timer = 0.3f;
    public BoxCollider spawnArea1;
    public BoxCollider spawnArea2;
    public BoxCollider spawnArea3;
    private RayPerceptionSensorComponent3D rayPerceptionSensor;
    private bool onLines;
    private float bestDistance;
    private float distanzaAttuale;

    // Start is called before the first frame update
    void Start()
    {
        //Time.timeScale = 2.0f;
        carController = GetComponent<PrometeoCarController>();
        //colliders = new WheelCollider[] { carController.frontRightCollider, carController.frontLeftCollider, carController.rearLeftCollider, carController.rearRightCollider };
        //coll = GetComponentInChildren<MeshCollider>();
        carTransform = GetComponent<Transform>();
        rBody = GetComponent<Rigidbody>();
        rayPerceptionSensor = GetComponent<RayPerceptionSensorComponent3D>();
        bestDistance = 100.0f;
        //inParking = true;
    }
    void SpawnObject()
    {
        float randomX;
        float randomZ;
        float randomAngle;
        int rand = Random.Range(0, 2);
        float[] ang_spawn1 = new float[] { 0f, 45f, 90f, 135f, 180f };
        float[] ang_spawn2 = new float[] { 0f, 45f, -45f, -90f, 90f, 135f, -135f, 180f, -180f };
        float[] ang_spawn3 = new float[] { 0f, 45f, 90f, 135f, 180f };


        if (rand == 0)
        {
            randomX = Random.Range(spawnArea1.bounds.min.x, spawnArea1.bounds.max.x);
            randomZ = Random.Range(spawnArea1.bounds.min.z, spawnArea1.bounds.max.z);
            randomAngle = ang_spawn1[Random.Range(0, 5)];


        }
        else if (rand == 1)
        {
            randomX = Random.Range(spawnArea2.bounds.min.x, spawnArea2.bounds.max.x);
            randomZ = Random.Range(spawnArea2.bounds.min.z, spawnArea2.bounds.max.z);
            randomAngle = ang_spawn2[Random.Range(0, 8)];

        }
        else
        {
            randomX = Random.Range(spawnArea3.bounds.min.x, spawnArea3.bounds.max.x);
            randomZ = Random.Range(spawnArea3.bounds.min.z, spawnArea3.bounds.max.z);
            randomAngle = ang_spawn3[Random.Range(0, 5)];
        }

        /*
        else
        {
            randomX = Random.Range(spawnArea2.bounds.min.x, spawnArea2.bounds.max.x);
            randomZ = Random.Range(spawnArea2.bounds.min.z, spawnArea2.bounds.max.z);
        }*/


        Vector3 spawnPosition = new Vector3(randomX, 0.1f, randomZ);

        //randomAngle = (Random.Range(0, 1))*180.0f;
        Vector3 rotationAngles = new Vector3(0f, randomAngle, 0f); 

        // Converti il Vector3 in un Quaternion
        Quaternion rotationQuaternion = Quaternion.Euler(rotationAngles);
        rBody.position = spawnPosition;
        rBody.rotation = rotationQuaternion;
 
    }

    public override void OnEpisodeBegin()
    {
        SpawnObject();
        bestDistance = 100.0f;
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
        
        // Valori minimo e massimo dell'intervallo
        float minXValue = -10.50f;
        float maxXValue = 10.50f;

        float minZValue = -6.50f;
        float maxZValue = 6.50f;


        // Normalizzazione del valore tra -1 e 1
        float normalizedXValue = (2 * (carTransform.localPosition.x - minXValue) / (maxXValue - minXValue)) - 1;
        float normalizedZValue = (2 * (carTransform.localPosition.z - minZValue) / (maxZValue - minZValue)) - 1;
        float normalizedCarSpeed = (2 * (carController.carSpeed - (-carController.maxReverseSpeed)) / ((carController.maxSpeed + 5.0f) - (-carController.maxReverseSpeed))) - 1;
        Vector3 normalizedRotation = carTransform.rotation.eulerAngles / 180.0f - Vector3.one;  // [-1,1]
        
        sensor.AddObservation(normalizedCarSpeed);
        sensor.AddObservation(normalizedXValue);
        sensor.AddObservation(normalizedZValue);
        sensor.AddObservation(carTransform.localPosition.y);
        sensor.AddObservation(normalizedRotation);
        //Debug.Log(normalizedRotation);

        //Debug.Log(string.Format("X: {0}, Y: {1}, Z: {2}", normalizedXValue, carTransform.localPosition.y, normalizedZValue));

        //maxZ = 6.50, minZ = -6.50, maxX=10.50, minX=-4.50
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        carController.move(actions.ContinuousActions[0], actions.ContinuousActions[1]);
        //Debug.Log(carTransform.localPosition);

        //CheckGroundHit();
        //checkParkingDistance();
        AddReward(-1.0f / MaxStep);
        //Debug.Log("Cumulative reward: " + GetCumulativeReward());
        //checkSidewalkDistance();
        checkTargetDistance();
        //Debug.Log(string.Format("Cumulative Reward: {0}", GetCumulativeReward()));

        //TODO: aggiungere penalita quando la distanza dal marciapiede di uno qualsiasi dei raggi è inferiore a una certa soglia, e quando l'auto si trova nel trigger delle linee
        // aggiungere reward +1 quando l'auto è a una certa distanza dal marciapiede target e non è sulle linee (end episode)
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Vertical");
        continuousActionsOut[1] = Input.GetAxis("Horizontal");
    }

    /*
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
    */

    private void checkTargetDistance()
    {
        RayPerceptionInput spec = rayPerceptionSensor.GetRayPerceptionInput();
        RayPerceptionOutput obs = RayPerceptionSensor.Perceive(spec);

        //output del raggio centrale anteriore
        RayPerceptionOutput.RayOutput anteriorCentralRayOutput = obs.RayOutputs[0];

        if (anteriorCentralRayOutput.HitTagIndex.Equals(0))
        {

            //distanzaPrecedente = distanzaAttuale;
            distanzaAttuale = anteriorCentralRayOutput.HitFraction;
           
            //Debug.Log(anteriorCentralRayOutput.HitFraction);
            if (distanzaAttuale < bestDistance)
            {
                setBestDistance(distanzaAttuale);

                //Minore è la distanza dal muro del parcheggio, maggiore il reward
                
                AddReward(0.01f);
                
                //Debug.Log(string.Format("Best distance: {0}, Cumulative reward: {1}", bestDistance, GetCumulativeReward()));
                if(distanzaAttuale < 0.12f && !onLines)
                {
                    AddReward(1.0f);
                    EndEpisode();
                }
                else if(distanzaAttuale < 0.12f && onLines)
                {
                    AddReward(-0.1f);
                    EndEpisode();
                }

            }
            else
            {
                AddReward(-0.003f);
            }
    
        }

    }

    private void setBestDistance(float d)
    {
        bestDistance = d;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("wall") || collision.gameObject.CompareTag("wall_target") || collision.gameObject.CompareTag("car"))
        {
            AddReward(-0.1f);
        }
    }

    
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("wall") || collision.gameObject.CompareTag("wall_target") || collision.gameObject.CompareTag("car"))
        {
            AddReward(-0.005f);
        }


    }
    

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("lines"))
        {
            onLines = true;
            AddReward(-0.1f);
            //Debug.Log(inParking);
        }
    }

    
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("lines"))
        {
            onLines = true;
            AddReward(-0.005f);
            //Debug.Log(inParking);
        }
    }
    

    private void OnTriggerExit(Collider other)
    {

        if (other.gameObject.CompareTag("lines"))
        {
            onLines = false;
        }

        if (other.gameObject.CompareTag("parking_floor"))
        {
            AddReward(-1.0f);
            EndEpisode();
            //Debug.Log(inParking);
        }
    }




    /*
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("parking_floor"))
        {
            inParking = true;
            //Debug.Log(inParking);
        }
     }
    /*

    /*
    

     
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
    {  
        if (other.gameObject.CompareTag("target") && !parked)
        {
            AddReward(-0.5f);
            //Debug.Log(GetCumulativeReward());
            timer = 2.0f;
         }
        

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
    */

    /*
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
    */




}
