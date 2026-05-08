using UnityEngine;

public struct ScenarioData
{
    public int scenarioNumber;
    public string name;
    public Texture2D icon;
    public long maxScore;
    public long maxScoreHard, maxScoreHell;
    public Difficulty maxDifficulty;
    public bool Developing;
}