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
    [Range(2f, 100f)] public int numberOfParents = 1;
    [Range(0f, 1f)] public float mutationRate = 0.055f;

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





    void Start()
    {
        initialPopulation = wavesPerGeneration * populationPerWave;
        currentGeneration = 0;
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
                if (i < numberOfParents)
                {
                    newBugController.isParentOfGeneration = true;
                }
            }

        }
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
        if(currentlyAlive > 0)
        {
            population[populationIndex].fitness = fitness;
            if(fitness > highestFitness)
            {
                //Debug.Log("Set new highest:" + fitness);
                highestFitness = fitness;
                UpdateUI();
            }
            else
            {
                
            }
        }
        else
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
        highestFitnessText.text = "Highest fitness: " + highestFitness;

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
                //while(aParentIndex == bParentIndex)
                //{
                //    bParentIndex = genePool[Random.Range(0, genePool.Count)];
                //}
            }

            NeuralNetwork child = new NeuralNetwork();
            child.Initialise(LAYERS, NEURONS, INPUT_COUNT, OUTPUT_COUNT);
            child.fitness = 0;

            //improve later to randomise individual weights, not just sets of weights
            for(int w = 0; w < child.weights.Count; w++)
            {
                if(Random.Range(0.0f, 1.0f) < 0.5f)
                {
                    child.weights[w] = population[aParentIndex].weights[w];
                }
                else
                {
                    child.weights[w] = population[bParentIndex].weights[w];
                }
            }

            for(int w = 0; w < child.biases.Count; w++)
            {
                if(Random.Range(0.0f, 1.0f) < 0.5f)
                {
                    child.biases[w] = population[aParentIndex].biases[w];
                }
                else
                {
                    child.biases[w] = population[bParentIndex].biases[w];
                }
            }
            newPopulation[naturallySelected] = child;
            naturallySelected++;

        }
    }

    void Mutate(NeuralNetwork[] newPopulation)
    {
        //randomise for each non-parent
        for(int i = numberOfParents; i < naturallySelected; i++)
        {
            for(int c = 0; c < newPopulation[i].weights.Count; c++)
            {
                if(Random.Range(0.0f, 1.0f) < mutationRate)
                {
                    newPopulation[i].weights[c] = MutateMatrix(newPopulation[i].weights[c]);
                }
            }
            for (int c = 0; c < newPopulation[i].biases.Count; c++)
            {
                if (Random.Range(0.0f, 1.0f) < mutationRate)
                {
                    newPopulation[i].biases[c] = MutateMatrix(newPopulation[i].biases[c]);
                }
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
            inMatrix[randomRow, randomColumn] = Mathf.Clamp((currentValue + Random.Range(-0.5f, 0.5f)), -1, 1);
        }


        return inMatrix;
    }

}
