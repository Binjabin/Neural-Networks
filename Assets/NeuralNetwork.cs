using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using System;
using Random = UnityEngine.Random;
[System.Serializable]
public class NeuralNetwork
{


    public Matrix<float> inputLayer;

    public List<Matrix<float>> hiddenLayers = new List<Matrix<float>>();

    public Matrix<float> outputLayer;

    public List<Matrix<float>> weights = new List<Matrix<float>>();

    public List<Matrix<float>> biases = new List<Matrix<float>>();

    public float fitness;

    List<float> outputValues = new List<float>();

    
    public void Initialise(int hiddenLayerCount, int hiddenNeuronCount, int inputCount, int outputCount)
    {
        
        hiddenLayers.Clear();
        weights.Clear();
        biases.Clear();
        outputValues.Clear();

        inputLayer = Matrix<float>.Build.Dense(1,inputCount);
        inputLayer.Clear();

        //initialise biases list
        for (int i = 0; i < hiddenLayerCount; i++)
        {
            biases.Add(Matrix<float>.Build.Dense(1, hiddenNeuronCount));
        }
        biases.Add(Matrix<float>.Build.Dense(1, outputCount));


        for (int i = 0; i < hiddenLayerCount; i++)
        {
            Matrix<float> f = Matrix<float>.Build.Dense(1, hiddenNeuronCount);
            hiddenLayers.Add(f);
  
        }

        outputLayer = Matrix<float>.Build.Dense(1,outputCount);
        outputLayer.Clear();
        

        for (int i = 0; i < hiddenLayerCount + 1; i++)
        {
            if(i == 0)
            {
                Matrix<float> inputToHidden = Matrix<float>.Build.Dense(inputCount, hiddenNeuronCount);
                weights.Add(inputToHidden);
            }
            else if(i == hiddenLayerCount)
            {
                Matrix<float> hiddentoOutput = Matrix<float>.Build.Dense(hiddenNeuronCount, outputCount);
                weights.Add(hiddentoOutput);
            }
            else
            {
                Matrix<float> hiddentoHidden = Matrix<float>.Build.Dense(hiddenNeuronCount, hiddenNeuronCount);
                weights.Add(hiddentoHidden);
            }
        }

        for (int i = 0; i < weights.Count; i++)
        {
            for(int x = 0; x < weights[i].RowCount; x++)
            {
                for(int y = 0; y < weights[i].ColumnCount; y++)
                {
                    weights[i][x,y] = 0f;
                }
            }
        }
    }

    public void RandomiseNetwork()
    {
        for (int i = 0; i < weights.Count; i++)
        {
            for (int x = 0; x < weights[i].RowCount; x++)
            {
                for (int y = 0; y < weights[i].ColumnCount; y++)
                {
                    weights[i][x, y] = Random.Range(-1f, 1f);
                }
            }
        }
        for (int i = 0; i < weights.Count; i++)
        {
            for (int y = 0; y < weights[i].ColumnCount; y++)
            {
                biases[i][0, y] = Random.Range(-1f, 1f);
            }
        }    
    }




    //would have to be changed for more inputs
    public List<float> RunNetwork(List<float> inputValues)
    {
        

        for(int i = 0; i < inputValues.Count; i++)
        {
            inputLayer[0, i] = inputValues[i];
        }
        
        //inputLayer = ActivationFunction(inputLayer);
        
        hiddenLayers[0] = ActivationFunction((inputLayer * weights[0]) + biases[0]);

        for(int i = 1; i < hiddenLayers.Count; i++)
        {
            hiddenLayers[i] = ActivationFunction((hiddenLayers[i-1] * weights[i]) + biases[i]);
        }


        outputLayer = ActivationFunction((hiddenLayers[ hiddenLayers.Count - 1] * weights[ weights.Count - 1]) + biases[ biases.Count - 1]);

        //first is accel, second is steer

        outputValues.Clear();
        for(int i = 0; i < outputLayer.ColumnCount; i++)
        {
            var outValue = ActivationFunction(outputLayer[0,i]);
            outputValues.Add(outValue);
        }
        return outputValues;
    }

    private float ActivationFunction(float s)
    {
        return (float)Math.Tanh(s);
    }

    private Matrix<float> ActivationFunction(Matrix<float> s)
    {
        //Matrix<float> exp = s * -1;
        //exp = exp.PointwiseExp();
        //exp = exp + 1;
        //return 1 / exp;

        return (s*3f).PointwiseTanh();
    }



    public NeuralNetwork InitialiseCopy(int hiddenLayerCount, int hiddenNeuronCount, int inputCount, int outputCount)
    {
        NeuralNetwork n = new NeuralNetwork();
        List<Matrix<float>> newWeights = new List<Matrix<float>>();
        for(int i = 0; i < this.weights.Count; i++)
        {
            Matrix<float> currentWeight = Matrix<float>.Build.Dense(weights[i].RowCount, weights[i].ColumnCount);
            
            for(int x = 0; x < currentWeight.RowCount; x++)
            {
                for(int y = 0; y < currentWeight.ColumnCount; y++)
                {
                    currentWeight[x,y] = weights[i][x,y];
                }
            }

            newWeights.Add(currentWeight);
        }

        List<Matrix<float>> newBiases = new List<Matrix<float>>();
        for(int i = 0; i < biases.Count; i++)
        {
            Matrix<float> currentBias = Matrix<float>.Build.Dense(1, biases[i].ColumnCount);

            for (int y = 0; y < currentBias.ColumnCount; y++)
            {
                currentBias[0, y] = biases[i][0, y];
            }

            newBiases.Add(currentBias);
        }

        n.weights = newWeights;
        n.biases = newBiases;

        n.InitialiseHidden(hiddenLayerCount, hiddenNeuronCount, inputCount, outputCount);
        return n;
    }

    void InitialiseHidden(int hiddenLayerCount, int hiddenNeuronCount, int inputCount, int outputCount)
    {
        inputLayer = Matrix<float>.Build.Dense(1,inputCount);
        outputLayer = Matrix<float>.Build.Dense(1, outputCount);

        inputLayer.Clear();
        hiddenLayers.Clear();
        outputLayer.Clear();

        for(int i = 0; i < hiddenLayerCount; i++)
        {
            Matrix<float> newHiddenLayer = Matrix<float>.Build.Dense(1, hiddenNeuronCount);
            hiddenLayers.Add(newHiddenLayer);
        }
    }
}
