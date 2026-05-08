using UnityEngine;

public class StageAdapter : FrameStage<StageAdapter>
{
    [SerializeField]
    private Transform topBorder;

    private PillarReshaper[] reshapers = new PillarReshaper[4];

    protected override void Awake()
    {
        base.Awake();

        for (int i = 0; i < pillars.childCount; i++)
        {
            reshapers[i] = pillars.GetChild(i).GetComponent<PillarReshaper>();
        }
    }

    public void Adapt(int topStageSize, int bottomStageSize, float height)
    {
        float mean = (topStageSize + bottomStageSize) / 2f;
        int diff = topStageSize - bottomStageSize;

        for (int i = 0; i < pillars.childCount; i++)
        {
            var pillar = pillars.GetChild(i);
            var reshaper = reshapers[i];
            Vector3 sign = GetPillarSign(pillar);

            Vector3 pillarPosition = mean * GetPillarSign(pillar);
            pillarPosition.y = -height / 2;
            pillar.localPosition = pillarPosition;

            Vector3 topCenter = 0.5f * diff *  sign;
            reshaper.ReshapePillar(-topCenter, topCenter);

            pillar.localScale = new Vector3(1, height, 1);
        }

        AdjustFrameSize(topStageSize);
    }
}