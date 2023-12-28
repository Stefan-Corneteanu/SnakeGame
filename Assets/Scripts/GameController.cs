using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public static GameController instance = null;
    public float snakeSpeed = 0;
    public BodyPart bodyPrefab = null;
    public GameObject rockPrefab = null;
    public GameObject eggPrefab = null;
    public GameObject goldenEggPrefab = null;
    public GameObject spikePrefab = null;

    public SnakeHead snakeHead = null;

    public Sprite tailSprite = null;
    public Sprite bodySprite = null;

    public bool isSnakeAlive = true;
    public bool waitingForPlayer = true;

    public int score = 0;
    public int highScore = 0;

    public Text scoreText = null;
    public Text hiScoreText = null;
    public Text lvlText = null;
    public Text GameOverText = null;
    public Text TapToPlayText = null;

    List<Egg> eggs = new List<Egg>();
    List<GameObject> spikes = new List<GameObject>();

    int lvl = 0;
    int noEggsLvlProgression = 0;
    int noSpikes = 0;

    const float WIDTH = 3.5f;
    const float HEIGHT = 7;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        CreateWalls();
        isSnakeAlive = false;
    }

    // Update is called once per frame
    void Update()
    {

    if (Input.GetKeyDown(KeyCode.Escape))
    {
#if UNITY_ANDROID && !UNITY_EDITOR //ANDROID BUILD
        AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
        activity.Call<bool>("moveTaskToBack", true);
#else //EDITOR or other builds (e.g. Windows)
            EditorApplication.isPlaying = false;
#endif
    }

    if (waitingForPlayer)
    {
#if UNITY_ANDROID && !UNITY_EDITOR //ANDROID BUILD
        foreach (Touch touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Ended)
            {
                StartGameplay();
            }
        }
#else //EDITOR or other builds (e.g. Windows)
        if (Input.GetMouseButtonUp(0))
        {
            StartGameplay();
        }
#endif
    }
}

    public void GameOver()
    {
        isSnakeAlive = false;
        waitingForPlayer = true;
        GameOverText.gameObject.SetActive(true);
        TapToPlayText.gameObject.SetActive(true);
    }

    void StartGameplay()
    {
        GameOverText.gameObject.SetActive(false);
        TapToPlayText.gameObject.SetActive(false);
        score = 0;
        scoreText.text = "Score: " + score;
        lvl = 0;
        noSpikes = 0;
        waitingForPlayer = false;
        isSnakeAlive = true;
        KillOldEggs();
        LvlUp();
    }

    void LvlUp()
    {
        lvl++;
        lvlText.text = "Level: " + lvl;
        noEggsLvlProgression = 4 + lvl*2;
        noSpikes++;
        snakeSpeed = 1f + lvl/4f;
        if (snakeSpeed > 6)
        {
            snakeSpeed = 6;
        }
        snakeHead.ResetSnake();

        KillOldSpikes();
        for (int i = 0; i < noSpikes; i++)
        {
            CreateSpike();
        }

        CreateEgg(); //moved this one last so that it checks at creation that it doesnt spawn on a spike
    }

    void CreateWalls()
    {
        //left wall
        Vector3 start = new Vector3(-WIDTH,-HEIGHT,-0.01f);
        Vector3 end = new Vector3(-WIDTH, HEIGHT, -0.01f);
        CreateWall(start, end);

        //right wall
        start = new Vector3(WIDTH, -HEIGHT, -0.01f);
        end = new Vector3(WIDTH, HEIGHT, -0.01f);
        CreateWall(start, end);

        //top wall
        start = new Vector3(-WIDTH, HEIGHT, -0.01f);
        end = new Vector3(WIDTH, HEIGHT, -0.01f);
        CreateWall(start, end);

        //bottom wall
        start = new Vector3(-WIDTH, -HEIGHT, -0.01f);
        end = new Vector3(WIDTH, -HEIGHT, -0.01f);
        CreateWall(start, end);
    }

    void CreateWall(Vector3 startPos, Vector3 endPos)
    {
        float dist = Vector3.Distance(startPos, endPos);
        int noRocks = (int)(dist * 3);
        Vector3 delta = (endPos - startPos)/noRocks;

        Vector3 pos = startPos;
        for (int i = 0; i < noRocks; i++)
        {
            float rot = Random.Range(0, 360f);
            float scale = Random.Range(1.5f, 2f);
            CreateRock(pos, scale, rot);
            pos += delta;
        }
    }

    void CreateRock(Vector3 pos, float scale, float rot)
    {
        GameObject rock = Instantiate(rockPrefab,pos,Quaternion.Euler(0,0,rot));
        rock.transform.localScale = new Vector3(scale, scale, 1);
    }

    void CreateEgg(bool isGolden = false)
    {
        Vector3 pos;
        pos.x = -WIDTH + Random.Range(1, 2 * WIDTH - 2);
        pos.y = -HEIGHT + Random.Range(1, 2 * HEIGHT - 2);
        pos.z = -0.01f;

        //do not spawn the egg on the snake head or on any spike
        bool safePosition = false;
        while (!safePosition)
        {
            safePosition = true;
            //check snake head
            if (pos.x == snakeHead.transform.position.x && pos.y == snakeHead.transform.position.y)
            {
                //generate new position and reiterate
                safePosition = false;
                pos.x = -WIDTH + Random.Range(1, 2 * WIDTH - 2);
                pos.y = -HEIGHT + Random.Range(1, 2 * HEIGHT - 2);
                continue;
            }

            //check spikes
            foreach (GameObject spike in spikes)
            {
                if (pos.x == spike.transform.position.x && pos.y == spike.transform.position.y)
                {
                    //generate new position and reiterate
                    safePosition = false;
                    pos.x = -WIDTH + Random.Range(1, 2 * WIDTH - 2);
                    pos.y = -HEIGHT + Random.Range(1, 2 * HEIGHT - 2);
                    break;
                }
            }
        }

        Egg egg = null;
        if (isGolden)
        {
            egg = Instantiate(goldenEggPrefab, pos, Quaternion.identity).GetComponent<Egg>();
        }
        else
        {
            egg = Instantiate(eggPrefab, pos,Quaternion.identity).GetComponent<Egg>();
        }

        eggs.Add(egg);
        
    }

    void CreateSpike()
    {
        Vector3 pos;
        pos.x = -WIDTH + Random.Range(1, 2 * WIDTH - 2);
        pos.y = -HEIGHT + Random.Range(1, 2 * HEIGHT - 2);
        pos.z = -0.01f;

        //do not spawn a spike on the snake head (body is acceptable), or other spikes
        bool safePosition = false;
        while (!safePosition)
        {
            safePosition = true;

            //check proximity to snake head
            if (Vector2.Distance(snakeHead.transform.position,pos) < 1)
            {
                //generate new position and reiterate
                safePosition = false;
                pos.x = -WIDTH + Random.Range(1, 2 * WIDTH - 2);
                pos.y = -HEIGHT + Random.Range(1, 2 * HEIGHT - 2);
                continue;
            }

            //check overlapping with other spikes
            foreach (GameObject s in spikes)
            {
                if (pos.x == s.transform.position.x && pos.y == s.transform.position.y)
                {
                    //generate new position and reiterate
                    safePosition = false;
                    pos.x = -WIDTH + Random.Range(1, 2 * WIDTH - 2);
                    pos.y = -HEIGHT + Random.Range(1, 2 * HEIGHT - 2);
                    break;
                }
            }
        }

        GameObject spike = Instantiate(spikePrefab, pos, Quaternion.identity);
        spikes.Add(spike);
    }

    void KillOldEggs()
    {
        foreach (Egg egg in eggs)
        {
            Destroy(egg.gameObject);
        }

        eggs.Clear();
    }

    void KillOldSpikes()
    {
        foreach (GameObject spike in spikes)
        {
            Destroy(spike);
        }
        spikes.Clear();
    }

    public void EggEaten(Egg egg)
    {
        score++;
        noEggsLvlProgression--;
        if (noEggsLvlProgression == 0)
        {
            score += 10;
            LvlUp();
        }
        else if (noEggsLvlProgression == 1)
        {
            CreateEgg(true); //golden egg
        }
        else
        {
            CreateEgg(); //normal egg
        }

        if (highScore < score)
        {
            highScore = score;
        }
        scoreText.text = "Score: " + score;
        hiScoreText.text = "HiScore: " + highScore;
        eggs.Remove(egg);
        Destroy(egg.gameObject);
    }
}
