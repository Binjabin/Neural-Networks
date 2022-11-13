using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using System.Linq;

public class GeneticManager : MonoBehaviour
{
    [Header("References")]
    public BugController controller;

    [Header("Generation Settings")]
    public int initialPopulation = 85;
    [Range(0f, 1f)] public float mutationRate = 0.055f;

    [Header("Crossover Settings")]
    public int bestAgentSelectionCount = 8;
    public int worstAgentSelectionCount = 3;
    public int numberToCrossover;

    List<int> genePool = new List<int>();
    int naturallySelected;

    NeuralNetwork[] population;

    [Header("Debug")]
    public int currentGeneration;
    public int currentGenome = 0;


    void Start()
    {
        CreatePopulation();
    }

    void CreatePopulation()
    {
        population = new NeuralNetwork[initialPopulation];
        FillPopulationWithRandomValues(population, 0);
        ResetToCurrentGenome();
    }

    void ResetToCurrentGenome()
    {
        controller.ResetWithNetwork(population[currentGenome]);
    }


    void FillPopulationWithRandomValues(NeuralNetwork[] newPopulation, int startingIndex)
    {
        while(startingIndex < initialPopulation)
        {
            newPopulation[startingIndex] = new NeuralNetwork();
            newPopulation[startingIndex].Initialise(controller.LAYERS, controller.NEURONS, controller.INPUT_COUNT, controller.OUTPUT_COUNT);
            startingIndex++;
        }
    }

    public void Death(float fitness, NeuralNetwork network)
    {
        if(currentGenome < population.Length - 1)
        {
            population[currentGenome].fitness = fitness;
            currentGenome++;
            ResetToCurrentGenome();
        }
        else
        {
            Repopulate();
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

        FillPopulationWithRandomValues(newPopulation ,naturallySelected);

        population = newPopulation;
        currentGenome = 0;
        ResetToCurrentGenome();

    }

    NeuralNetwork[] PickPopulation()
    {
        NeuralNetwork[] newPopulation = new NeuralNetwork[initialPopulation];

        for(int i = 0; i < bestAgentSelectionCount; i++)
        {
            newPopulation[naturallySelected] = population[i].InitialiseCopy(controller.LAYERS, controller.NEURONS, controller.INPUT_COUNT, controller.OUTPUT_COUNT);
            newPopulation[naturallySelected].fitness = 0f;

            int numberOfThisNetwork = Mathf.RoundToInt(population[i].fitness) * 10;
            for(int c = 0; c < numberOfThisNetwork; c++)
            {
                genePool.Add(i);
            }
            naturallySelected++;
        }

        for(int i = 0; i < worstAgentSelectionCount; i++)
        {
            int last = population.Length - 1;
            last -= i;
            newPopulation[naturallySelected] = population[last].InitialiseCopy(controller.LAYERS, controller.NEURONS, controller.INPUT_COUNT, controller.OUTPUT_COUNT);
            newPopulation[naturallySelected].fitness = 0f;

            int numberOfThisNetwork = Mathf.RoundToInt(population[last].fitness) * 10;
            for(int c = 0; c < numberOfThisNetwork; c++)
            {
                genePool.Add(last);
            }
            naturallySelected++;
        }

        return newPopulation;
    }

    private void SortPopulation()
    {
        population = population.OrderByDescending(x => x.fitness).ToArray();
    }

    private void Crossover(NeuralNetwork[] newPopulation)
    {
        for(int i = 0; i < numberToCrossover; i+=2)
        {
            int aIndex = i;
            int bIndex = i+1;

            if(genePool.Count >= 1)
            {
                aIndex = genePool[Random.Range(0, genePool.Count)];
                bIndex = genePool[Random.Range(0, genePool.Count)];
                while(aIndex != bIndex)
                {
                    bIndex = genePool[Random.Range(0, genePool.Count)];
                }
            }

            NeuralNetwork child1 = new NeuralNetwork();
            NeuralNetwork child2 = new NeuralNetwork();
            child1.Initialise(controller.LAYERS, controller.NEURONS, controller.INPUT_COUNT, controller.OUTPUT_COUNT);
            child2.Initialise(controller.LAYERS, controller.NEURONS, controller.INPUT_COUNT, controller.OUTPUT_COUNT);
            child1.fitness = 0;
            child2.fitness = 0;


            //improve later mabye
            for(int w = 0; w < child1.weights.Count; w++)
            {
                if(Random.Range(0.0f, 1.0f) < 0.5f)
                {
                    child1.weights[w] = population[aIndex].weights[w];
                    child2.weights[w] = population[bIndex].weights[w];
                }
                else
                {
                    child1.weights[w] = population[bIndex].weights[w];
                    child2.weights[w] = population[aIndex].weights[w];
                }
            }

            for(int w = 0; w < child1.biases.Count; w++)
            {
                if(Random.Range(0.0f, 1.0f) < 0.5f)
                {
                    child1.biases[w] = population[aIndex].biases[w];
                    child2.biases[w] = population[bIndex].biases[w];
                }
                else
                {
                    child1.biases[w] = population[bIndex].biases[w];
                    child2.biases[w] = population[aIndex].biases[w];
                }
            }

            newPopulation[naturallySelected] = child1;
            naturallySelected++;
            newPopulation[naturallySelected] = child2;
            naturallySelected++;

        }
    }

    void Mutate(NeuralNetwork[] newPopulation)
    {
        for(int i = 0; i < naturallySelected; i++)
        {
            for(int c = 0; c < newPopulation[i].weights.Count; c++)
            {
                if(Random.Range(0.0f, 1.0f) < mutationRate)
                {
                    newPopulation[i].weights[c] = MutateMatrix(newPopulation[i].weights[c]);
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
            inMatrix[randomRow, randomColumn] = Mathf.Clamp((currentValue + Random.Range(-1f, 1f)), -1, 1);
        }


        return inMatrix;
    }

}
