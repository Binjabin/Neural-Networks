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

    [Header("Fitness")]
    public float overallFitness;
    public float fitnessPerFood = 1f;
    float timeSinceStart;

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
        directionsToLook.Add(transform.up);
        directionsToLook.Add(Vector3.Normalize(transform.up + transform.right));
        directionsToLook.Add(transform.right);
        directionsToLook.Add(Vector3.Normalize(-transform.up + transform.right));
        directionsToLook.Add(-transform.up);
        directionsToLook.Add(Vector3.Normalize(-transform.up - transform.right));
        directionsToLook.Add(-transform.right);
        directionsToLook.Add(Vector3.Normalize(transform.up - transform.right));
        for(int i = 0; i < 8; i++)
        {
            sensorValues.Add(0f);
        }
        timeSinceStart = 0f;
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
        timeSinceStart += Time.deltaTime;
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
        FoodObject food = collision.collider.gameObject.GetComponent<FoodObject>();
        Debug.Log("hit food");
        if(food != null)
        {
            Debug.Log("hit food");
            overallFitness += fitnessPerFood;
            food.Eaten();
            FindObjectOfType<BacteriaGeneticManager>().SpawnChild(network, transform.position);
        }
    }

    private void Death()
    {
        if(network != null)
        {
            Destroy(gameObject);
        }
        
    }

    void CalculateFitness()
    {
        if (timeSinceStart > 10)
        {
            Death();
        }
    }

    public void SpawnWithNetwork(NeuralNetwork net)
    {
        network = net;
    }
}
