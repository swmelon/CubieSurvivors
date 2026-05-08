using StarterAssets;
using System.Collections;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;

public class TeleportTrigger : MonoBehaviour
{
    [SerializeField]
    private TeleportTrigger destination;

    private Collider collider;
    private WaitForSeconds delay = new WaitForSeconds(3f);
    private WaitForSeconds hideDelay = new WaitForSeconds(1f);
    private ParticleSystem.MainModule[] mainModules;
    private Color[] startColors;
    private Color[] zeroAlphaColors;
    private bool hideParticles = false;
    private float timeElapsed = 0f;

    private void Awake()
    {
        collider = GetComponent<Collider>();
        mainModules = GetComponentsInChildren<ParticleSystem>().Select(p => p.main).ToArray();
        startColors = mainModules.Select(m => m.startColor.color).ToArray();
        zeroAlphaColors = startColors.Select(c => new Color(c.r, c.g, c.b, 0f)).ToArray();
    }

    private void OnEnable()
    {
        hideParticles = false;
        collider.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Player player) && player.enabled
            && player.TryGetComponent(out CustomThirdPersonController controller))
        {
            DisableTriggerForSec();
            destination.DisableTriggerForSec();
            controller.MoveOnlyCharacterTo(destination.transform.position);
        }
    }

    private void Update()
    {
        timeElapsed += Time.deltaTime;

        if (hideParticles)
        {
            for (int i = 0; i < mainModules.Length; i++)
            {
                mainModules[i].startColor = Color.Lerp(startColors[i], zeroAlphaColors[i], timeElapsed);
            }
        }
        else
        {
            for (int i = 0; i < mainModules.Length; i++)
            {
                mainModules[i].startColor = Color.Lerp(zeroAlphaColors[i], startColors[i], timeElapsed);
            }
        }
    }

    private void DisableTriggerForSec()
    {
        StartCoroutine(EnableColliderAfterDelay());
    }

    private IEnumerator EnableColliderAfterDelay()
    {
        hideParticles = true;
        collider.enabled = false;
        yield return delay;
        hideParticles = false;
        yield return hideDelay;
        collider.enabled = true;
    }
}
