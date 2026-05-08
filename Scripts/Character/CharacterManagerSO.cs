
using StarterAssets;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;


[CreateAssetMenu(fileName = "CharacterManager", menuName = "ScriptableObjects/CharacterManager", order = SOAssetMenuIndex.Manager)]
public class CharacterManagerSO : ScriptableObject, IDependentInitialization
{
    [SerializeField] 
    private List<CharDataSO> charData;

    [SerializeField]
    private EventChannelSO resetCharacterTransformChannel;
    
    [SerializeField]
    private SaveLoadManagerSO saveLoadManager;

    [SerializeField]
    private GameAccessoryManager gameAccManager;


    private Dictionary<int, GameObject> availableCharInstances = new Dictionary<int, GameObject>();
    private Dictionary<int, GameObject> lockedCharInstances = new Dictionary<int, GameObject>(); 
    private SaveFile saveFile;
    private int currentCharIndex = -1;
    private Quaternion initialRotation;
    private Vector3 initialPosition;
    private Dictionary<int, List<AccData>> accessoriesEquipped; 

    public void Initialize()
    {
        ClearInstances();

        accessoriesEquipped = gameAccManager.AccessoriesEquipped;
        initialRotation = Quaternion.Euler(0, 180, 0);
        initialPosition = new Vector3(0, 0.5f, 0);
        saveFile = saveLoadManager.SaveFile;
        resetCharacterTransformChannel.Subscribe(ResetCharacterTransform);

        foreach (CharDataSO data in charData)
        {
            GameObject characterObject = data.Instantiate();

            if (data.CharIndex == saveFile.LastSelectedCharIndex)
            {
                currentCharIndex = data.CharIndex; // current Index.
            }

            characterObject.SetActive(false);
            AccessoryManager accManager = characterObject.GetComponent<AccessoryManager>();
            DecoData decoData = new DecoData();

            // find the accessory whose key matches the character index

            if (accessoriesEquipped.TryGetValue(data.CharIndex, out List<AccData> accessories))
            {
                foreach (AccData accData in accessories)
                {
                    decoData.AddAccessory(accData);
                }
            }
            else
            {
                // 악세서리가 초기화 되지 않은 경우 초기값으로 세이브 파일에 기록
                if (saveFile.charactersUnlocked.TryGetValue(data.CharIndex, out bool value) && !value)
                {
                    saveFile.charactersUnlocked[data.CharIndex] = true;

                    accManager.BaseAccessoryIDs.ForEach(id =>
                    {
                        if (gameAccManager.TryGetBaseAccData(data.CharIndex, id, out AccData accData, saveData : true))
                        {
                            decoData.AddAccessory(accData);
                        }
                    });
                }
            } 

            // add user customized material.

            accManager.SetInitialDecoration(decoData);

            if (!saveFile.charactersUnlocked.TryGetValue(data.CharIndex, out bool unlocked))
            {
                    // 만약 saveFile에 캐릭터가 없더라도  오류가 생기진 않는다.
                lockedCharInstances.Add(data.CharIndex, characterObject);
            }
            else
            {
                availableCharInstances.Add(data.CharIndex, characterObject);
            }

            characterObject.transform.rotation = initialRotation;
        }

        availableCharInstances[currentCharIndex].SetActive(true);
    }

    private void ClearInstances()
    {
        availableCharInstances.Clear();
        lockedCharInstances.Clear();
    }
    
    public int GetCurrentCharData(out CharDataSO charData)
    {
        charData = this.charData[currentCharIndex];
        return currentCharIndex;
    }
    
    public int GetCurrentCharIndex()
    {
        return currentCharIndex;
    }
    
    
    public bool TryGetChar(int charIndex, out GameObject character)
    {
        if (!CharAvailable(charIndex))
        {
            character = null;
            return false;
        }
        
        character = availableCharInstances[charIndex];
        return true;
    }

    public GameObject GetCurrentCharObject()
    {
        return availableCharInstances[currentCharIndex];
    }

    public bool ChangeChar(int charIndex)
    {
        if (!CharAvailable(charIndex))
        {
            return false;
        }
        
        availableCharInstances[currentCharIndex].SetActive(false);
        availableCharInstances[charIndex].SetActive(true);
        currentCharIndex = charIndex;
        saveFile.LastSelectedCharIndex = currentCharIndex;
        return true;
    }
    
    public bool TryGetCharIcon(int charIndex, out Sprite icon)
    {
        if (!CharAvailable(charIndex))
        {
            icon = default;
            return false;
        }
        
        icon = charData[charIndex].Icon;
        return true;
    }
    
    public bool TryGetCharName(int charIndex, out string charName)
    {
        if (!CharAvailable(charIndex))
        {
            charName = default;
            return false;
        }
        
        charName = charData[charIndex].CharName;
        return true;
    }
    
    public bool TryGetCharDescription(int charIndex, out string charDescription)
    {
        if (!CharAvailable(charIndex))
        {
            charDescription = default;
            return false;
        }
        
        charDescription = charData[charIndex].Description;
        return true;
    }

    public bool CharAvailable(int charIndex)
    {
        if  (charIndex < 0 || charIndex >= charData.Count)
        {
            Debug.Log($"CharacterManager: CharIndex {charIndex} is out of range.");
            return false;
        }

        if (!saveFile.charactersUnlocked.TryGetValue(charIndex, out bool value))
        {
            return false;    
        }
        
        return true;
    }

    public GameObject GetLockedCharacter(int charIndex)
    {
        if (!lockedCharInstances.TryGetValue(charIndex, out GameObject character))
        {
            Debug.Log($"CharacterManager: CharIndex {charIndex} is not locked.");
            return null;
        }

        if (!character.TryGetComponent(out AccessoryManager accessoryManager))
        {
            Debug.LogError($"CharacterManager: CharIndex {charIndex} does not have an AccessoryManager component.");
            return null;
        }

        accessoryManager.BaseAccessoryIDs.ForEach(id =>
        {
            if (gameAccManager.TryGetBaseAccData(charIndex, id, out AccData accData, saveData : false))
            {
                accessoryManager.Equip(accData);
            }
        });

        return character;
    }

    public void ReturnAndUnlockCharacter(int charIndex, GameObject charInstance)
    {
        if (availableCharInstances.TryGetValue(charIndex, out GameObject character))
        {
            Debug.LogError($"CharacterManager: CharIndex {charIndex} is already available. Check charIndex");
            return;
        }

        if (!charInstance.TryGetComponent(out AccessoryManager accessoryManager))
        {
            Debug.LogError($"CharacterManager: CharIndex {charIndex} does not have an AccessoryManager component.");
            return;
        }

        List<AccData> accDatas = accessoryManager.GetEquippedAccessories();

        foreach (AccData accData in accDatas)
        {
            gameAccManager.SaveEquippedAccessory(charIndex, accData);
        }

        charInstance.SetActive(false);
        
        availableCharInstances.Add(charIndex, charInstance);
        lockedCharInstances.Remove(charIndex);

        // Disable the character, deitemize it and save the game.


        // 알아서 추가된다.
        // 원래 bool을 unlock 상태를 나타내기위해 사용했지만, 존재만으로 unlock을 판단
        // 가능하니 이것을 악세사리가 초기화 됐는지 안됐는지를 나타내는 용도로 사용하겠다.
        saveFile.charactersUnlocked[charIndex] = true;
        saveFile.LastSelectedCharIndex = charIndex;
        saveLoadManager.Save();
    }

    private void ResetCharacterTransform()
    {
        foreach (var character in availableCharInstances)
        {
            character.Value.transform.rotation = initialRotation;

            if (character.Value.TryGetComponent(out CustomThirdPersonController controller))
            {
                controller.IgnoreInput();
                controller.MoveOnlyCharacterTo(initialPosition);
                controller.ListenInput();
            }
            else
            {
                character.Value.transform.position = initialPosition;
            }
        }

        foreach (var character in lockedCharInstances)
        {
            character.Value.transform.rotation = initialRotation;

            if (character.Value.TryGetComponent(out CustomThirdPersonController controller))
            {
                controller.IgnoreInput();
                controller.MoveOnlyCharacterTo(initialPosition);
                controller.ListenInput();
            }
            else
            {
                character.Value.transform.position = initialPosition;
            }
        }
    }
}
