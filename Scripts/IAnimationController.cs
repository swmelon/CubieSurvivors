public interface IAnimationController
{
    public void Spin();
    public void SpinLeft();

    public bool IsSpinning { get; }
    public void FinishSpinning();

    public bool IsInAir { get; }
}