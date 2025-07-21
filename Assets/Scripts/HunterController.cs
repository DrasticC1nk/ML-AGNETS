using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Integrations.Match3;
public class HunterController : Agent
{
    //Hunter Variables
    [SerializeField] private float moveSpeed = 4f;
    private Rigidbody rb;

    //Enviroment Variables
    Material envMaterial;
    public GameObject env;

    public GameObject prey;
    public Agentcontroller classObject;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        envMaterial = env.GetComponent<Renderer>().material;
    }

    public override void OnEpisodeBegin()
    {
        Vector3 spawnLocation = new Vector3(Random.Range(4.78f, -3.42f), -5.99f, Random.Range(10.02f, -1.19f));

        bool distanceGood = classObject.CheckOverlap(prey.transform.localPosition, spawnLocation, 5f);

        while(!distanceGood)
        {
            spawnLocation = new Vector3(Random.Range(4.78f, -3.42f), -5.99f, Random.Range(10.02f, -1.19f));

            distanceGood = classObject.CheckOverlap(prey.transform.localPosition, spawnLocation, 5f);
        }

        transform.localPosition = spawnLocation;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        //sensor.AddObservation(target.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveRotate = actions.ContinuousActions[0];
        float mmoveForward = actions.ContinuousActions[1];

        rb.MovePosition(transform.position + transform.forward * mmoveForward * moveSpeed * Time.deltaTime);
        transform.Rotate(0f, moveRotate * moveSpeed, 0f, Space.Self);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuosActions = actionsOut.ContinuousActions;
        continuosActions[0] = Input.GetAxisRaw("Horizontal");
        continuosActions[1] = Input.GetAxisRaw("Vertical");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Agent")
        {
            AddReward(10f);

            classObject.AddReward(-13f);
     
            envMaterial.color = Color.yellow;

            classObject.EndEpisode();
            
            EndEpisode();          
        }
        if (other.gameObject.tag == "Wall")
        {
            envMaterial.color = Color.red;
            classObject.EndEpisode();
            AddReward(-15f);
            EndEpisode();
        }
    }
}
