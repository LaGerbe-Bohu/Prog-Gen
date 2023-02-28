using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


struct Truck
{
    public Vector2 pointCabine;
    public Vector2 followCabine;

    public Vector2 pointTruck;
    public Vector2 followTruck;
    
    public Vector2 distanceTruck;
    public Vector2 distanceCabine;

}

public class TruckSimulator : MonoBehaviour
{

    public GameObject gameObject;
    [HideInInspector] 
    private Truck truck;

    // Start is called before the first frame update
    void Start()
    {
        truck = new Truck();
        truck.followCabine = truck.distanceTruck + Vector3.up * truck.distanceCabine;
    }


    public void MoveTruck()
    {
        gameObject.transform.position = new Vector3( truck.followCabine.x, 0,truck.followCabine.y);

        Vector2 vec = truck.followCabine - truck.pointCabine;
        vec.x = Mathf.Cos( 2f)*vec.x - Mathf.Sin(2f)*vec.x;
        vec.y = Mathf.Sin(2f*vec.y) + Mathf.Cos(2f)*vec.y;
        
        truck.followCabine += vec;
    }
    
    // Update is called once per frame
    void Update()
    {   
        MoveTruck();
    }
}
