using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class spawnObjects : MonoBehaviour
{

    [SerializeField]
    private GameObject enemyAgent;
    [SerializeField]
    private GameObject bombPU;
    [SerializeField]
    private GameObject healthPackPU;

    [SerializeField]
    private GameObject LDoor;
    [SerializeField]
    private GameObject RDoor;


    [SerializeField]
    private GameObject stage1;
    [SerializeField]
    private GameObject stage2;
    [SerializeField]
    private GameObject stage3;
    [SerializeField]
    private GameObject stage4;


    private int enemyCount;
    private int enemiesDefeated;
    private float lastSpawnTime;

    // Update is called once per frame
    void Update()
    {
        Debug.Log("(spawner)spawned:"+enemyCount);
        Debug.Log("(spawner)defeated: " + enemiesDefeated);

        if (Input.GetKeyDown(KeyCode.P))
        {
            Instantiate(enemyAgent, LDoor.transform);
            enemyAgent.SetActive(true);
        }

        foreach (Transform child in LDoor.transform)
        {
            if (child.gameObject.GetComponent<enemyId>().getIsActive() == false)
            {

                if (Time.time - lastSpawnTime >= 5f)
                {
                    GameObject.Destroy(child.gameObject);
                    enemiesDefeated++;
                }
            }
        }
        foreach (Transform child in RDoor.transform)
        {

            if (child.gameObject.GetComponent<enemyId>().getIsActive() == false)
            {

                if (Time.time - lastSpawnTime >= 5f)
                {
                    GameObject.Destroy(child.gameObject);
                    enemiesDefeated++;
                }
            }
        }
    }

    public void spawnEnemy(string side)
    {
        if (side == "left")
        {
            Instantiate(enemyAgent, LDoor.transform);
            enemyCount ++;

            lastSpawnTime = Time.time;
        }
        else
        {
            Instantiate(enemyAgent, RDoor.transform);
            enemyCount ++;

            lastSpawnTime = Time.time;
        }
    }

    public void spawnPowerUp(string pu)
    {
        // select random stage spawn
        GameObject[] stages = { stage1, stage2, stage3, stage4 };
        int randomIndex = Random.Range(0, stages.Length);


        if (pu == "bomb")
        {
            Instantiate (bombPU, stages[randomIndex].transform);
        }
        else
        {
            Instantiate(healthPackPU, stages[randomIndex].transform);
        }
    }
    public void destroyAll()
    {
        foreach (Transform child in LDoor.transform)
        {
            if (child.gameObject.layer != 3)
            {
                GameObject.Destroy(child.gameObject);
                enemyCount --;
            }
        }

        foreach (Transform child in RDoor.transform)
        {
            if (child.gameObject.layer != 3)
            {
                GameObject.Destroy(child.gameObject);
                enemyCount --;
            }
        }
    }

    public int getEnemyCount()
    {
        return enemyCount;
    }

    public int getEnemiesDefeated()
    {
        return enemiesDefeated;
    }

    public void resetCounters()
    {
        enemiesDefeated = 0;
        enemyCount = 0;
        lastSpawnTime = 0;
    }


}
