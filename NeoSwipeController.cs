using UnityEngine;
using UnityEngine.Events;

public interface ISwipeSubscriber
{
    void SubscribeToSwipe(SwipeData swipeData);
}

/// <summary>
/// SwipeController is responsible for detecting swipe gestures on both touch devices and PCs.
/// It can differentiate between swipes in all four cardinal directions and trigger events accordingly.
/// </summary>
public class NeoSwipeController : MonoBehaviour
{
    /// <summary>
    /// Determines if the swipe detection is currently active. If set to false, no swipe input will be processed.
    /// </summary>
    [SerializeField]
    public bool interactable = true;
    
    /// <summary>
    /// If true, swipes are only detected after the user has released the touch or mouse button. This can be useful
    /// for games or applications where swipes are meant to be deliberate actions rather than continuous inputs.
    /// </summary>
    [SerializeField]
    public bool detectSwipeOnlyAfterRelease = false;
    /// <summary>
    /// The minimum distance (in screen pixels) that the user must move their finger or the mouse for a movement
    /// to be considered a swipe. This helps differentiate between swipes and taps or clicks.
    /// </summary>
    [SerializeField]
    private float minDistanceForSwipe = 20f;

    private Vector2 fingerDownPosition;
    private Vector2 fingerUpPosition;

    private bool swipeCompleted = true;
    private SwipeDirection lastSwipeDirection;
    private static NeoSwipeController instance;

    public UnityEvent<SwipeData> OnSwipe = new UnityEvent<SwipeData>();


    public static NeoSwipeController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<NeoSwipeController>();
            }
            return instance;
        }
    }

    private void Update()
    {
        if (interactable)
        {
            if (Input.touchSupported && Input.touchCount > 0)
            {
                HandleTouch();
            }
            else
            {
                HandleMouse();
            }
        }   
    }

    private void HandleTouch()
    {
        foreach (Touch touch in Input.touches)
        {
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    fingerUpPosition = touch.position;
                    fingerDownPosition = touch.position;
                    break;

                case TouchPhase.Moved:
                    if (!detectSwipeOnlyAfterRelease)
                    {
                        fingerDownPosition = touch.position;
                        DetectSwipe();
                    }
                    break;

                case TouchPhase.Ended:
                    fingerDownPosition = touch.position;
                    DetectSwipe();
                    swipeCompleted = true;
                    break;
            }
        }
    }

    private void HandleMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            fingerDownPosition = Input.mousePosition;
            fingerUpPosition = Input.mousePosition;
        }

        if (!detectSwipeOnlyAfterRelease && Input.GetMouseButton(0))
        {
            fingerDownPosition = Input.mousePosition;
            DetectSwipe();
        }

        if (Input.GetMouseButtonUp(0))
        {
            fingerDownPosition = Input.mousePosition;
            DetectSwipe();
            swipeCompleted = true;
        }
    }

    private void DetectSwipe()
    {
        if (SwipeDistanceCheckMet())
        {
            if (IsVerticalSwipe())
            {
                var direction = fingerDownPosition.y - fingerUpPosition.y > 0 ? SwipeDirection.Up : SwipeDirection.Down;
                if (swipeCompleted || lastSwipeDirection != direction)
                {
                    swipeCompleted = false;
                    lastSwipeDirection = direction;
                    SendSwipe(direction);
                }
            }
            else
            {
                var direction = fingerDownPosition.x - fingerUpPosition.x > 0 ? SwipeDirection.Right : SwipeDirection.Left;
                if (swipeCompleted || lastSwipeDirection != direction)
                {
                    swipeCompleted = false;
                    lastSwipeDirection = direction;
                    SendSwipe(direction);
                }
            }
            fingerUpPosition = fingerDownPosition;
        }
    }

    void SendSwipe(SwipeDirection direction)
    {
        SwipeData swipeData = new SwipeData()
        {
            Direction = direction,
            StartPosition = fingerDownPosition,
            EndPosition = fingerUpPosition
        };
        Debug.Log(swipeData.Direction);
        OnSwipe?.Invoke(swipeData);
    }

    private bool IsVerticalSwipe()
    {
        return VerticalMovementDistance() > HorizontalMovementDistance();
    }

    private bool SwipeDistanceCheckMet()
    {
        return VerticalMovementDistance() > minDistanceForSwipe
            || HorizontalMovementDistance() > minDistanceForSwipe;
    }

    private float VerticalMovementDistance()
    {
        return Mathf.Abs(fingerDownPosition.y - fingerUpPosition.y);
    }

    private float HorizontalMovementDistance()
    {
        return Mathf.Abs(fingerDownPosition.x - fingerUpPosition.x);
    }
}

public struct SwipeData
{
    public Vector2 StartPosition;
    public Vector2 EndPosition;
    public SwipeDirection Direction;
}

public enum SwipeDirection
{
    Up,
    Down,
    Left,
    Right
}