using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class NeuralNetworkGraph : MonoBehaviour
{
    public bool randomiseNetwork;
    NeuralNetwork network;

    [Header("Network Options")]
    public int LAYERS = 1;
    public int NEURONS = 3;
    public int INPUT_COUNT = 2;
    public int OUTPUT_COUNT = 2;

    [Header("Visuals Options")]
    public float graphHeight;
    float distanceBetweenNodes;
    public float graphWidth;
    float distanceBetweenLayers;
    public float nodeSize;
    public Gradient nodeGradient;
    public Gradient weightGradient;
    public GameObject nodePrefab;
    public GameObject linePrefab;


    public GameObject[][] nodeObjects;
    public GameObject[][][] weightObjects;

    [Range(-1f, 1f)] public List<float> inputValues = new List<float>();
    List<float> currentInputValues = new List<float>();


    private void Update()
    {

        if (randomiseNetwork)
        {
            randomiseNetwork = false;
            NewGraph();

        }
        UpdateGraph();

    }

    private void Awake()
    {
        NewGraph();
    }

    void NewGraph()
    {
        network = new NeuralNetwork();
        network.Initialise(LAYERS, NEURONS, INPUT_COUNT, OUTPUT_COUNT);
        InitialiseGraph();
        network.RandomiseNetwork();
        UpdateGraph();
    }

    void UpdateGraph()
    {
        network.RunNetwork(inputValues);
        RenderGraph();
    }

    void InitialiseGraph()
    {
        //init nodes
        nodeObjects = new GameObject[LAYERS + 2][];
        for (int i = 0; i < LAYERS + 2; i++)
        {
            if (i == 0)
            {
                nodeObjects[i] = new GameObject[INPUT_COUNT];
                for (int j = 0; j < INPUT_COUNT; j++)
                {
                    GameObject newNode = Instantiate(nodePrefab);
                    newNode.transform.SetParent(transform, false);
                    nodeObjects[i][j] = newNode;
                }
            }
            else if (i == LAYERS + 1)
            {
                nodeObjects[i] = new GameObject[OUTPUT_COUNT];
                for (int j = 0; j < OUTPUT_COUNT; j++)
                {
                    GameObject newNode = Instantiate(nodePrefab);
                    newNode.transform.SetParent(transform, false);
                    nodeObjects[i][j] = newNode;
                }

            }
            else
            {
                nodeObjects[i] = new GameObject[NEURONS];
                for (int j = 0; j < NEURONS; j++)
                {
                    GameObject newNode = Instantiate(nodePrefab);
                    newNode.transform.SetParent(transform, false);
                    nodeObjects[i][j] = newNode;
                }
            }
        }

        //init weight lines
        weightObjects = new GameObject[LAYERS + 1][][];
        for (int i = 0; i < LAYERS + 1; i++)
        {
            if (i == 0)
            {
                weightObjects[i] = new GameObject[INPUT_COUNT][];
                for (int j = 0; j < INPUT_COUNT; j++)
                {
                    weightObjects[i][j] = new GameObject[NEURONS];
                    for (int k = 0; k < NEURONS; k++)
                    {
                        GameObject newLine = Instantiate(linePrefab);
                        newLine.transform.SetParent(transform, false);
                        weightObjects[i][j][k] = newLine;
                    }
                }
            }
            else if (i == LAYERS)
            {
                weightObjects[i] = new GameObject[NEURONS][];
                for (int j = 0; j < NEURONS; j++)
                {
                    weightObjects[i][j] = new GameObject[OUTPUT_COUNT];
                    for (int k = 0; k < OUTPUT_COUNT; k++)
                    {
                        GameObject newLine = Instantiate(linePrefab);
                        weightObjects[i][j][k] = newLine;
                        newLine.transform.SetParent(transform, false);
                    }
                }
            }
            else
            {
                weightObjects[i] = new GameObject[NEURONS][];
                for (int j = 0; j < NEURONS; j++)
                {
                    weightObjects[i][j] = new GameObject[NEURONS];
                    for (int k = 0; k < NEURONS; k++)
                    {
                        GameObject newLine = Instantiate(linePrefab);
                        weightObjects[i][j][k] = newLine;
                        newLine.transform.SetParent(transform, false);
                    }
                }
            }
        }

    }

    void RenderGraph()
    {
        for (int i = 0; i < nodeObjects.Length; i++)
        {
            for (int j = 0; j < nodeObjects[i].Length; j++)
            {
                distanceBetweenNodes = graphHeight / (nodeObjects[i].Length + 1);
                float vertical = (graphHeight / 2) - ((j+1) * distanceBetweenNodes);
                distanceBetweenLayers = graphWidth / (nodeObjects.Length + 1);
                float horizontal = ((i + 1) * distanceBetweenLayers) - (graphWidth / 2);
                Vector3 pos = new Vector3(horizontal, vertical, 0f);
                GameObject newNode = nodeObjects[i][j];
                newNode.transform.localPosition = pos;
                newNode.transform.localScale = new Vector3(nodeSize, nodeSize, 1);
                float value = GetNodeValue(i, j);
                


                newNode.GetComponentInChildren<TextMeshProUGUI>().text = "" + value;
                newNode.GetComponent<SpriteRenderer>().color = GetNodeColor(value);
            }
        }
        for (int i = 0; i < weightObjects.Length; i++)
        {
            for (int j = 0; j < weightObjects[i].Length; j++)
            {
                for (int k = 0; k < weightObjects[i][j].Length; k++)
                {
                    distanceBetweenNodes = graphHeight / (weightObjects[i].Length + 1);
                    float vertical1 = (graphHeight / 2) - ((j + 1) * distanceBetweenNodes);
                    distanceBetweenNodes = graphHeight / (weightObjects[i][j].Length + 1);
                    float vertical2 = (graphHeight / 2) - ((k + 1) * distanceBetweenNodes);
                    distanceBetweenLayers = graphWidth / (weightObjects.Length + 2);
                    float horizontal1 = ((i + 1) * distanceBetweenLayers) - (graphWidth / 2);
                    float horizontal2 = ((i + 2) * distanceBetweenLayers) - (graphWidth / 2) ;

                    Vector3 pos1 = new Vector3(horizontal1, vertical1, 0);
                    Vector3 pos2 = new Vector3(horizontal2, vertical2, 0);
                    GameObject line = weightObjects[i][j][k];
                    line.transform.position = pos1;
                    line.transform.SetParent(transform, false);
                    line.GetComponent<LineRenderer>().SetPosition(0, transform.position + pos1);
                    line.GetComponent<LineRenderer>().SetPosition(1, transform.position + pos2);
                    line.GetComponent<LineRenderer>().startColor = GetWeightColor(network.weights[i][j, k]);
                    line.GetComponent<LineRenderer>().endColor = GetWeightColor(network.weights[i][j, k]);

                }
            }
        }

    }

    float GetNodeValue(int fromLayer, int node)
    {
        float unrounded;
        if (fromLayer == 0)
        {
            unrounded = network.inputLayer[0, node];
        }
        else if (fromLayer == nodeObjects.Length - 1)
        {
            unrounded = network.outputLayer[0, node];
        }
        else
        {
            unrounded = network.hiddenLayers[fromLayer - 1][0, node];
        }
        float rounded = Mathf.Round(100 * unrounded) / 100;
        return rounded;
    }

    void ClearGraph()
    {
        //clear nodes
        for (int i = 0; i < nodeObjects.Length; i++)
        {
            for (int j = 0; j < nodeObjects[i].Length; j++)
            {
                Destroy(nodeObjects[i][j]);
            }

        }

        for (int i = 0; i < weightObjects.Length; i++)
        {
            for (int j = 0; j < weightObjects[i].Length; j++)
            {
                for (int k = 0; k < weightObjects[i][j].Length; k++)
                {
                    Destroy(weightObjects[i][j][k]);
                }
            }
        }

        //clear weights
    }

    Color GetNodeColor(float value)
    {
        value = (value + 1) / 2f;
        return nodeGradient.Evaluate(value);
    }

    Color GetWeightColor(float value)
    {
        value = (value + 1) / 2f;
        return weightGradient.Evaluate(value);
    }
}


