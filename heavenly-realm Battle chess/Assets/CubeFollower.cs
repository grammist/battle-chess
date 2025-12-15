using UnityEngine;
using System.Collections.Generic;

public class CubeFollower : MonoBehaviour
{
    [Header("Board root (parent of A1..H8)")]
    public GameObject parentObject; // e.g., "Game board"

    [Header("Follower")]
    public string cubeName = "Cube";
    public Vector3 offset = new Vector3(0f, 3f, 0f);
    public float rescanEverySeconds = 0.5f;

    private readonly List<GameObject> targetChildren = new List<GameObject>();
    private GameObject cube;
    private Renderer cubeRenderer;
    private float nextRescanTime;

    void Start()
    {
        // Find the cube
        cube = GameObject.Find(cubeName);
        if (cube) cubeRenderer = cube.GetComponent<Renderer>();

        // Find the board root if not assigned
        if (parentObject == null)
            parentObject = GameObject.Find("Game board");

        // Initial scan
        RebuildTargets();
        nextRescanTime = Time.time + rescanEverySeconds;
    }

    void Update()
    {
        // Keep the list clean and fresh (handles promotions / captures)
        if (Time.time >= nextRescanTime)
        {
            RebuildTargets();
            nextRescanTime = Time.time + rescanEverySeconds;
        }
        targetChildren.RemoveAll(item => item == null);

        // If we lost the cube (or never had one), try to find it again
        if (cube == null)
        {
            cube = GameObject.Find(cubeName);
            if (cube) cubeRenderer = cube.GetComponent<Renderer>();
        }
        if (cube == null) return; // nothing to move

        foreach (var obj in targetChildren)
        {
            if (obj == null) continue;

            var hover = obj.GetComponent<HoverChangeColor>();
            if (hover == null) continue; // not a piece

            if (hover.checkisHovered())
            {
                cube.transform.position = obj.transform.position + offset;

                if (cubeRenderer != null)
                    cubeRenderer.material.color = hover.checkisClicked() ? Color.red : Color.yellow;

                return; // stop at first hovered piece
            }
        }

        // No hovered piece -> hide cube
        cube.transform.position = new Vector3(0f, 2000f, 0f);
    }

    // Finds the actual piece object on each tile, even if it sits under ScaleNeutralizer
    private void RebuildTargets()
    {
        targetChildren.Clear();
        if (parentObject == null) return;

        foreach (Transform tile in parentObject.transform)
        {
            // Look for any child in this tile (or its descendants) that has HoverChangeColor
            var hover = tile.GetComponentInChildren<HoverChangeColor>(true);
            if (hover != null)
            {
                var pieceGO = hover.gameObject;
                if (!targetChildren.Contains(pieceGO))
                    targetChildren.Add(pieceGO);
            }
        }
    }
}
