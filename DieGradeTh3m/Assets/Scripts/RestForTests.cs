using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestForTests : MonoBehaviour
{
    public Vector3 positionToSet;

    private void Start()
    {
        positionToSet = transform.position;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Yes");
            transform.position = positionToSet;
            transform.position = new Vector3(transform.position.x, positionToSet.y + 10, transform.position.z);
        }
    }
}
