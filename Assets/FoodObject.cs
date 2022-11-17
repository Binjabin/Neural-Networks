using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodObject : MonoBehaviour
{
    public bool isEaten = false;
    public void TryEat(BlobController blob)
    {
        if(!isEaten)
        {
            isEaten = true;
            blob.Eat(gameObject);
        }

    }
}
