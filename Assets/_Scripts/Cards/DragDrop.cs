using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DragDrop : NetworkBehaviour
{
    // public Player p;
    private GameObject board;
    private GameObject startParent;
    private GameObject dropZone;

    private Vector2 startPosition;

    public float returnDuration = 0.9f;
    public float timeStartReturn = 0f;

    [Header("Permissions")]
    [SerializeField] private bool isDraggable = false;
    private bool isOverDropZone = false;
    public bool isDragging = false;
    private bool isReturning = false;

    private void Start()
    {
        board = GameObject.Find("PlayBoard");
    }

    public void ChangeDragPermission(bool draggable)
    {
        isDraggable = draggable;
    }

    void Update()
    {
        if(!hasAuthority) return;

        if (isDragging) {
            transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            transform.SetParent(board.transform, true);
        }

        // Return card to original position
        else if (isReturning) {
            transform.position = Vector2.Lerp(transform.position, startPosition, timeStartReturn);
            timeStartReturn += Time.deltaTime;

            // If position is close enough to startPosition, then stop
            if (Vector2.Distance(transform.position, startPosition) < 0.1f) {
                transform.position = startPosition;
                transform.SetParent(startParent.transform, true);
                isReturning = false;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        isOverDropZone = true;
        dropZone = collision.gameObject;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isOverDropZone = false;
        dropZone = null;
    }

    public void StartDrag()
    {
        print("Starting drag with isDraggable: " + isDraggable);
        if (!isDraggable) return;
        if (isReturning) return;
        // if ()

        isDragging = true;
        startParent = transform.parent.gameObject;
        startPosition = transform.position;
        // Debug.Log("StartDrag:"+startPosition.ToString());
    }

    public void EndDrag()
    {
        if (!isDraggable) return;

        isDragging = false;

        if (isOverDropZone) {
            // transform.SetParent(dropZone.transform, false);
            isDraggable = false;
            NetworkIdentity networkIdentity = NetworkClient.connection.identity;
            PlayerManager p = networkIdentity.GetComponent<PlayerManager>();
            p.PlayCard(gameObject);
        } else {
            isReturning = true;
            timeStartReturn = 0f;
        }
    }
}
