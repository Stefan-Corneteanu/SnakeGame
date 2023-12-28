using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeHead : BodyPart
{
    Vector2 mvmt;
    private BodyPart tail = null;
    private const float TIMETOADDBODYPART = 0.1f;
    float addTimer = TIMETOADDBODYPART;
    public SwipeControls.SwipeDirections snakeDir = SwipeControls.SwipeDirections.NONE;

    public int partsToAdd = 0;

    List<BodyPart> parts = new List<BodyPart>();

    public AudioSource[] gulpAudios = new AudioSource[3];
    public AudioSource dieAudio = null;

    // Start is called before the first frame update
    void Start()
    {
        SwipeControls.OnSwipe += SwipeDetection;
    }

    // Update is called once per frame
    public override void Update()
    {
        if (!GameController.instance.isSnakeAlive)
        {
            return;
        }

        base.Update();
        SetMvmt(mvmt*Time.deltaTime);
        UpdateDirection();
        UpdatePos();

        if (partsToAdd > 0)
        {
            addTimer -= Time.deltaTime;
            if (addTimer <= 0)
            {
                addTimer = TIMETOADDBODYPART;
                AddBodyPart();
                partsToAdd--;
            }

        }
    }

    internal void ResetSnake()
    {
        foreach (BodyPart part in parts)
        {
            Destroy(part.gameObject);
        }
        parts.Clear();

        tail = null;
        int rand = UnityEngine.Random.Range(0, 4);
        gameObject.transform.position = new Vector3(0,0,-8f); //screen center, in front of us
        gameObject.transform.localEulerAngles = new Vector3(0, 0, 90 * rand);

        switch (rand)
        {
            case 0: MoveUp(); break;
            case 1: MoveLeft(); break;
            case 2: MoveDown(); break;
            case 3: MoveRight(); break;
        }

        ResetMemory();
        partsToAdd = 5;
        addTimer = TIMETOADDBODYPART;
    }

    void SwipeDetection(SwipeControls.SwipeDirections dir)
    {
        switch (dir)
        {
            case SwipeControls.SwipeDirections.UP:MoveUp(); break;
            case SwipeControls.SwipeDirections.DOWN: MoveDown(); break;
            case SwipeControls.SwipeDirections.LEFT: MoveLeft(); break;
            case SwipeControls.SwipeDirections.RIGHT: MoveRight(); break;
        }
    }

    void MoveUp()
    {
        if (snakeDir == SwipeControls.SwipeDirections.DOWN)
            return;
        mvmt = Vector2.up * GameController.instance.snakeSpeed;
        snakeDir = SwipeControls.SwipeDirections.UP;
    }

    void MoveDown()
    {
        if (snakeDir == SwipeControls.SwipeDirections.UP)
            return;
        mvmt = Vector2.down * GameController.instance.snakeSpeed;
        snakeDir = SwipeControls.SwipeDirections.DOWN;
    }

    void MoveLeft()
    {
        if (snakeDir == SwipeControls.SwipeDirections.RIGHT)
            return;
        mvmt = Vector2.left * GameController.instance.snakeSpeed;
        snakeDir = SwipeControls.SwipeDirections.LEFT;
    }

    void MoveRight()
    {
        if (snakeDir == SwipeControls.SwipeDirections.LEFT)
            return;
        mvmt = Vector2.right * GameController.instance.snakeSpeed;
        snakeDir = SwipeControls.SwipeDirections.RIGHT;
    }

    void AddBodyPart()
    {
        if (tail == null)
        {
            Vector3 newPos = transform.position;
            newPos.z += 0.01f;

            BodyPart newPart = Instantiate(GameController.instance.bodyPrefab,newPos,Quaternion.identity);
            newPart.following = this;
            tail = newPart;
            newPart.TurnIntoTail();
            parts.Add(newPart);
        }
        else
        {
            Vector3 newPos = tail.transform.position;
            newPos.z += 0.01f;

            BodyPart newPart = Instantiate(GameController.instance.bodyPrefab, newPos, tail.transform.rotation);
            newPart.following = tail;
            newPart.TurnIntoTail();
            tail.TurnIntoBodyPart();
            tail = newPart;
            parts.Add(newPart);
        }
    }

    private void EatEgg(Egg egg)
    {
        partsToAdd = 5;
        addTimer = 0;
        GameController.instance.EggEaten(egg);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        Egg egg = collision.GetComponent<Egg>();
        if (egg != null)
        {
            EatEgg(egg);
            int randIdx = UnityEngine.Random.Range(0, 3);
            gulpAudios[randIdx].Play();
        }
        else
        {
            GameController.instance.GameOver();
            dieAudio.Play();
        }
    }
}
