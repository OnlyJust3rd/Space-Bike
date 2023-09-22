using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Formation
{
    public float multiplier = 1;
    public GameObject[] prefabs;
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private void Awake()
    {
        instance = this;
    }

    public Formation[] formations;
    public GameObject[] bosses;
    public int difficulty = 0;
    public Text scoreLabel;
    public int bossSpawnInterval = 10;
    public Text fpsLabel;
    public GameObject bossLabel;

    public GameObject itemBase;

    private int score = 0, currentSpawnInterval;
    private bool isWaitForSpawn;

    private void Start()
    {
        // StartCoroutine(SpawnEnemy());
        FindObjectOfType<AudioManager>().Play("bgm");
    }
    public float deltaTime;
    private void Update()
    {
        CheckForSpawn();

        //deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        //float fps = 1.0f / deltaTime;
        //fpsLabel.text = $"FPS:{Mathf.Ceil(fps)}";
    }

    private void CheckForSpawn()
    {
        int n = FindObjectsOfType<EnemyController>().Length;
        if (n <= 0 && !isWaitForSpawn)
        {
            isWaitForSpawn = true;

            if (currentSpawnInterval < bossSpawnInterval) 
            {
                Invoke("SpawnEnemy", 3f);
                currentSpawnInterval++;
                print(bossSpawnInterval);
            }
            else
            {
                Invoke("SpawnBoss", 3f);
                currentSpawnInterval = 0;
            }
        }
    }

    private void SpawnEnemy()
    {
        // float randx = Random.Range(-3.5f, 3.5f);
        Vector2 spawnPos = Vector2.up * 6.1f;

        int e = Random.Range(0, formations[difficulty].prefabs.Length - 1);
        if (currentSpawnInterval % 4 == 3) e = formations[difficulty].prefabs.Length - 1;

        GameObject newEnemy = Instantiate(formations[difficulty].prefabs[e], spawnPos, Quaternion.identity);
        foreach(Transform child in newEnemy.transform)
        {
            child.GetComponent<EnemyController>().enemyInfo.maxHp *= formations[difficulty].multiplier;
        }
        // newEnemy.GetComponent<EnemyController>().movementSpeed = Random.Range(9, 12);
        isWaitForSpawn = false;
    }

    private void SpawnBoss()
    {
        FindObjectOfType<AudioManager>().Stop("bgm");
        FindObjectOfType<AudioManager>().Play("boss music");

        Vector2 spawnPos = Vector2.up * 7.5f;
        int b = Random.Range(0, bosses.Length);

        GameObject newEnemy = Instantiate(bosses[b], spawnPos, Quaternion.identity);
        newEnemy.name = bosses[b].name;

        bosses[b].transform.GetChild(0).GetComponent<EnemyController>().enemyInfo.maxHp = 4000;
        bosses[b].transform.GetChild(0).GetComponent<EnemyController>().enemyInfo.cooldown *= Mathf.Abs(.6f / formations[difficulty].multiplier);

        isWaitForSpawn = false;
    }

    public float scoreMultiplier = 1f;
    public void GivePoint(int p)
    {
        score += Mathf.CeilToInt(p * scoreMultiplier);
        scoreLabel.text = $"{score}";

        scoreLabel.GetComponent<Animator>().SetTrigger("GetScore");
    }

    public void ResetScoreMultiplier()
    {
        scoreMultiplier = 1;
    }

    public void LoadScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
