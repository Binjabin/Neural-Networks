using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BugController : MonoBehaviour
{
    Vector3 startPosition, startRotation;
    NeuralNetwork network;
    public float visionDistance = 5f;

    [Range(-1f, 1f)] 
    public float acceleration, turning;
    
    public float timeSinceStart = 0f;

    [Header("Fitness")]
    public float overallFitness;
    public float distanceMultiplier = 1.4f;
    public float averageSpeedMultiplier = 0.2f;
    public float sensorMultiplier;

    Vector3 lastPosition;
    float totalDistanceTravelled;
    float averageSpeed;

    public LayerMask sensorLayerMask;
    float aSensor, bSensor, cSensor;

    List<float> inputValueList = new List<float>();
    List<float> outputValueList = new List<float>();

    public int genome;
    bool isMultiSpawned;

    // Start is called before the first frame update
    void Awake()
    {
        startPosition = transform.position;
        startRotation = transform.eulerAngles;


        //test code
        //network.Initialise(LAYERS, NEURONS, INPUT_COUNT, OUTPUT_COUNT);

    }

    void Reset()
    {
        timeSinceStart = 0f;
        totalDistanceTravelled = 0f;
        averageSpeed = 0f;
        lastPosition = startPosition;
        overallFitness = 0f;
        transform.position = startPosition;
        transform.eulerAngles = startRotation;
        Destroy(gameObject);

        //network.Initialise(LAYERS, NEURONS, INPUT_COUNT, OUTPUT_COUNT);
    }

    public void ResetWithNetwork(NeuralNetwork net)
    {
        network = net;
        isMultiSpawned = false;
        Reset();
    }

    public void SpawnWithNetwork(NeuralNetwork net)
    {
        network = net;
        isMultiSpawned = true;
    }

    private void OnCollisionEnter2D(Collision2D collision) 
    {
        Death();
    }

    void InputSensors()
    {
        Vector3 aDirection = Vector3.Normalize(transform.up + transform.right);
        Vector3 bDirection = transform.up;
        Vector3 cDirection = Vector3.Normalize(transform.up - transform.right);


        RaycastHit2D aHit = Physics2D.Raycast(transform.position, aDirection, visionDistance, sensorLayerMask);
        if(aHit.collider != null)
        {
            aSensor = aHit.distance/visionDistance;
            Debug.DrawRay(transform.position, aDirection * visionDistance, Color.red);
            //Debug.Log("A:" + aSensor);
        }
        else
        {
            aSensor = 1f;
        }

        RaycastHit2D bHit = Physics2D.Raycast(transform.position, bDirection, visionDistance, sensorLayerMask);
        if(bHit.collider != null)
        {
            bSensor = bHit.distance/visionDistance;
            Debug.DrawRay(transform.position, bDirection * visionDistance, Color.red);
            //Debug.Log("B:" + bSensor + " " + bHit.collider.gameObject);
        }
        else
        {
            bSensor = 1f;
        }

        RaycastHit2D cHit = Physics2D.Raycast(transform.position, cDirection, visionDistance, sensorLayerMask);
        if(cHit.collider != null)
        {
            cSensor = cHit.distance/visionDistance;
            Debug.DrawRay(transform.position, cDirection * visionDistance, Color.red);
            //Debug.Log("C:" + cSensor);
        }
        else
        {
            cSensor = 1f;
        }


    }

    void CalculateFitness()
    {
        totalDistanceTravelled += Vector3.Distance(transform.position, lastPosition);
        averageSpeed = totalDistanceTravelled/timeSinceStart;
        overallFitness = (totalDistanceTravelled * distanceMultiplier) + (averageSpeed * averageSpeedMultiplier) + (((aSensor + bSensor + cSensor)/3)*sensorMultiplier);

        if (timeSinceStart > 20 && overallFitness < 10)
        {
            Death();
        }

        if (timeSinceStart > 30)
        {
            //save network to json
            Death();
        }

    }

    Vector3 input;
    public void MoveCar(float inputAcceleration, float inputRotation)
    {
        //tweak this to make more sense later
        Vector3 positionLerpTarget = new Vector3(0f, inputAcceleration * visionDistance, 0f);
        input = Vector3.Lerp(Vector3.zero, positionLerpTarget, 0.1f);
        input = transform.TransformDirection(input);
        transform.position += input;

        float newRotation = Mathf.Lerp(0f, inputRotation*90f, 0.05f);
        transform.eulerAngles += new Vector3(0f, 0f, newRotation);

    }

    void Update()
    {
        Debug.DrawRay(transform.position, Vector3.Normalize(transform.up + transform.right));
        Debug.DrawRay(transform.position, transform.up);
        Debug.DrawRay(transform.position, Vector3.Normalize(transform.up - transform.right));
    }

    private void FixedUpdate() 
    {
        InputSensors();
        lastPosition = transform.position;

        //neural network here
        inputValueList.Clear();
        inputValueList.Add(aSensor);
        inputValueList.Add(bSensor);
        inputValueList.Add(cSensor);

        if(network != null)
        {
            outputValueList = network.RunNetwork(inputValueList);
            acceleration = outputValueList[0];
            turning = outputValueList[1];
        }
        
        

        MoveCar(acceleration, turning);

        timeSinceStart += Time.deltaTime;
        CalculateFitness();

        //later reset accel and rotation

    }

    private void Death()
    {
        if(network != null)
        {
            FindObjectOfType<MultiGenerationGeneticManager>().Death(overallFitness, network, genome);
            Destroy(gameObject);
        }
        else
        {
            Reset();
        }
        
    }
}
