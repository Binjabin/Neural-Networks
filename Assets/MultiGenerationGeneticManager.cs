using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using System.Linq;
using TMPro;

public class MultiGenerationGeneticManager : MonoBehaviour
{
    [Header("References")]
    public GameObject bugPrefab;
    public Transform spawnPoint;

    [Header("Generation Settings")]
    public int wavesPerGeneration = 1;
    int wavesSoFar;
    [Range(0, 80)] public int populationPerWave = 50;
    int initialPopulation;
    int spawnedSoFar;
    
    public float delayBetweenGenerations;

    [Header("Crossover Settings")]
    [Range(1, 100)] public int numberOfParents = 1;
    [Range(0f, 1f)] public float mutationRate = 0.055f;
    [Range(0f, 1f)] public float mutationAmount = 0.055f;
    [SerializeField] bool nudgeAllValuesOnMutate;

    List<int> genePool = new List<int>();
    int naturallySelected;

    NeuralNetwork[] population;
    NeuralNetwork[] sortedPopulation;

    [Header("Debug")]
    public int currentGeneration;
    public int currentlyAlive;

    [Header("UI")]
    [SerializeField] TextMeshProUGUI generationCounterText;
    [SerializeField] TextMeshProUGUI waveCounterText;
    [SerializeField] TextMeshProUGUI highestFitnessText;
    float highestFitness = 0f;

    [Header("Network Options")]
    public int LAYERS = 1;
    public int NEURONS = 10;
    public int INPUT_COUNT = 3;
    public int OUTPUT_COUNT = 2;
    //TMProUGUI averageFitness;


    NeuralNetworkGraph graph;


    void Start()
    {
        initialPopulation = wavesPerGeneration * populationPerWave;
        currentGeneration = 0;
        graph = FindObjectOfType<NeuralNetworkGraph>();
        CreatePopulation();
    }

    void CreatePopulation()
    {
        population = new NeuralNetwork[initialPopulation];
        FillPopulationWithRandomValues(population, 0);
        SpawnGeneration();
    }



    void SpawnWave()
    {
        wavesSoFar++;
        currentlyAlive = populationPerWave;
        for(int i = spawnedSoFar; i < populationPerWave * wavesSoFar; i++)
        {
            spawnedSoFar++;
            GameObject newBug = Instantiate(bugPrefab, spawnPoint.position, Quaternion.identity);
            newBug.transform.parent = transform;
            var newBugController = newBug.GetComponent<BugController>();
            if(newBugController != null)
            {
                newBugController.SpawnWithNetwork(population[i]);
                newBugController.genome = i;
                if (i < numberOfParents && currentGeneration > 0)
                {
                    newBugController.isParentOfGeneration = true;
                }
            }

        }
        UpdateUI();
    }

    void SpawnGeneration()
    {
        wavesSoFar = 0;
        spawnedSoFar = 0;
        


        SpawnWave();
    }

    void FillPopulationWithRandomValues(NeuralNetwork[] newPopulation, int startingIndex)
    {
        while(startingIndex < initialPopulation)
        {
            newPopulation[startingIndex] = new NeuralNetwork();
            newPopulation[startingIndex].Initialise(LAYERS, NEURONS, INPUT_COUNT, OUTPUT_COUNT);
            newPopulation[startingIndex].RandomiseNetwork();
            startingIndex++;
        }
    }

    public void Death(float fitness, NeuralNetwork network, int populationIndex)
    {
        currentlyAlive--;
        population[populationIndex].fitness = fitness;
        if(fitness > highestFitness)
        {
            //Debug.Log("Set new highest:" + fitness);
            highestFitness = fitness;
            UpdateUI();
        }
        if(currentlyAlive == 0)
        {
            if(wavesSoFar < wavesPerGeneration)
            {
                SpawnWave();
            }
            else
            {
                StartCoroutine(WaitForNextGeneration());
            }
        }
    }

    IEnumerator WaitForNextGeneration()
    {
        yield return new WaitForSeconds(delayBetweenGenerations);
        Repopulate();
    }

    void UpdateUI()
    {
        generationCounterText.text = "Generation: " + currentGeneration;
        waveCounterText.text = "Wave: " + wavesSoFar + "/" + wavesPerGeneration;
        highestFitnessText.text = "Highest fitness: " + System.Math.Round(highestFitness, 2);

        if (graph != null)
        {
            graph.SetGraph(population[0]);
        }

    }

    void Repopulate()
    {
        genePool.Clear();
        currentGeneration++;
        naturallySelected = 0;
        SortPopulation();


        NeuralNetwork[] newPopulation = PickPopulation();

        Crossover(newPopulation);
        Mutate(newPopulation);

        //FillPopulationWithRandomValues(newPopulation, naturallySelected);

        population = newPopulation;
        
        SpawnGeneration();

    }

    NeuralNetwork[] PickPopulation()
    {
        NeuralNetwork[] newPopulation = new NeuralNetwork[initialPopulation];

        for(int i = 0; i < numberOfParents; i++)
        {
            newPopulation[naturallySelected] = population[i].InitialiseCopy(LAYERS, NEURONS, INPUT_COUNT, OUTPUT_COUNT);
            newPopulation[naturallySelected].fitness = 0f;
            genePool.Add(i);
            naturallySelected++;
        }

        return newPopulation;
    }

    private void SortPopulation()
    {
        population = population.OrderByDescending(x => x.fitness).ToArray();
        sortedPopulation = population;
    }

    private void Crossover(NeuralNetwork[] newPopulation)
    {
        for(int i = numberOfParents; i < initialPopulation; i++)
        {

            int aParentIndex = 0;
            int bParentIndex = 0;

            if(genePool.Count >= 1)
            {
                aParentIndex = genePool[Random.Range(0, genePool.Count)];
                bParentIndex = genePool[Random.Range(0, genePool.Count)];
            }

            NeuralNetwork child = new NeuralNetwork();
            child.Initialise(LAYERS, NEURONS, INPUT_COUNT, OUTPUT_COUNT);
            child.fitness = 0;

            for(int w = 0; w < child.weights.Count; w++)
            {

                for(int x = 0; x < child.weights[w].RowCount; x++)
                {
                    for(int y = 0; y < child.weights[w].ColumnCount; y++)
                    {
                        if(Random.Range(0.0f, 1.0f) < 0.5f)
                        {
                            child.weights[w][x,y] = population[aParentIndex].weights[w][x, y];
                        }
                        else
                        {
                            child.weights[w][x,y] = population[bParentIndex].weights[w][x, y];
                        }
                    }
                }
            }

            for(int w = 0; w < child.biases.Count; w++)
            {
                for(int x = 0; x < child.biases[w].RowCount; x++)
                {
                    for(int y = 0; y < child.biases[w].ColumnCount; y++)
                    {
                        if(Random.Range(0.0f, 1.0f) < 0.5f)
                        {
                            child.biases[w][x,y] = population[aParentIndex].biases[w][x, y];
                        }
                        else
                        {
                            child.biases[w][x,y] = population[bParentIndex].biases[w][x, y];
                        }
                    }
                }
            }
            newPopulation[naturallySelected] = child;
            naturallySelected++;
        }
    }

    Matrix<float> CopyMatrix(Matrix<float> inMatrix)
    {
        Matrix<float> newMatrix = Matrix<float>.Build.Dense(inMatrix.RowCount, inMatrix.ColumnCount);
            
        for(int x = 0; x < inMatrix.RowCount; x++)
        {
            for(int y = 0; y < inMatrix.ColumnCount; y++)
            {
                newMatrix[x,y] = inMatrix[x,y];
            }
        }
        return newMatrix;
    }

    void Mutate(NeuralNetwork[] newPopulation)
    {
        //randomise for each non-parent
        for(int i = numberOfParents; i < naturallySelected; i++)
        {
            DoMutation(newPopulation[i]);

            if (Random.Range(0.0f, 1.0f) < mutationRate)
            {
                if(nudgeAllValuesOnMutate)
                {
                    //mutate all weights slightly
                    for (int w = 0; w < newPopulation[i].weights.Count; w++)
                    {
                        newPopulation[i].weights[w] = NudgeValueMatrix(newPopulation[i].weights[w]);
                    }
                    for (int w = 0; w < newPopulation[i].biases.Count; w++)
                    {
                        newPopulation[i].biases[w] = NudgeValueMatrix(newPopulation[i].biases[w]);
                    }
                }
                else
                {
                    //nudge a single weight
                    if (Random.Range(0.0f, 1.0f) < 0.5f)
                    {
                        //choose a weight
                        int c = Random.Range(0, newPopulation[i].weights.Count);
                        newPopulation[i].weights[c] = NudgeValueMatrix(newPopulation[i].weights[c]);
                    }
                    else
                    {
                        //choose a bias
                        int c = Random.Range(0, newPopulation[i].biases.Count);
                        newPopulation[i].weights[c] = NudgeValueMatrix(newPopulation[i].weights[c]);
                    }
                }
                
            }
        }

    }

    void DoMutation(NeuralNetwork net)
    {
        int randomPoints = 0;
        bool done = false;

        //work out amount of values to mutate
        while (!done)
        {
            if (Random.Range(0.0f, 1.0f) < mutationRate)
            {
                randomPoints++;
            }
            else
            {
                done = false;
            }
        }

        //work out which matrix to mutate from
        int numberOfMatricies = net.weights.Count + net.biases.Count;
        Matrix<float> matrixChosen;
        int matrixIndexChosen = Random.Range(0, numberOfMatricies);


        if(matrixIndexChosen > net.weights.Count)
        {
            matrixChosen = net.biases[matrixIndexChosen - net.weights.Count];
        }
        else
        {
            matrixChosen = net.biases[matrixIndexChosen - net.weights.Count];
        }

    }

    Matrix<float> NudgeValueMatrix(Matrix<float> inMatrix)
    {
        int numberOfWeights = inMatrix.RowCount * inMatrix.ColumnCount;

        for(int i = 0; i < randomPoints; i++)
        {
            int randomColumn = Random.Range(0, inMatrix.ColumnCount);
            int randomRow = Random.Range(0, inMatrix.RowCount);

            float currentValue = inMatrix[randomRow, randomColumn];
            inMatrix[randomRow, randomColumn] = Mathf.Clamp((currentValue + Random.Range(-0.2f, 0.2f)), -1, 1);
        }


        return inMatrix;
    }

    Matrix<float> NudgeWholeMatrix(Matrix<float> inMatrix)
    {
        for (int x = 0; x < inMatrix.RowCount; x++)
        {
            for (int y = 0; y < inMatrix.RowCount; y++)
            {
                float currentValue = inMatrix[x, y];
                inMatrix[x, y] = Mathf.Clamp((currentValue + Random.Range(-mutationAmount, mutationAmountf)), -1, 1);
            }
        }


        return inMatrix;
    }

}
