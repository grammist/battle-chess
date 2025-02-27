using UnityEngine;
using System.Collections.Generic;

public class CubeFollower : MonoBehaviour
{
    public GameObject parentObject;
    private List<GameObject> targetChildren = new List<GameObject>();
    GameObject cube;

    void Start()
    {
        cube = GameObject.Find("Cube");
        if (parentObject == null)
        {
            parentObject = GameObject.Find("Game board");
            if (parentObject == null)
            {
                return;
            }
        }

        foreach (Transform child in parentObject.transform)
        {
            if (child.childCount > 0)
            {
                targetChildren.Add(child.GetChild(0).gameObject);
            }
        }
    }

    void Update()
    {
        targetChildren.RemoveAll(item => item == null); // Remove destroyed objects

        foreach (var obj in targetChildren)
        {
            if (obj != null && obj.GetComponent<HoverChangeColor>().checkisHovered())
            {
                cube.transform.position = obj.transform.position + new Vector3(0, 3.0f, 0);
                
                if (obj.GetComponent<HoverChangeColor>().checkisClicked())
                {
                    cube.GetComponent<Renderer>().material.color = Color.red;
                }
                else
                {
                    cube.GetComponent<Renderer>().material.color = Color.yellow;
                }
                return;
            }
        }

        cube.transform.position = new Vector3(0, 2000.0f, 0);
    }
}
