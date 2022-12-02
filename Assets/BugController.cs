using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BugController : MonoBehaviour
{
    Vector3 startPosition, startRotation;
    NeuralNetwork network;
    ClosestPoint progressTracker;
    public int currentProgressIndex;

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
    [SerializeField, Range(0f, 1f)]float aSensor, bSensor, cSensor, dSensor, eSensor;

    List<float> inputValueList = new List<float>();
    List<float> outputValueList = new List<float>();

    public int genome;
    bool isMultiSpawned;

    [Header("Visuals")]
    public bool isParentOfGeneration;
    SpriteRenderer renderer;
    TrailRenderer trail;
    [SerializeField] Color parentColor;
    [SerializeField] Color standardColor;

    MultiGenerationGeneticManager geneticManager;

    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rotationAngle = transform.eulerAngles.z;
        progressTracker = FindObjectOfType<ClosestPoint>();
        trail = GetComponentInChildren<TrailRenderer>();
        renderer = GetComponentInChildren<SpriteRenderer>();
        geneticManager = FindObjectOfType<MultiGenerationGeneticManager>();
        //test code
        //network.Initialise(LAYERS, NEURONS, INPUT_COUNT, OUTPUT_COUNT);

    }

    private void Start()
    {
        currentProgressIndex = progressTracker.InitProgress(transform.position);
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
        Vector3 aDirection = -transform.right;
        Vector3 bDirection = Vector3.Normalize(transform.up + transform.right);
        Vector3 cDirection = transform.up;
        Vector3 dDirection = Vector3.Normalize(transform.up - transform.right);
        Vector3 eDirection = transform.right;

        
        RaycastHit2D aHit = Physics2D.Raycast(transform.position, aDirection, visionDistance, sensorLayerMask);
        if(aHit.collider != null)
        {
            aSensor = 1 - (aHit.distance/visionDistance);
            Debug.DrawLine(transform.position, aHit.point, Color.red);
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
            Debug.DrawLine(transform.position, bHit.point, Color.red);
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
            Debug.DrawLine(transform.position, cHit.point, Color.red);
            //Debug.Log("C:" + cSensor);
        }
        else
        {
            cSensor = 0f;
        }

        RaycastHit2D dHit = Physics2D.Raycast(transform.position, dDirection, visionDistance, sensorLayerMask);
        if(dHit.collider != null)
        {
            dSensor = 1 - (dHit.distance/visionDistance);
            Debug.DrawLine(transform.position, dHit.point, Color.red);
            //Debug.Log("A:" + aSensor);
        }
        else
        {
            aSensor = 0f;
        }

        RaycastHit2D eHit = Physics2D.Raycast(transform.position, eDirection, visionDistance, sensorLayerMask);
        if(eHit.collider != null)
        {
            eSensor = 1 - (eHit.distance/visionDistance);
            Debug.DrawLine(transform.position, eHit.point, Color.red);
            //Debug.Log("A:" + aSensor);
        }
        else
        {
            aSensor = 0f;
        }

        

    }

    void CalculateFitness()
    {
        
        

        if (timeSinceStart > 10 && overallFitness < timeSinceStart)
        {
            Death();
        }

        if (timeSinceStart > 30)
        {
            //time limit
            Death();
        }
        int newProgress = progressTracker.GetProgress(transform.position, currentProgressIndex);
        
        if (newProgress != currentProgressIndex)
        {
            totalDistanceTravelled += Mathf.Min(Mathf.Abs(newProgress - currentProgressIndex), 10);

            currentProgressIndex = newProgress;
            
        }
        averageSpeed = totalDistanceTravelled / timeSinceStart;
        //(((aSensor + bSensor + cSensor) / 3) * sensorMultiplier)
        overallFitness = (totalDistanceTravelled * distanceMultiplier) + (averageSpeed * averageSpeedMultiplier);
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
            accelerationInput = outputValueList[0];
            turningInput = outputValueList[1];
            if(isParentOfGeneration)
            {
                renderer.color = parentColor;
                trail.startColor = parentColor;
                trail.endColor = parentColor;
                trail.sortingOrder = 1;
                renderer.sortingOrder = 1;
            }
            else
            {
                renderer.color = standardColor;
                trail.startColor = standardColor;
                trail.endColor = standardColor;
            }
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
            geneticManager.Death(overallFitness, network, genome);
            Destroy(gameObject);
            if(isParentOfGeneration)
            {
                //a
            }
        }
        else
        {
            
        }
        
    }
}
