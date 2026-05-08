using UnityEngine;
using UnityEngine.UIElements;
using SimpleJSON;

public class CustomNumAnimController : MonoBehaviour
{
    public Label _numberText;
    public NumAnimData _data;
    public int m_currentNum;

    void Start()
    {
    }

    public void Animate(NumAnimData d)
    {
        // Stop previous animation
        LeanTween.cancel(gameObject);
        LeanTween.value(gameObject, m_currentNum, d._total, d._animationTime)
            .setEase(LeanTweenType.easeOutQuart)
            .setOnUpdate(UpdateUI);
    }

    public void AnimateFromJson(string filename)
    {
        JSONObject jackpot = JsonLoadforTest.LoadJsonFile(filename);
        _data._total = jackpot["Jackpotnum"];
        _data._animationTime = jackpot["animationtime"];
        Animate(_data);
    }

    public void UpdateUI(float v)
    {
        m_currentNum = (int)v;
        _numberText.text = v.ToString("0");
    }
}