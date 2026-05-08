using Local.Scripts.Extensions;
using System.Collections;
using UnityEngine;

public class LightingProp : Prop
{
    [SerializeField]
    private FloatChannelSO lightIntensityChannel;

    [SerializeField]
    private float checkRadius = 5f;

    [SerializeField]
    private int lightCount = 1;

    private Transform objectOn, objectOff;
    private bool isOn;
    private Collider[] collidersInTrigger = new Collider[4];
    private WaitForSeconds waitTime;
    private static float eplison = Mathf.Epsilon;
    private static int numLighting = 0;
    private static int numLightLimit = 2;

    private void Awake()
    {
        objectOff = transform.GetChild(0);
        objectOn = transform.GetChild(1);
        waitTime = new WaitForSeconds(RandomExtenstion.GetFloatInRange(0.9f, 1.1f));
    }

    private void OnEnable()
    {
        TurnOffLight();
    }

    private void Start()
    {
        StartCoroutine(StartLighting());
    }


    private IEnumerator StartLighting()
    {
        while (true)
        {
            yield return waitTime;
            CheckPlayer();
        }
    }

    private void CheckPlayer()
    {
        if (lightIntensityChannel.Value > eplison)
        {
            return;
        }

        int num = Physics.OverlapSphereNonAlloc(transform.position, checkRadius, collidersInTrigger, LayerMaskCash.OnlyPlayer);

        for (int i = 0; i < num; i++)
        {
            if (collidersInTrigger[i].TryGetComponent(out Player player))
            {
                TurnOnLight();
                return;
            }
        }

        TurnOffLight();
    }

    private void TurnOnLight()
    {
        if (isOn)
        {
            return;
        }

        if (numLighting >= numLightLimit)
        {
            return;
        }

        numLighting += lightCount;
        objectOn.gameObject.SetActive(true);
        objectOff.gameObject.SetActive(false);
        isOn = true;
    }

    private void TurnOffLight()
    {
        if (!isOn)
        {
            return;
        }

        objectOn.gameObject.SetActive(false);
        objectOff.gameObject.SetActive(true);
        isOn = false;
        numLighting -= lightCount;
    }

    private void OnDestroy()
    {
        if (isOn)
        {
            numLighting -= lightCount;
        }
    }
}