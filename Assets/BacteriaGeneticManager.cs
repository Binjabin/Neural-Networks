using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using System.Linq;
using TMPro;


public class BacteriaGeneticManager : MonoBehaviour
{

    [Header("References")]
    public GameObject agentPrefab;

    [Header("Generation Settings")]
    public int initialPopulation;
    [Range(0f, 1f)] public float mutationRate = 0.055f;

    NeuralNetwork[] population;

    [Header("Debug")]
    public int currentlyAlive;

    [Header("Network Options")]
    public int LAYERS = 1;
    public int NEURONS = 10;
    public int INPUT_COUNT = 3;
    public int OUTPUT_COUNT = 2;
    //TMProUGUI averageFitness;

    [Header("Food Options")]
    [SerializeField] GameObject foodPrefab;
    [SerializeField] int foodToSpawn = 100;
    public List<GameObject> foodList = new List<GameObject>();
    // Start is called before the first frame update
    float foodTimer;
    [SerializeField] float foodRespawnSpeed;

    [Header("Area Options")]
    public float maxDistanceFromCenter = 10f;

    void SpawnFood()
    {
        GameObject foodObject = Instantiate(foodPrefab, GetRandomPosInArena(), Quaternion.identity);
        foodObject.transform.parent = transform;
        foodList.Add(foodObject);
        foodTimer = 0f;
    }

    private void Update()
    {
        foodTimer += Time.deltaTime;
        if(foodTimer >= foodRespawnSpeed && foodList.Count < foodToSpawn)
        {
            SpawnFood();
        }
    }

    public void DeleteFood(GameObject food)
    {
        foodList.Remove(food);
        Debug.Log("destroyed food");
        Destroy(food);
        //SpawnFood();
    }

    public void ClearFood()
    {
        for(int i = 0; i < foodList.Count; i++)
        {
            Destroy(foodList[i]);
        }
        foodList.Clear();
    }

    public void FillFood()
    {
        for(int i = 0; i < foodToSpawn; i++)
        {
            SpawnFood();
        }
    }

    void Start()
    {
        CreatePopulation();
        FillFood();
    }

    void CreatePopulation()
    {
        population = new NeuralNetwork[initialPopulation];
        FillPopulationWithRandomValues(population, 0);
        SpawnInitialPopulation();
    }

    void SpawnInitialPopulation()
    {
        for(int i = 0; i < initialPopulation; i++)
        {
            GameObject newAgent = GameObject.Instantiate(agentPrefab, GetRandomPosInArena(), Quaternion.identity);
            var newAgentController = newAgent.GetComponent<BlobController>();
            if(newAgentController != null)
            {
                newAgentController.SpawnWithNetwork(population[i], true);
            }
        }
    }

    Vector3 GetRandomPosInArena()
    {
        return new Vector3(Random.Range(-maxDistanceFromCenter, maxDistanceFromCenter), Random.Range(-maxDistanceFromCenter, maxDistanceFromCenter), 0f);
    }

    void FillPopulationWithRandomValues(NeuralNetwork[] newPopulation, int startingIndex)
    {
        while(startingIndex < initialPopulation)
        {
            newPopulation[startingIndex] = new NeuralNetwork();
            newPopulation[startingIndex].Initialise(LAYERS, NEURONS, INPUT_COUNT, OUTPUT_COUNT);
            population[startingIndex].RandomiseNetwork();
            startingIndex++;
        }
    }


    void Mutate(NeuralNetwork agentToMutate)
    {
        
        for(int c = 0; c < agentToMutate.weights.Count; c++)
        {
            if(Random.Range(0.0f, 1.0f) < mutationRate)
            {
                agentToMutate.weights[c] = MutateMatrix(agentToMutate.weights[c]);
            }
        }
    }

    Matrix<float> MutateMatrix(Matrix<float> inMatrix)
    {
        int numberOfWeights = inMatrix.RowCount * inMatrix.ColumnCount;
        //minimum 1 weight change
        float minProportion = 1f/numberOfWeights;
        
        //max a 7th
        float maxProportion = Mathf.Max(minProportion, 1f/7f);

        float proportionOfWeightsToChange = Random.Range(minProportion, maxProportion);
        int randomPoints = Mathf.RoundToInt(proportionOfWeightsToChange * numberOfWeights);

        for(int i = 0; i < randomPoints; i++)
        {
            int randomColumn = Random.Range(0, inMatrix.ColumnCount);
            int randomRow = Random.Range(0, inMatrix.RowCount);

            float currentValue = inMatrix[randomRow, randomColumn];
            inMatrix[randomRow, randomColumn] = Mathf.Clamp((currentValue + Random.Range(-1f, 1f)), -1, 1);
        }


        return inMatrix;
    }

    public void SpawnChild(NeuralNetwork net, Vector3 pos)
    {
        GameObject newAgent = Instantiate(agentPrefab, pos, Quaternion.identity);
        var newAgentController = newAgent.GetComponent<BlobController>();
        if(newAgentController != null)
        {
            NeuralNetwork newNetwork = net.InitialiseCopy(LAYERS, NEURONS, INPUT_COUNT, OUTPUT_COUNT);
            float f = Random.Range(0.0f, 1.0f);
            if(f<mutationRate)
            {
                Mutate(newNetwork);
                newAgentController.SpawnWithNetwork(newNetwork, true);
            }
            else
            {
                newAgentController.SpawnWithNetwork(newNetwork, false);
            }
            
        }
    }
}
