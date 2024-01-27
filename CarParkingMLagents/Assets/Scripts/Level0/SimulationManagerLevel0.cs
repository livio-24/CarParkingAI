using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class SimulationManagerLevel0 : MonoBehaviour
{
    [SerializeField] private List<ParkingLotLevel0> parkingLots;
    [SerializeField] private GameObject carPrefab;
    private CarAgent2Level0 agent;
    public BoxCollider spawnArea1;
    public BoxCollider spawnArea2;

    private void Start()
    {
        agent = GetComponent<CarAgent2Level0>();
    }


    public void InitializeSimulation()
    {
        RepositionAgentRandom();
    }
    public void RepositionAgentRandom()
    {
        if (agent != null)
        {
            agent.GetComponent<Rigidbody>().velocity = Vector3.zero;
            agent.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            agent.GetComponent<PrometeoCarController>().ThrottleOff();

            float randomX;
            float randomZ;
            float randomAngle;
            int rand = Random.Range(0, 1);
            float[] ang_near_slot = new float[] { -90f, 90f, 180f, -180f, 0f };

            if (rand == 0)
            {
                randomX = Random.Range(spawnArea1.bounds.min.x, spawnArea1.bounds.max.x);
                randomZ = Random.Range(spawnArea1.bounds.min.z, spawnArea1.bounds.max.z);
                randomAngle = 90.0f;


            }
            else
            {
                randomX = Random.Range(spawnArea2.bounds.min.x, spawnArea2.bounds.max.x);
                randomZ = Random.Range(spawnArea2.bounds.min.z, spawnArea2.bounds.max.z);
                randomAngle = ang_near_slot[Random.Range(0, 4)];

            }

            Vector3 spawnPosition = new Vector3(randomX, 0.1f, randomZ);

            //randomAngle = (Random.Range(0, 1))*180.0f;
            Vector3 rotationAngles = new Vector3(0f, randomAngle, 0f);

            // Converti il Vector3 in un Quaternion
            Quaternion rotationQuaternion = Quaternion.Euler(rotationAngles);
            agent.GetComponent<Rigidbody>().position = spawnPosition;
            agent.GetComponent<Rigidbody>().rotation = rotationQuaternion;
        }

    }

    public ParkingLotLevel0 GetRandomEmptyParkingSlot()
    {
        return parkingLots.Where(r => r.IsOccupied == false).OrderBy(r => Guid.NewGuid()).FirstOrDefault();
    }

}