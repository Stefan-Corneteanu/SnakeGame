using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeControls : MonoBehaviour
{

    public enum SwipeDirections
    {
        UP, DOWN, LEFT, RIGHT, NONE
    };

    Vector2 swipeStart, swipeEnd;
    const float MINDIST = 10;

    public static event System.Action<SwipeDirections> OnSwipe = delegate { };

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_ANDROID && !UNITY_EDITOR //ANDROID BUILD
        foreach (Touch touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Began)
            {
                swipeStart = touch.position;
            }

            else if (touch.phase == TouchPhase.Ended)
            {
                swipeEnd = touch.position;
                ProcessSwipe();
            }
        }
#else //EDITOR or other builds (e.g. Windows)
        //mouse simulation of touches
        if (Input.GetMouseButtonDown(0))
        {
            swipeStart = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            swipeEnd = Input.mousePosition;
            ProcessSwipe();
        }
#endif
    }
    private void ProcessSwipe()
    {
        float distance = Vector2.Distance(swipeStart, swipeEnd);

        if (distance > MINDIST)
        { //it is a swipe
            if (IsVerticalSwipe())
            { //vertical swipe
                if (swipeEnd.y > swipeStart.y)
                {
                    OnSwipe(SwipeDirections.UP);
                }
                else
                {
                    OnSwipe(SwipeDirections.DOWN);
                }
            }
            else
            { //horizontal swipe
                if (swipeEnd.x > swipeStart.x)
                {
                    OnSwipe(SwipeDirections.RIGHT);
                }
                else
                {
                    OnSwipe(SwipeDirections.LEFT);
                }
            }
        }
    }

    private bool IsVerticalSwipe()
    {
        float deltaH = Mathf.Abs(swipeEnd.x - swipeStart.x);
        float deltaV = Mathf.Abs(swipeEnd.y - swipeStart.y);
        return deltaV > deltaH;
    }
}
