using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using UnityEngine;

public abstract class FrameStage<T> : BaseStage<T> where T : FrameStage<T>
{
    [SerializeField]
    private Transform frames;

    [SerializeField]
    protected Transform pillars;

    [SerializeField]
    protected List<Renderer> rendererToDisableWhenBlockingView = new List<Renderer>();

    [SerializeField]
    private MaterialChannelSO sideFrameMatChannel, pillarMatChannel;

    private Collider[] edgeColliders;

    private Renderer[] pillarRenderers;
    private Renderer[] sideFrameRenderers;
    private ObjectFader[] pillarFaders;

    protected virtual void Awake()
    {
        if (ReferenceEquals(pillars, null))
        {
            Debug.LogWarning("Pillars are not set.");
            return;
        }

        edgeColliders = frames.GetComponentsInChildren<BoxCollider>();

        pillarRenderers = pillars.GetComponentsInChildren<Renderer>();
        sideFrameRenderers = frames.GetComponentsInChildren<Renderer>();
        pillarFaders = pillars.GetComponentsInChildren<ObjectFader>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        SetMaterial();
    }

    protected virtual void OnDisable()
    {
        EnablePillarBlockingView();
        EnableEdgeCollider();
    }

    protected void AdjustFrameSize(int size)
    {
        for (int i = 0; i < frames.childCount; i++)
        {
            var frame = frames.GetChild(i);
            var keyAxis = Locator.GetKeyAxis(frame.localPosition);
            frame.localPosition = (size - 0.5f) * keyAxis.vertical;
            frame.localScale = (2 * size - 1) * keyAxis.horizontal + Vector3.one;
        }
    }

    protected Vector3 GetPillarSign(Transform pillar)
    {
        int xSign = pillar.localPosition.x > 0 ? 1 : -1;
        int zSign = pillar.localPosition.z > 0 ? 1 : -1;
        return new Vector3(xSign, 0, zSign);
    }

    public void DisablePillarBlockingView()
    {
        for (int i = 0; i < rendererToDisableWhenBlockingView.Count; i++)
        {
            rendererToDisableWhenBlockingView[i].enabled = false;
        }
    }

    public void EnablePillarBlockingView()
    {
        for (int i = 0; i < rendererToDisableWhenBlockingView.Count; i++)
        {
            rendererToDisableWhenBlockingView[i].enabled = true;
        }
    }

    public void DisableEdgeCollider()
    {
        for (int i = 0; i < edgeColliders.Length; i++)
        {
            edgeColliders[i].enabled = false;
        }
    }

    public void EnableEdgeCollider()
    {
        for (int i = 0; i < edgeColliders.Length; i++)
        {
            edgeColliders[i].enabled = true;
        }
    }

    private void SetMaterial()
    {
        Material pillarMaterial = pillarMatChannel.Value;
        Material sideFrameMaterial = sideFrameMatChannel.Value;

        if (ReferenceEquals(pillarMaterial, null) || ReferenceEquals(sideFrameMaterial, null))
        {
            return;
        }

        for (int i = 0; i < pillarRenderers.Length; i++)
        {
            pillarRenderers[i].sharedMaterial = pillarMaterial;
        }

        for (int i = 0; i < sideFrameRenderers.Length; i++)
        {
            sideFrameRenderers[i].sharedMaterial = sideFrameMaterial;
        }

        for (int i = 0; i < pillarFaders.Length; i++)
        {
            pillarFaders[i].opaqueMaterial = pillarMaterial;
        }
    }
}