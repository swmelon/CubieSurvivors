using Local.Scripts.Extensions;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class DragRotateCharacter : MonoBehaviour
{
    [SerializeField]
    private TransformChannelSO playerTransformChannel;

    [SerializeField] private InputAction press, screenPos;

    [SerializeField] private Vector3 initialOffset, initialCamRotation;

    [SerializeField]
    private Camera characterCamera;

    [SerializeField]
    float rotationSpeed = 0.1f; // Adjust rotation speed to your liking

    private bool isDragging;
    private Vector2 previousScreenPos;
    private Vector2 curScreenPos;
    private Transform playerTransform;
    private Vector3 offset;

    private void OnEnable()
    {
        playerTransformChannel.Subscribe(SetPlayerTransform);

        screenPos.Enable();
        press.Enable();
        screenPos.performed += OnScreenPosChanged;
        press.performed += OnScreenPressed;
        press.canceled += OnScreenPressCanceled;

        Quaternion lookingCharacterRotation = playerTransform.rotation * Quaternion.Euler(0, 180, 0);

        offset = lookingCharacterRotation* initialOffset;
        transform.position = playerTransform.position + offset;
        transform.rotation = Quaternion.Euler(0, initialCamRotation.y, 0) * lookingCharacterRotation * Quaternion.Euler(initialCamRotation.x, 0, 0);
    }

    private void OnDisable()
    {
        screenPos.Disable();
        press.Disable();
        screenPos.performed -= OnScreenPosChanged;
        press.performed -= OnScreenPressed;
        press.canceled -= OnScreenPressCanceled;
        playerTransformChannel.Unsubscribe(SetPlayerTransform);
    }

    private IEnumerator DragRotation()
    {
        isDragging = true;
        while (isDragging)
        {
            Vector2 delta = curScreenPos - previousScreenPos;
            float angleY = delta.x * rotationSpeed;
            float angleX = -delta.y * rotationSpeed;

            // Rotate around the up axis of the object
            characterCamera.transform.RotateAround(playerTransform.position, Vector3.up, angleY);

            characterCamera.transform.RotateAround(playerTransform.position, characterCamera.transform.right, angleX);

            offset = characterCamera.transform.position - playerTransform.position;
            
            previousScreenPos = curScreenPos; // Update the previous position for the next frame
            yield return null;
        }
    }

    private void OnScreenPosChanged(InputAction.CallbackContext context)
    {
        curScreenPos = context.ReadValue<Vector2>();
    }

    private void OnScreenPressed(InputAction.CallbackContext context)
    {
        if (isClickedOn)
        {
            Debug.Log("Clicked on player");
            previousScreenPos = Mouse.current.position.ReadValue();

            if (previousScreenPos == Vector2.zero)
            {
                previousScreenPos = Touchscreen.current.primaryTouch.position.ReadValue();
            }

            StartCoroutine(DragRotation());
        }
    }

    private void OnScreenPressCanceled(InputAction.CallbackContext context)
    {
        isDragging = false;
    }

    private void SetPlayerTransform(Transform player)
    {
        playerTransform = player;
    }

    private bool isClickedOn
    {
        get
        {
            Ray ray = characterCamera.ScreenPointToRay(curScreenPos);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMaskCash.OnlyPlayer, QueryTriggerInteraction.Ignore))
            {
                return ReferenceEquals(hit.transform.root, playerTransform);
            }
            return false;
        }
    }

    private void Update()
    {
        if (playerTransform == null)
        {
            return;
        }

        transform.position = playerTransform.position + offset;
    }
}
