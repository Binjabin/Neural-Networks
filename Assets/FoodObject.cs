using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodObject : MonoBehaviour
{
    public void Eaten()
    {
        FindObjectOfType<BacteriaGeneticManager>().DeleteFood(this);
    }
}
