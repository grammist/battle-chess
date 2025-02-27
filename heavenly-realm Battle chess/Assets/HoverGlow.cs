using UnityEngine;

public class HoverGlow : MonoBehaviour
{
    private Renderer objRenderer;
    private Color originalEmissionColor;
    private Material objMaterial;

    void Start()
    {
        objRenderer = GetComponent<Renderer>();
        objMaterial = objRenderer.material;

        if (objMaterial.HasProperty("_EmissionColor"))
        {
            originalEmissionColor = objMaterial.GetColor("_EmissionColor");
        }
        else
        {
            Debug.LogWarning("Material does not support Emission.");
        }
    }

    void OnMouseEnter()
    {
        if (objMaterial.HasProperty("_EmissionColor"))
        {
            objMaterial.EnableKeyword("_EMISSION");
            objMaterial.SetColor("_EmissionColor", Color.white * 2.5f);
        }
    }

    void OnMouseExit()
    {
        if (objMaterial.HasProperty("_EmissionColor"))
        {
            objMaterial.SetColor("_EmissionColor", originalEmissionColor);
        }
    }
}
