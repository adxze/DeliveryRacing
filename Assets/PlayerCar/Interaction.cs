using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Interaction : MonoBehaviour
{
    private carNew _carNew; 
    private bool _hasPakage = false;

    private void Awake()
    {
        _carNew = GetComponent<carNew>();
    }

    private void OnTriggerEnter2D(Collider2D item)
    {
        if (item.CompareTag("Item") && !_hasPakage)
        {
            Debug.Log("item received");
            _hasPakage = true;
        }

        if (item.CompareTag("Point") && _hasPakage)
        {
            Debug.Log("point recieved");
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        Debug.Log("Collision enter");
    }
}
