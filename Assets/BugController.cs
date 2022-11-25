using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BugController : MonoBehaviour
{
    Vector3 startPosition, startRotation;
    NeuralNetwork network;

    [Header("Car Settings")]
    public float visionDistance = 5f;

    public float driftFactor = 0.95f;
    public float accelerationFactor = 30f;
    public float turnFactor = 3.5f;
    [Range(-1f, 1f)] public float accelerationInput, turningInput;

    float rotationAngle;
    Rigidbody2D rb;

    

    [Header("Fitness")]
    public float timeSinceStart = 0f;
    public float overallFitness;
    public float distanceMultiplier = 1.4f;
    public float averageSpeedMultiplier = 0.2f;
    public float sensorMultiplier;

    Vector3 lastPosition;
    float totalDistanceTravelled;
    float averageSpeed;

    public LayerMask sensorLayerMask;
    [SerializeField, Range(0f, 1f)]float aSensor, bSensor, cSensor;

    List<float> inputValueList = new List<float>();
    List<float> outputValueList = new List<float>();

    public int genome;
    bool isMultiSpawned;

    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rotationAngle = transform.eulerAngles.z;


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
            aSensor = 1 - (aHit.distance/visionDistance);
            Debug.DrawRay(transform.position, aDirection * visionDistance, Color.red);
            //Debug.Log("A:" + aSensor);
        }
        else
        {
            aSensor = 0f;
        }

        RaycastHit2D bHit = Physics2D.Raycast(transform.position, bDirection, visionDistance, sensorLayerMask);
        if(bHit.collider != null)
        {
            bSensor = 1 - (bHit.distance/visionDistance);
            Debug.DrawRay(transform.position, bDirection * visionDistance, Color.red);
            //Debug.Log("B:" + bSensor + " " + bHit.collider.gameObject);
        }
        else
        {
            bSensor = 0f;
        }

        RaycastHit2D cHit = Physics2D.Raycast(transform.position, cDirection, visionDistance, sensorLayerMask);
        if(cHit.collider != null)
        {
            cSensor = 1 - (cHit.distance/visionDistance);
            Debug.DrawRay(transform.position, cDirection * visionDistance, Color.red);
            //Debug.Log("C:" + cSensor);
        }
        else
        {
            cSensor = 0f;
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
    public void MoveCar()
    {
        //forward engine
        Vector2 engineForceVector = transform.up * accelerationInput * accelerationFactor;
        rb.AddForce(engineForceVector, ForceMode2D.Force);


        //drift
        Vector2 forwardVelocity = transform.up * Vector2.Dot(rb.velocity, transform.up);
        Vector2 rightVelocity = transform.right * Vector2.Dot(rb.velocity, transform.right);
        rb.velocity = forwardVelocity + rightVelocity * driftFactor;

        //turning
        float minSpeedForTurnFactor = rb.velocity.magnitude / 8;
        minSpeedForTurnFactor = Mathf.Clamp01(minSpeedForTurnFactor);
        rotationAngle -= turningInput * turnFactor * minSpeedForTurnFactor;
        rb.MoveRotation(rotationAngle);


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
            accelerationInput = (outputValueList[0] + 1 )/ 2;
            turningInput = outputValueList[1];
        }
        
        

        MoveCar();

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
