using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [Header("Interact")]
    public List<InteractEntry> interactEntries;

    [Header("Chaos Trackers")]
    [Tooltip("Drag the 4 objects whose active state drives the chaos effects.")]
    public List<GameObject> chaosObjects;

    private Rigidbody2D rb;
    private float horizontal;
    private int groundCount = 0;
    private bool isGrounded => groundCount > 0;

    private HashSet<GameObject> touchingTriggers = new HashSet<GameObject>();

    private bool lastKeyA = false;
    private bool lastKeyD = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        int chaosLevel = GetChaosLevel();

        float rawHorizontal = 0f;
        if (keyboard.aKey.isPressed) rawHorizontal = -1f;
        if (keyboard.dKey.isPressed) rawHorizontal = 1f;

        bool rawJump = keyboard.wKey.wasPressedThisFrame && isGrounded;

        // chaos 1: reverse A/D
        if (chaosLevel >= 1)
            rawHorizontal = -rawHorizontal;

        // chaos 2: 30% chance input is swallowed
        if (chaosLevel >= 2 && Random.value < 0.3f)
        {
            rawHorizontal = 0f;
            rawJump = false;
        }

        // chaos 3: delay input by 0.3s
        if (chaosLevel >= 3)
        {
            bool currentA = keyboard.aKey.isPressed;
            bool currentD = keyboard.dKey.isPressed;
            bool inputChanged = (currentA != lastKeyA) || (currentD != lastKeyD);

            if (inputChanged)
            {
                lastKeyA = currentA;
                lastKeyD = currentD;
                StartCoroutine(DelayInput(rawHorizontal, 0.3f));
            }

            if (rawJump)
                StartCoroutine(DelayJump(0.3f));
        }
        else
        {
            horizontal = rawHorizontal;
            if (rawJump)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // interact
        if (keyboard.fKey.wasPressedThisFrame && touchingTriggers.Count > 0)
        {
            foreach (var entry in interactEntries)
            {
                if (entry.triggerObject == null) continue;
                if (!touchingTriggers.Contains(entry.triggerObject)) continue;

                foreach (var obj in entry.activateObjects)
                    if (obj != null) obj.SetActive(true);
            }
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocity.y);
    }

    int GetChaosLevel()
    {
        int count = 0;
        foreach (var obj in chaosObjects)
            if (obj != null && obj.activeSelf) count++;
        return count;
    }

    IEnumerator DelayInput(float h, float delay)
    {
        yield return new WaitForSeconds(delay);
        horizontal = h;
    }

    IEnumerator DelayJump(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (isGrounded)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        groundCount++;
        foreach (var entry in interactEntries)
        {
            if (entry.triggerObject == col.gameObject)
            {
                touchingTriggers.Add(col.gameObject);
                break;
            }
        }
    }

    void OnCollisionExit2D(Collision2D col)
    {
        groundCount--;
        touchingTriggers.Remove(col.gameObject);
    }
}