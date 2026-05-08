using UnityEngine;

public class StepableProp : MonoBehaviour
{
    // if the player is stepping on this prop
    // shirink y scale
    // time pass, return to original scale

    private enum AnimationMode
    {
        None,
        Shrink,
        Wait,
        Return
    }

    private float originalYScale;
    private float shirinkYScale = 0.2f;
    private float shrinkTime = 0.2f;
    private float returnWaitTime = 3f;
    private float returnTime = 1f;
    private float time;
    private AnimationMode animationMode;

    private void Awake()
    {
        originalYScale = transform.localScale.y;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            animationMode = AnimationMode.Shrink;
            time = 0f;
        }
    }

    private void Update()
    {
        switch (animationMode)
        {
            case AnimationMode.Shrink:
                Shrink();
                break;
            case AnimationMode.Wait:
                Wait();
                break;
            case AnimationMode.Return:
                Return();
                break;
        }
    }

    private void Shrink()
    {
        time += Time.deltaTime;
        float t = time / shrinkTime;
        float yScale = Mathf.Lerp(originalYScale, shirinkYScale, t);
        transform.localScale = new Vector3(transform.localScale.x, yScale, transform.localScale.z);

        if (t >= 1f)
        {
            animationMode = AnimationMode.Wait;
            time = 0f;
        }
    }

    private void Wait()
    {
        time += Time.deltaTime;
        if (time >= returnWaitTime)
        {
            animationMode = AnimationMode.Return;
            time = 0f;
        }
    }

    private void Return()
    {
        time += Time.deltaTime;
        float t = time / returnTime;
        float yScale = Mathf.Lerp(shirinkYScale, originalYScale, t);
        transform.localScale = new Vector3(transform.localScale.x, yScale, transform.localScale.z);

        if (t >= 1f)
        {
            animationMode = AnimationMode.None;
        }
    }
}