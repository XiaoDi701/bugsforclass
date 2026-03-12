using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class InteractEntry
{
    public GameObject triggerObject;
    public List<GameObject> activateObjects;
}

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
    public List<GameObject> chaosObjects;  // exactly 4 expected

    private Rigidbody2D rb;
    private float horizontal;
    private int groundCount = 0;
    private bool isGrounded => groundCount > 0;

    private HashSet<GameObject> touchingTriggers = new HashSet<GameObject>();

    // delayed input state
    private float pendingHorizontal = 0f;
    private bool pendingJump = false;
    private Coroutine delayCoroutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        int chaosLevel = GetChaosLevel();

        // --- read raw input ---
        float rawHorizontal = 0f;
        if (keyboard.aKey.isPressed) rawHorizontal = -1f;
        if (keyboard.dKey.isPressed) rawHorizontal = 1f;

        bool rawJump = keyboard.wKey.wasPressedThisFrame && isGrounded;

        // chaos 3: reverse A/D
        if (chaosLevel >= 3)
            rawHorizontal = -rawHorizontal;

        // chaos 2: 50% chance input is swallowed entirely
        if (chaosLevel >= 2 && Random.value < 0.5f)
        {
            rawHorizontal = 0f;
            rawJump = false;
        }

        // chaos 1: delay input by 1 second
        if (chaosLevel >= 1)
        {
            if (delayCoroutine != null) StopCoroutine(delayCoroutine);
            delayCoroutine = StartCoroutine(DelayInput(rawHorizontal, rawJump, 1f));
        }
        else
        {
            // no chaos — apply immediately
            horizontal = rawHorizontal;
            if (rawJump)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // --- interact ---
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

    // --- chaos level: count how many chaosObjects are active ---
    int GetChaosLevel()
    {
        int count = 0;
        foreach (var obj in chaosObjects)
            if (obj != null && obj.activeSelf) count++;
        return count;
    }

    IEnumerator DelayInput(float h, bool jump, float delay)
    {
        yield return new WaitForSeconds(delay);
        horizontal = h;
        if (jump && isGrounded)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    // --- collision ---
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