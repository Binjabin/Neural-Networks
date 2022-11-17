using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlobController : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("Bug Settings")]
    [SerializeField] float speed;
    [SerializeField] float visionDistance;
    [SerializeField] LayerMask sensorLayerMask;
    [SerializeField] float xInput;
    [SerializeField] float yInput;
    public float viewDirections;

    [Header("Fitness")]
    public float currentEnergy;
    public float energyPerFood = 10f;
    public float energyLossPerSecond = 1f;
    public float energyToReproduce = 30f;
    public float startingEnergy = 10f;

    
    List<float> inputValueList = new List<float>();
    List<float> outputValueList = new List<float>();
    List<Vector3> directionsToLook = new List<Vector3>();
    List<float> sensorValues = new List<float>();

    NeuralNetwork network;
    public int genome;

    BacteriaGeneticManager manager;

    void Start()
    {
        //look directions (8 directions around)
        currentEnergy = startingEnergy;
        for(int i = 0; i < viewDirections; i++)
        {
            float thisAngle = i * (360 / viewDirections);
            directionsToLook.Add(Quaternion.AngleAxis(thisAngle, new Vector3(0, 0, 1)) * new Vector3(1, 0, 0));
        }
        
        
        for (int i = 0; i < directionsToLook.Count; i++)
        {
            sensorValues.Add(0f);
        }
        manager = FindObjectOfType<BacteriaGeneticManager>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
        InputSensors();

        inputValueList = sensorValues;

        if(network != null)
        {
            outputValueList = network.RunNetwork(inputValueList);

            xInput = outputValueList[0];
            yInput = outputValueList[1]; 
        }

        Move();
        currentEnergy -= energyLossPerSecond * Time.deltaTime;
        CalculateFitness();
    }

    void InputSensors()
    {
        
        for(int i = 0; i < directionsToLook.Count; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, directionsToLook[i], visionDistance, sensorLayerMask);
            if(hit.collider != null)
            {
                sensorValues[i] = (visionDistance - hit.distance)/visionDistance;
                Debug.DrawRay(transform.position, directionsToLook[i] * visionDistance, Color.red);
            }
            else
            {
                Debug.DrawRay(transform.position, directionsToLook[i] * visionDistance, Color.green);
            }
        }
    }

    void Move()
    {
        Vector3 movementDirection = new Vector3(xInput, yInput, 0f);
        transform.position += movementDirection * Time.deltaTime * speed;

        if (transform.position.x > manager.maxDistanceFromCenter)
        {
            transform.position -= new Vector3(2 * manager.maxDistanceFromCenter, 0f, 0f);
        }
        if (transform.position.x < -manager.maxDistanceFromCenter)
        {
            transform.position += new Vector3(2 * manager.maxDistanceFromCenter, 0f, 0f);
        }
        if (transform.position.y > manager.maxDistanceFromCenter)
        {
            transform.position -= new Vector3(0f, 2 * manager.maxDistanceFromCenter, 0f);
        }
        if (transform.position.y < -manager.maxDistanceFromCenter)
        {
            transform.position -= new Vector3(0f, 2 * manager.maxDistanceFromCenter, 0f);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) 
    {
        GameObject food = collision.collider.gameObject;
        if(food.GetComponent<FoodObject>() != null)
        {
            food.GetComponent<FoodObject>().TryEat(this);
        }
    }

    public void Eat(GameObject food)
    {
        Debug.Log("eaten food");
        currentEnergy += energyPerFood;
        FindObjectOfType<BacteriaGeneticManager>().DeleteFood(food);
    }

    private void Death()
    {
        if(network != null)
        {
            Destroy(gameObject);
        }
        
    }

    void MutateTraits()
    {
        visionDistance += Random.Range(-1f, 1f);
        visionDistance = Mathf.Max(visionDistance, 0f);

        speed += Random.Range(-0.5f, 0.5f);
        speed = Mathf.Max(speed, 0f);

    }

    void CalculateFitness()
    {
        if (currentEnergy <= 0f)
        {
            Death();
        }
        if(currentEnergy >= 30f)
        {
            currentEnergy -= 10f;
            FindObjectOfType<BacteriaGeneticManager>().SpawnChild(network, transform.position);

        }
    }

    public void SpawnWithNetwork(NeuralNetwork net, bool mutateTraits)
    {
        network = net;
        if(mutateTraits)
        {
            MutateTraits();
        }
    }
}
