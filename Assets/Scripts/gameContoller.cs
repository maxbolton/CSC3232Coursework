using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gameContoller : MonoBehaviour
{
    enum GameMode
    {
        gameplay,
        titleScreen,
        pauseMenu,
        endGame
    }

    GameMode gameMode = GameMode.titleScreen;

    [SerializeField]
    GameObject UI;

    [SerializeField]
    GameObject pauseUI;
    [SerializeField]
    GameObject controlInstruct;

    [SerializeField]
    GameObject titleUI;

    [SerializeField]
    GameObject GameplayUI;

    [SerializeField]
    GameObject EndGameUI;
    [SerializeField]
    GameObject endTitle;

    [SerializeField]
    GameObject controls;

    private int waveCounter = 1;
    private float playerHealth;
    private bool Iframe;

    private float lastSpawnTime;
    private float lastPUSpawnTime;
    private float spawnInterval = 2.0f;

    private int enemyId = 0;

    [SerializeField]
    playerController playerController;
    [SerializeField]
    spawnObjects spawner;

    // Start is called before the first frame update
    void Start()
    {
        StartTitleScreen();
        playerHealth = 500;
        Iframe = false;
    }

    // Update is called once per frame
    void Update()
    {

        switch (gameMode)
        {
            case GameMode.gameplay: UpdateGameplay(); break;
            case GameMode.titleScreen: UpdateTitleScreen(); break;
            case GameMode.pauseMenu: UpdatePauseMenu(); break;
            case GameMode.endGame: UpdateEndGame(); break;
        }

    }

    void UpdateGameplay()
    {
        GameObject.Find("healthCounter").GetComponent<TextMesh>().text = getPlayerHealth().ToString();
        GameObject.Find("waveCounter").GetComponent<TextMesh>().text = waveCounter.ToString();

        if (playerHealth <= 0)
        {
            endTitle.GetComponent<TextMesh>().text = "Game Over,\r\nYou Lose!";
            playerController.flop();
            StartEndGame();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StartPauseMenu();
        }
        else if(waveCounter == 4)
        {
            StartEndGame();
        }

        newWave();
    }

    void StartGameplay()
    {
        gameMode = GameMode.gameplay;
        pauseUI.gameObject.SetActive(false);
        controls.gameObject.SetActive(false);
        GameplayUI.gameObject.SetActive(true);
        titleUI.gameObject.SetActive(false);
        EndGameUI.gameObject.SetActive(false);

    }

    void UpdateTitleScreen()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            resetGame();
            StartGameplay();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            controls.gameObject.SetActive(true);
            controlInstruct.gameObject.SetActive(false);
        }
        else if (Input.GetKeyUp(KeyCode.C))
        {
            controls.gameObject.SetActive(false);
            controlInstruct.gameObject.SetActive(true);
        }
    }

    void StartTitleScreen()
    {
        gameMode = GameMode.titleScreen;
        pauseUI.gameObject.SetActive(false);
        titleUI.gameObject.SetActive(true);
        GameplayUI.gameObject.SetActive(false);
        controls.gameObject.SetActive(false);
        EndGameUI.gameObject.SetActive(false);
        Iframe = true;
        playerController.gameObject.SetActive(false);

    }

    void UpdatePauseMenu()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            controls.gameObject.SetActive(true);
            controlInstruct.gameObject.SetActive(false);
            titleUI.SetActive(false);
        }
        else if (Input.GetKeyUp(KeyCode.C))
        {
            controls.gameObject.SetActive(false);
            controlInstruct.gameObject.SetActive(true);
            titleUI.SetActive(true);
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {

            pauseUI.gameObject.SetActive(false);
            StartGameplay();
        }
        
    }

    void StartPauseMenu()
    {
        Iframe = true;
        gameMode = GameMode.pauseMenu;
        pauseUI.gameObject.SetActive(true);
        GameplayUI.gameObject.SetActive(false);
        titleUI.gameObject.SetActive(false);
    }

    void UpdateEndGame()
    {
        pauseUI.gameObject.SetActive(false);
        controls.gameObject.SetActive(false);
        GameplayUI.gameObject.SetActive(false);
        titleUI.gameObject.SetActive(false);
        EndGameUI.gameObject.SetActive(true);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            resetGame();
            StartGameplay();
        }
    }

    void StartEndGame()
    {
        Iframe = true;
        gameMode = GameMode.endGame;
    }


    private void resetGame()
    {
        playerController.gameObject.SetActive(true);
        playerController.resetFlop();
        setPlayerHealth(500);
        playerController.transform.position = Vector3.zero;
        spawner.destroyAll();
        waveCounter = 0;
        Iframe = false;
    }



    // decides how many enemies/powerups to spawn and how often
    private void newWave()
    {
        int agentCount = (waveCounter +2);

        // start next wave
        if ((spawner.getEnemyCount() == spawner.getEnemiesDefeated()) && (spawner.getEnemyCount() != 0))
        {
            if (Time.time - lastSpawnTime >= 5f)
            {
            waveCounter++;
            spawner.resetCounters();
                Debug.Log("New wave");
            }
        }
        else if (spawner.getEnemyCount() < agentCount)
        {

            //Power Up spawner
            if(Time.time - lastPUSpawnTime >= 5)
            {

                if (spawner.getEnemyCount() - spawner.getEnemiesDefeated() >= spawner.getEnemyCount() / 2)
                {
                    spawner.spawnPowerUp("bomb");
                    lastPUSpawnTime = Time.time;
                }
                else if (playerHealth <= 200)
                {
                    spawner.spawnPowerUp("health");
                    lastPUSpawnTime = Time.time;
                }
            }
            // choose where to spawn next enemy
            if (Time.time - lastSpawnTime >= spawnInterval)
            {
                string spawnSide = (Random.Range(0, 2) == 0) ? "right" : "left";
                spawner.spawnEnemy(spawnSide);

                spawnInterval = Random.Range(1.0f, 2.0f);
                lastSpawnTime = Time.time;
            }

        }

    }


    public int getEnemyId()
    {
        int id = enemyId;
        enemyId += 1;
        return id;
    }

    public void incrementPlayerHealth(float health)
    {
        playerHealth += health;
    }

    public void setPlayerHealth(float health)
    {
        playerHealth = health;
    }

    public float getPlayerHealth()
    {
        return playerHealth;
    }

    public bool getIsIframe()
    {
        return Iframe;
    }

    public void setIframe(bool boolean)
    {
        Iframe = boolean;
    }

}
