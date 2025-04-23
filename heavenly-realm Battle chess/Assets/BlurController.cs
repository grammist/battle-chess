using UnityEngine;

public class BlurController : MonoBehaviour
{
    public Material blurMaterial;
    private float blurAmount = 0;
    private bool isBlurring = false;

    void Update()
    {
        if (isBlurring)
        {
            blurAmount = Mathf.Lerp(blurAmount, 5, Time.deltaTime * 2); // 逐渐增加模糊
        }
        else
        {
            blurAmount = Mathf.Lerp(blurAmount, 0, Time.deltaTime * 2); // 逐渐取消模糊
        }

        blurMaterial.SetFloat("_BlurSize", blurAmount);
    }

    public void StartBlur()
    {
        isBlurring = true;
    }

    public void StopBlur()
    {
        isBlurring = false;
    }
}
