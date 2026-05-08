using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;
using Minimalist.Quantity;


[RequireComponent(typeof(GravityDelegator))]
public class ExpManager : MonoBehaviour
{
    [Header("Invoke")]
    [SerializeField]
    private EventChannelSO maxExpEventChannel;

    [SerializeField]
    private FloatEventChannelSO expChangeEventChannel;

    [Header("Subscribe")]
    [SerializeField]
    private QuantityBhvChannel expQuantityChannel;

    [SerializeField]
    private DifficultyCurveManagerSO difficultyCurveManager;

    [SerializeField]
    private DifficultyCurveEC difficultyCurveResetEC;

    [SerializeField]
    private EventChannelSO playerDeadEC, playerReviveEC;

    [SerializeField]
    private Slider slider;

    [SerializeField]
    private TextMeshProUGUI textMesh;

    [SerializeField]
    private GameObject levelUpEffect;


    private QuantityBhv expQuantityBhv;
    private GravityDelegator gravityDelegator;
    private int exp;
    private int expMax;
    private bool locked;

    private int EXP
    {
        get { return exp; }
        set
        {
            exp = value;
            float expRatio = (float) exp / expMax;
            expQuantityBhv.FillAmount = expRatio;
            
            if (slider != null)
            {
                slider.value = expRatio;
            }
            
            expChangeEventChannel.Raise(expRatio);
            
            if (exp < expMax)
            {
                return;
            }

            exp = expMax;
            maxExpEventChannel.Raise();

            SetMaxExp(difficultyCurveManager.UpdateExp());

            levelUpEffect .SetActive(false);
            levelUpEffect.SetActive(true);

            // do not level up when player's gravity is not activated.
        }
    }

    private void OnEnable()
    {
        playerDeadEC.Subscribe(OnDead);
        playerReviveEC.Subscribe(OnRevive);
        expQuantityChannel.Subscribe(SetExpQuantityBhv);
        difficultyCurveResetEC.Subscribe(OnResetDifficultyCurve);
    }

    private void OnDisable()
    {
        playerDeadEC.Unsubscribe(OnDead);
        playerReviveEC.Unsubscribe(OnRevive);
        expQuantityChannel.Unsubscribe(SetExpQuantityBhv);
        difficultyCurveResetEC.Unsubscribe(OnResetDifficultyCurve);
    }
    
    public void GetExp(int exp)
    {
        if (locked)
        {
            return;
        }

        EXP += exp;
    }

    public void SetMaxExp(int expMax)
    {
        this.expMax = expMax;
        EXP = 0;
        expQuantityBhv.MaximumAmount = expMax;
    }
    
    public void SetExpBar(Slider slider)
    {
        this.slider = slider;
    }

    private void OnDead()
    {
        locked = true;
    }

    private void OnRevive()
    {
        locked = false;
    }

    private void SetExpQuantityBhv(QuantityBhv expQuantityBhv)
    {
        this.expQuantityBhv = expQuantityBhv;
    }

    private void OnResetDifficultyCurve(DifficultyCurveManagerSO curveManager)
    {
        SetMaxExp(curveManager.GetMaxExp());
    }
}

