using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Assign_Char : MonoBehaviour
{
    private Dictionary<GameObject, Sprite> objectSpriteMap = new Dictionary<GameObject, Sprite>();
    [System.Serializable]

    public struct ObjectSpritePair
    {
        public string objectName;
        public Sprite sprite;
    }

    public List<ObjectSpritePair> objectSpritePairs;
    public Image uiImage1;
    public Image uiImage2;
    
    // Start is called before the first frame update
    void Start()
    {
        PopulateDictionary ();
    }

    void PopulateDictionary()
    {
        foreach (var pair in objectSpritePairs)
        {
            GameObject obj = GameObject.Find(pair.objectName);
            if (obj != null)
            {
                objectSpriteMap[obj] = pair.sprite;
                Debug.Log($"GameObject {pair.objectName} bound with Sprite: {pair.sprite.name}");
            }
            else
            {
                Debug.LogWarning($"do not find GameObject: {pair.objectName}");
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowObjectImage(GameObject obj, GameObject obj1)
    {
        Debug.Log(0);
        if (objectSpriteMap.ContainsKey(obj))
        {
            if (uiImage1 != null)
            {
                uiImage1.sprite = objectSpriteMap[obj];
            }
            else
            {
                Debug.LogError("UI Image component is not assigned!");
            }
        }
        else
        {
            Debug.LogWarning($"No sprite found for {obj.name} in dictionary.");
        }

        if (objectSpriteMap.ContainsKey(obj1))
        {
            if (uiImage2 != null)
            {
                uiImage2.sprite = objectSpriteMap[obj1];
            }
            else
            {
                Debug.LogError("UI Image component is not assigned!");
            }
        }
        else
        {
            Debug.LogWarning($"No sprite found for {obj.name} in dictionary.");
        }
    }

    public void ClearUIImage()
    {
        if (uiImage1 != null)
        {
            uiImage1.sprite = null;
            Debug.Log("UI Image is null");
        }
        else
        {
            Debug.LogError("UI Image do not have value");
        }

        if (uiImage2 != null)
        {
            uiImage2.sprite = null;
            Debug.Log("UI Image is null");
        }
        else
        {
            Debug.LogError("UI Image do not have value");
        }
    }
}
