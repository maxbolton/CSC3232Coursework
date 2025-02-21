using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class powerUpBehaviour : MonoBehaviour
{
    // Start is called before the first frame update


    private float dt;
    private float startY;

    void Start()
    {
        dt = 0f;
        startY = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {

        dt += Time.deltaTime;

        transform.position = new Vector3(transform.position.x, startY + (Mathf.Sin(dt) * 0.75f), transform.position.z);
    }

}
