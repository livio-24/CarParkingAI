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
        
    }

    public override void OnEpisodeBegin()
    {
        //spawn random
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        
       
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        
    }



}
