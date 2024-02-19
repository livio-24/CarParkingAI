using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class SimulationManagerLevel2 : MonoBehaviour
{
    [SerializeField] private List<ParkingLotLevel2> parkingLots;
    [SerializeField] private GameObject carPrefab;
    private CarAgent2Level2 agent;
    private List<GameObject> parkedCars;
    public BoxCollider spawnArea1;
    public BoxCollider spawnArea2;

    public bool initComplete;
    private void Start()
    {
        agent = GetComponent<CarAgent2Level2>();
        parkedCars = new List<GameObject>();

    }


    public void InitializeSimulation()
    {
        initComplete = false;
        StartCoroutine(OccupyParkingSlotsWithRandomCars());
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
            int rand = Random.Range(0, 3);
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

    
    public void ResetSimulation()
    {

        foreach (GameObject parkedCar in parkedCars)
        {
            Destroy(parkedCar);
        }


        foreach (ParkingLotLevel2 parkingLot in parkingLots)
        {
            parkingLot.IsOccupied = false;
        }
        parkedCars.Clear();
    }
    

    
    public IEnumerator OccupyParkingSlotsWithRandomCars()
    {
        foreach (ParkingLotLevel2 parkingLot in parkingLots)
        {
            parkingLot.IsOccupied = false;
        }
        yield return new WaitForSeconds(0.1f);

        int total = Random.Range(7, 16);
        //int total = 0;

        for (int i = 0; i < total; i++)
        {
            ParkingLotLevel2 lot = parkingLots.Where(r => r.IsOccupied == false).OrderBy(r => Guid.NewGuid()).FirstOrDefault();
            if (lot != null)
            {
                GameObject carInstance = Instantiate(carPrefab);
                //carInstance.tag = "car";
                //carInstance.layer = 7;
                if (lot.transform.position.z > 0)
                    carInstance.transform.position = new Vector3(lot.transform.position.x, 0f, lot.transform.position.z - 5f);
                else
                    carInstance.transform.position = new Vector3(lot.transform.position.x, 0f, lot.transform.position.z);

                parkedCars.Add(carInstance);
                lot.IsOccupied = true;
                if (parkedCars.Count >= total)
                    break;
            }
        }

        initComplete = true;
    }

    public ParkingLotLevel2 GetRandomEmptyParkingSlot()
    {
        return parkingLots.Where(r => r.IsOccupied == false).OrderBy(r => Guid.NewGuid()).FirstOrDefault();
    }

    public ParkingLotLevel2 GetNearestEmptyParkingSlot()
    {
        return parkingLots.Where(r => r.IsOccupied == false).FirstOrDefault();
    }

    // Metodo per trovare il parcheggio libero più vicino
    public ParkingLotLevel2 GetNearestEmptyParkingSlot2()
    {
        //Debug.Log(emptyParkingLots);
        return parkingLots.Where(lot => !lot.IsOccupied).OrderBy(lot => Vector3.Distance(agent.transform.position, lot.transform.position)).FirstOrDefault();
        
    }

}