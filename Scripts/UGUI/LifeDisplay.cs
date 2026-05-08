using UnityEngine;
using UnityEngine.UI;



public class LifeDisplay : MonoBehaviour
{
    [SerializeField]
    private Sprite lifeFull, lifeEmpty;

    [SerializeField]
    private UGUIEasyHeartbeat easyBeat;

    private Image[] images; 
    private int lifeCount = 0;
    private int maxLife = 0;

    public int LifeCount => lifeCount;

    private void Awake()
    {
        images = GetComponentsInChildren<Image>();
    }

    public void SetMaxLife(int maxLife)
    {
        ResetIcons();

        this.maxLife = maxLife;
        lifeCount = maxLife;

        for (int i = 0; i < maxLife; i++)
        {
            images[i].gameObject.SetActive(true);
            images[i].sprite = lifeFull;
            easyBeat.AddListener(images[i].rectTransform);
        }
    }

    public void ConsumeLife()
    {
        if (lifeCount <= 0)
        {
            return;
        }

        lifeCount--;
        images[lifeCount].sprite = lifeEmpty;
        easyBeat.RemoveListener(images[lifeCount].rectTransform);
    }


    public void AddLife()
    {
        if (maxLife == 0)
        {
            SetMaxLife(1);
            return;
        }

        if ( maxLife + 1 > images.Length)
        {
            Debug.LogWarning("Max life is greater than the number of images");

            images[lifeCount].sprite = lifeFull;
            easyBeat.AddListener(images[lifeCount].rectTransform);
            lifeCount++;
            return;
        }

        images[maxLife].gameObject.SetActive(true);
        easyBeat.AddListener(images[lifeCount].rectTransform);


        for (int i = maxLife; i  >= 1; i--)
        {
            images[i].sprite = images[i - 1].sprite;
        }

        images[0].sprite = lifeFull;
        lifeCount++;
        maxLife++;
    }

    private void ResetIcons()
    {
        for (int i = 0; i < images.Length; i++)
        {
            images[i].gameObject.SetActive(false);
        }
    }
}