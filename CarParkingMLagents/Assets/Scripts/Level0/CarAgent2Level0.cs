using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEditor;
using UnityEngine;

public class CarAgent2Level0 : Agent
{
    private Rigidbody rb;
    private PrometeoCarController controller;
    private SimulationManagerLevel0 simulationManager;
    private ParkingLotLevel0 nearestLot;
    public Material lot_material;
    // The length of the arrow, in meters

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        controller = GetComponent<PrometeoCarController>();
        simulationManager = GetComponent<SimulationManagerLevel0>();
        simulationManager.InitializeSimulation();
    }

    public override void OnEpisodeBegin()
    {
        simulationManager.InitializeSimulation();
        //RemoveMaterial();
        nearestLot = null;
    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Vertical");
        continuousActionsOut[1] = Input.GetAxis("Horizontal");
    }


    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("wall") || other.gameObject.CompareTag("car"))
        {
            AddReward(-0.1f);
            EndEpisode();
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        controller.move(actions.ContinuousActions[0], actions.ContinuousActions[1]);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (nearestLot == null)
        {
            nearestLot = simulationManager.GetRandomEmptyParkingSlot();
            //AddMaterial();
        }

        Vector3 dirToTarget = (nearestLot.transform.position - transform.position).normalized;
        sensor.AddObservation(transform.position.normalized);
        sensor.AddObservation(transform.InverseTransformPoint(nearestLot.transform.position));
        sensor.AddObservation(transform.InverseTransformVector(rb.velocity.normalized));
        sensor.AddObservation(transform.InverseTransformDirection(dirToTarget));
        sensor.AddObservation(transform.forward);
        sensor.AddObservation(transform.right);
        float velocityAlignment = Vector3.Dot(dirToTarget, rb.velocity.normalized);
        //Debug.Log(velocityAlignment);
        AddReward(0.001f * velocityAlignment);
        //Debug.Log(GetCumulativeReward());

    }

    /*
    public void AddMaterial()
    {
        
        Renderer renderer = nearestLot.street_lot.GetComponent<Renderer>();

        if (renderer != null)
        {
            Material[] materialiEsistenti = renderer.materials;

            // Controlla se l'oggetto ha un solo materiale
            if (materialiEsistenti.Length == 1)
            {
                // Crea una nuova lista di materiali
                Material[] nuoviMateriali = new Material[2];
                nuoviMateriali[0] = materialiEsistenti[0];
                nuoviMateriali[1] = lot_material;

                // Assegna la nuova lista di materiali all'oggetto
                renderer.materials = nuoviMateriali;
            }
        }
    }

    public void RemoveMaterial()
    {
        if(nearestLot != null)
        {
            Renderer renderer = nearestLot.street_lot.GetComponent<Renderer>();

            if (renderer != null)
            {
                Material[] materialiEsistenti = renderer.materials;
                List<Material> m = new List<Material>(materialiEsistenti);

                // Controlla se l'oggetto ha un solo materiale
                if (materialiEsistenti.Length == 2)
                {
                    // Crea una nuova lista di materiali
                    m.RemoveAt(1);
                    Material[] new_m = m.ToArray();
                    renderer.materials = new_m;

                }
            }

        }
    }
    */

    public IEnumerator ParkingReward(float bonus)
    {
        //Debug.Log("COLLIDER HIT");
        AddReward(0.2f + bonus);
        yield return new WaitForEndOfFrame();
        //Debug.Log(GetCumulativeReward());
        EndEpisode();
    }

    
}