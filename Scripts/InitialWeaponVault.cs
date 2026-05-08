using UnityEngine;
using UnityEngine.Events;

public class InitialWeaponVault : MonoBehaviour
{
    public UnityEvent<Vector3> OnWeaponVaultOpened;

    [SerializeField]
    private EventChannelSO onGetInitialWeaponChannel;

    private bool opened = false;
    private Animator animator;
    private string openVaultTrigger = "OpenVault";

    private void Awake()
    {
        onGetInitialWeaponChannel.Subscribe(DestroySelf);
        animator = GetComponent<Animator>();
    }

    private void OnDestroy()
    {
        onGetInitialWeaponChannel.Unsubscribe(DestroySelf);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !opened)
        {
            OnWeaponVaultOpened?.Invoke(transform.position + Vector3.up);
            animator.SetTrigger(openVaultTrigger);
            FMODAudioManager.instance.PlayOneShot(SFXTags.OpenChest);
            //play open sound
            opened = true;
        }
    }

    private void DestroySelf()
    {
        Destroy(gameObject);
    }
}