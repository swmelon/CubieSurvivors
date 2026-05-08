using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTransparency : MonoBehaviour
{
    [SerializeField]
    private TransformChannelSO playerTransformChannel;

    private ObjectFader _fader;
    private Transform player;
    public LayerMask targetLayer;

    private CameraController mainCameraController;

    private void Awake()
    {
        mainCameraController = GetComponent<CameraController>();
    }

    private void OnEnable()
    {
        playerTransformChannel.Subscribe(SetPlayerTransform);
    }

    private void OnDisable()
    {
        playerTransformChannel.Unsubscribe(SetPlayerTransform);
    }

    void Update()
    {
        if (!ReferenceEquals(player, null))//&& mainCameraController.IsThirdPersonMode())
        {
            Vector3 dir = player.position - transform.position;
            Ray ray = new Ray(transform.position, dir);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f, targetLayer, queryTriggerInteraction: QueryTriggerInteraction.Ignore))
            {
                if (hit.collider == null)
                    return;

                if (ReferenceEquals(hit.collider.gameObject, player.gameObject))
                {
                    // Nothing is in front of the player
                    if (_fader != null)
                    {
                        _fader.DoFade = false;
                    }
                }
                else
                {
                    _fader = hit.collider.gameObject.GetComponent<ObjectFader>();
                    if (_fader != null)
                    {
                        _fader.DoFade = true;
                    }
                }
            }
        }
    }

    private void SetPlayerTransform(Transform newTransform) 
    {
        player = newTransform;
    }
}
