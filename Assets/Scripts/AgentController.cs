using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Integrations.Match3;

public class Agentcontroller : Agent
{
    //Pellet Varaibles
    [SerializeField] private Transform target;
    public int pelletCount;
    public GameObject food;
    [SerializeField] private List<GameObject> spawnedPelletList = new List<GameObject>();

    //Agent Variables
    [SerializeField] private float moveSpeed = 4f;
    private Rigidbody rb;

    //Enviroment Variables
    [SerializeField] private Transform enviromentLocation;
    Material envMaterial;
    public GameObject env;

    //Time keeping Variables 
    [SerializeField] private int timeForEpisode;
    private float timeLeft;

    //Enemy Agent
    public HunterController classObject;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        envMaterial = env.GetComponent<Renderer>().material;
    }

    public override void OnEpisodeBegin()
    {
        
        Vector3 agentPosition = new Vector3(Random.Range(4.78f, -3.42f), -5.99f, Random.Range(10.02f, -1.19f));

        transform.localPosition = agentPosition;
        
        CreatePellet();

        EpisodeTimerNew();
    }

    private void Update()
    {
        CheckRemainingTime();
    }

    private void CreatePellet()
    {
        distList.Clear();
        badDistList.Clear();

        if(spawnedPelletList.Count != 0)
        {
            RemovePellet(spawnedPelletList);
        }

        for(int i = 0; i < pelletCount; i++)
        {
            int counter = 0;
            bool distanceGood;
            bool alreadyDecremented = false;


            GameObject newPellet = Instantiate(food);

            newPellet.transform.parent = enviromentLocation;

            Vector3 pelletLocation = new Vector3(Random.Range(4.78f, -3.42f), -5.99f, Random.Range(10.02f, -1.19f));
            
            if(spawnedPelletList.Count != 0)
            {
                for(int k = 0; k < spawnedPelletList.Count; k++)
                {
                    if(counter < 10)
                    {
                        distanceGood = CheckOverlap(pelletLocation, spawnedPelletList[k].transform.localPosition, 5f);

                        if(distanceGood == false)
                        {
                            pelletLocation = new Vector3(Random.Range(4.78f, -3.42f), -5.99f, Random.Range(10.02f, -1.19f));
                            
                            k--;

                            alreadyDecremented = true;

                            Debug.Log("Too close to pellet");
                        }

                        distanceGood = CheckOverlap(pelletLocation, transform.localPosition, 5f);

                        if (distanceGood == false)
                        {
                            Debug.Log("Too close to agent");

                            pelletLocation = new Vector3(Random.Range(4.78f, -3.42f), -5.99f, Random.Range(10.02f, -1.19f));
                            
                            if(alreadyDecremented == false)
                            {
                                k--;
                            }
                        }

                        counter++;
                    }
                    else
                    {
                        k = spawnedPelletList.Count;
                    }
                }
            }

            newPellet.transform.localPosition = pelletLocation;

            spawnedPelletList.Add(newPellet);

            Debug.Log("Spawned");
        }
    }

    public List<float> distList = new List<float>();
    public List<float> badDistList = new List<float>();

    public bool CheckOverlap(Vector3 avoidOverlap,  Vector3 alreadyExisting, float minDist)
    {
        float Dist = Vector3.Distance(avoidOverlap, alreadyExisting);

        if(minDist <= Dist)
        {
            distList.Add(Dist);
            return true;
        }

        badDistList.Add(Dist);

        return false;
    }

    private void RemovePellet(List<GameObject> toBeDeletedGameObjectList)
    {
        foreach(GameObject i in toBeDeletedGameObjectList)
        {
            Destroy(i.gameObject);
        }

        toBeDeletedGameObjectList.Clear();
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
        if(other.gameObject.tag == "Ball")
        {
            spawnedPelletList.Remove(other.gameObject);
            Destroy(other.gameObject);
            AddReward(10f);

            if(spawnedPelletList.Count == 0)
            {
                envMaterial.color = Color.green;
                RemovePellet(spawnedPelletList);
                AddReward(5f);
                classObject.AddReward(-5f);
                classObject.EndEpisode();
                EndEpisode();
            }
        }
        if (other.gameObject.tag == "Wall")
        {
            envMaterial.color = Color.red;
            RemovePellet(spawnedPelletList);
            AddReward(-15f);
            classObject.EndEpisode();
            EndEpisode();
        }
    }

    private void EpisodeTimerNew()
    {
        timeLeft = Time.time + timeForEpisode;
    }

    private void CheckRemainingTime()
    {
        if(Time.time >= timeLeft)
        {
            envMaterial.color = Color.blue;
            AddReward(-15f);
            classObject.AddReward(-15);
            RemovePellet(spawnedPelletList);
            classObject.EndEpisode();
            EndEpisode();
        }
    }
}
