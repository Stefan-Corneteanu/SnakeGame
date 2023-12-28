using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyPart : MonoBehaviour
{
    Vector2 deltaPos;

    public BodyPart following = null;
    private SpriteRenderer spriteRenderer = null;
    private const int PARTSREMEMBERED = 10;
    public Vector3[] prevPoses = new Vector3[PARTSREMEMBERED];

    public int setIndex = 0;
    public int getIndex = -(PARTSREMEMBERED-1);

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public virtual void Update()
    {
        if (!GameController.instance.isSnakeAlive)
        {
            return;
        }

        Vector3 followPos;
        if (following != null)
        {
            if (following.getIndex > -1)
            {
                followPos = following.prevPoses[following.getIndex];
            }
            else
            {
                followPos = following.transform.position;
            }
        }
        else
        {
            followPos = this.transform.position;
        }

        prevPoses[setIndex] = gameObject.transform.position;

        //increment indices, ensure they stay within circular buffer's bounds
        setIndex = (setIndex + 1) % PARTSREMEMBERED;
        getIndex = (getIndex + 1) % PARTSREMEMBERED;

        if (following != null)
        {
            Vector3 newPos;
            if (following.getIndex > -1)
            {
                newPos = followPos;
            }
            else
            {
                newPos = following.transform.position;
            }

            newPos.z += 0.01f;
            SetMvmt(newPos - gameObject.transform.position);
            UpdateDirection();
            UpdatePos();
        }
    }

    public void ResetMemory()
    {
        setIndex = 0;
        getIndex = -(PARTSREMEMBERED - 1);
    }

    public void SetMvmt(Vector2 mvmt)
    {
        deltaPos = mvmt;
    }

    public void UpdatePos()
    {
        gameObject.transform.position += (Vector3)deltaPos;
    }

    public void UpdateDirection()
    {
        if (deltaPos.y > 0)
        {
            gameObject.transform.localEulerAngles = new Vector3(0,0,0);
        }
        else if (deltaPos.y < 0)
        {
            gameObject.transform.localEulerAngles = new Vector3(0, 0, 180);
        }
        else if (deltaPos.x < 0)
        {
            gameObject.transform.localEulerAngles = new Vector3(0, 0, 90);
        }
        else if (deltaPos.x > 0)
        {
            gameObject.transform.localEulerAngles = new Vector3(0, 0, -90);
        }
    }

    internal void TurnIntoTail()
    {
        spriteRenderer.sprite = GameController.instance.tailSprite;
    }

    internal void TurnIntoBodyPart()
    {
        spriteRenderer.sprite = GameController.instance.bodySprite;
    }
}
