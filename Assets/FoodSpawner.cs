using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    [SerializeField] GameObject foodPrefab;
    [SerializeField] float maxDistanceFromCenter = 10f;
    [SerializeField] int numberToSpawn = 100;
    List<FoodObject> foodList = new List<FoodObject>();
    // Start is called before the first frame update

    void SpawnFood()
    {
        Vector3 pos = new Vector3(Random.Range(-maxDistanceFromCenter, maxDistanceFromCenter), Random.Range(-maxDistanceFromCenter, maxDistanceFromCenter), 0f);
        GameObject foodObject = Instantiate(foodPrefab, pos, Quaternion.identity);
        foodObject.transform.parent = transform;
        foodList.Add(foodObject.GetComponent<FoodObject>());
    }
    
    public void DeleteFood(FoodObject food)
    {
        foodList.Remove(food);
        Destroy(food.gameObject);
        SpawnFood();
    }

    public void ClearFood()
    {
        for(int i = 0; i < foodList.Count; i++)
        {
            Destroy(foodList[i].gameObject);
        }
        foodList.Clear();
    }

    public void FillFood()
    {
        for(int i = 0; i < numberToSpawn; i++)
        {
            SpawnFood();
        }
    }

}
