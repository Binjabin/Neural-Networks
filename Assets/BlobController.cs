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
    float timeAlive;
    
    List<float> inputValueList = new List<float>();
    List<float> outputValueList = new List<float>();
    List<Vector3> directionsToLook = new List<Vector3>();
    List<float> sensorValues = new List<float>();

    NeuralNetwork network;
    public int genome;

    BacteriaGeneticManager manager;

    [SerializeField] Gradient colorOverTime;
    [SerializeField] SpriteRenderer mainBlob;


    void Start()
    {
        timeAlive = 0f;
        //look directions (8 directions around)
        currentEnergy = startingEnergy;
        for(int i = 0; i < viewDirections; i++)
        {
            float thisAngle = i * (360 / viewDirections);
            directionsToLook.Add(Quaternion.AngleAxis(thisAngle, new Vector3(0, 0, 1)) * new Vector3(1, 0, 0));
        }
        
        sensorValues.Add(0f);
        sensorValues.Add(0f);

        //for (int i = 0; i < directionsToLook.Count; i++)
        //{
        //    sensorValues.Add(0f);
        //}
        manager = FindObjectOfType<BacteriaGeneticManager>();
    }
    void DirectionSensors()
    {
        Vector3 direction = GetNearestFoodDirection();
        direction.Normalize();
        sensorValues[0] = direction.x;
        sensorValues[1] = direction.y;
    }
    // Update is called once per frame
    void FixedUpdate()
    {

        //InputSensors();
        timeAlive += Time.deltaTime;
        mainBlob.color = colorOverTime.Evaluate(Mathf.Clamp01(timeAlive / 90f));
        DirectionSensors();

       

        inputValueList = sensorValues;

        if (network != null)
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
            manager.currentlyAlive--;
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

    Vector3 GetNearestFoodDirection()
    {
        Vector3 directionToTravel = Vector3.zero;
        float nearestDistance = Mathf.Infinity;
        GameObject nearestObject = null;
        foreach (GameObject foodObject in manager.foodList)
        {
            float x = 1;
            float y = 1;
            if (transform.position.x > 0) { x = -1; }
            if (transform.position.y > 0) { y = -1; }

            Vector3 altPos1 = new Vector3(transform.position.x + (manager.maxDistanceFromCenter * 2 * x), transform.position.y, 0);
            Vector3 altPos2 = new Vector3(transform.position.x, transform.position.y + (manager.maxDistanceFromCenter * 2 * y), 0);
            Vector3 altPos3 = new Vector3(transform.position.x + (manager.maxDistanceFromCenter * 2 * x), transform.position.y + (manager.maxDistanceFromCenter * 2 * y), 0);

            float distanceFromOriginal = Vector3.Distance(transform.position, foodObject.transform.position);
            float distanceFromAlt1 = Vector3.Distance(altPos1, foodObject.transform.position);
            float distanceFromAlt2 = Vector3.Distance(altPos2, foodObject.transform.position);
            float distanceFromAlt3 = Vector3.Distance(altPos3, foodObject.transform.position);

            float thisDistance = Mathf.Min(distanceFromOriginal, distanceFromAlt1, distanceFromAlt2, distanceFromAlt3);

            if (thisDistance < nearestDistance)
            {
                nearestDistance = thisDistance;
                nearestObject = foodObject;

                if (thisDistance == distanceFromOriginal)
                {
                    directionToTravel = foodObject.transform.position - transform.position;
                }
                else if (thisDistance == distanceFromAlt1)
                {
                    directionToTravel = foodObject.transform.position - altPos1;
                }
                else if (thisDistance == distanceFromAlt2)
                {
                    directionToTravel = foodObject.transform.position - altPos2;
                }
                else if (thisDistance == distanceFromAlt3)
                {
                    directionToTravel = foodObject.transform.position - altPos3;
                }

            }
        }
        Debug.DrawRay(transform.position, directionToTravel);
        return directionToTravel;
    }
}
