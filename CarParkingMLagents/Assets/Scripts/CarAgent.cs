using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class CarAgent : Agent
{
    private PrometeoCarController carController;

    // Start is called before the first frame update
    void Start()
    {
        carController = GetComponent<PrometeoCarController>();
    }

    public override void OnEpisodeBegin()
    {
        //spawn random
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(carController.carSpeed);
       
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        carController.move(actions.ContinuousActions[0], actions.ContinuousActions[1]);

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Vertical");
        continuousActionsOut[1] = Input.GetAxis("Horizontal");
    }



}
