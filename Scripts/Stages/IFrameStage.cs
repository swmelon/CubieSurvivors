using UnityEngine;

public interface IFrameStage : IPoolable
{
    public float Height { get; set;}
    public float LowerHeight { get; set; }
    public int Size { get; set; }
    public void OnFinishStageMove();
    public void SetPillarHeight(float height, float lowerHeight);

    public void FadeOutPillars(float changeTime);
    public void DisablePillarBlockingView();
    public StageType StageType { get; set; }
}
