using UnityEngine;

public class CharacterUnlockItem : CoveredItem<Player>
{
    [SerializeField]
    private int charIndex;

    [SerializeField]
    private CharacterManagerSO characterManager;

    [SerializeField]
    private EventChannelSO getCharacterUnlockItemEC, startCharacterUnlockCutSceneEC;

    [SerializeField]
    private OnePureEffectSpawner disapearEffectSpawner;

    [SerializeField]
    private Portal portal;

    private Player playerHit, playerContent;
    private float shrinkScale = 0.25f;

    protected override void OnEnable()
    {
        base.OnEnable();
        startCharacterUnlockCutSceneEC.Subscribe(OnStartCutScene);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        startCharacterUnlockCutSceneEC.Unsubscribe(OnStartCutScene);
    }

    private void Start()
    {
        if (characterManager.CharAvailable(charIndex))
        {
            gameObject.SetActive(false);
            portal.gameObject.SetActive(true);
            return;
        }

        Player player = characterManager.GetLockedCharacter(charIndex).GetComponent<Player>();
        player.BeItem();
        SetContent(player);
        player.gameObject.SetActive(true);
        collider.enabled = true;
        playerContent = player;
    }

    public override void Activate(Player player)
    {
        getCharacterUnlockItemEC.Raise();
        collider.enabled = false;
        playerHit = player;
    }

    protected override void OnStageMove()
    {
        // Do nothing
    }

    private void OnStartCutScene()
    { 
        Transform targetTransform = playerHit.transform;
        SetPopOutPosition(transform.position + targetTransform.forward, targetTransform.position + 2f * Vector3.up);
        moveTime = 5f;
    }

    protected override void Update()
    {
        if (isMoving)
        {
            spinSpeed += 200f * Time.deltaTime;

            if (timeCount < moveTime)
            {
                transform.localScale = Vector3.Lerp(Vector3.one, shrinkScale * Vector3.one, timeCount / moveTime);
            }

            if (timeCount + Time.deltaTime > moveTime)
            {
                DeitemizeCharacter();
                portal.gameObject.SetActive(true);
            }
        }

        base.Update();
    }

    private void DeitemizeCharacter()
    {
        characterManager.ReturnAndUnlockCharacter(charIndex, content.gameObject);
        content.transform.parent = null;
        content = null;
        playerContent.Deitemize();
        disapearEffectSpawner.Spawn().transform.position = transform.position;
        Destroy(gameObject);
    }
}