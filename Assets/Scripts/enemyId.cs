using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyId : MonoBehaviour
{
    [SerializeField]
    private int id;
    private bool isTargeted;
    private bool isActive;
    private GameObject controller;

    void Start()
    {
        if (controller = GameObject.Find("gameController"))
        {
            id = controller.GetComponent<gameContoller>().getEnemyId();
        }
    }

    public int getId()
    {
        return id;
    }

    public bool getTargeted()
    {
        return isTargeted;
    }
    public void setTargeted(bool Bool)
    {
        isTargeted = Bool;
    }

    public bool getIsActive()
    {
        return isActive;
    }
    public void setIsActive(bool boolean)
    {
        isActive = boolean;
    }


}
