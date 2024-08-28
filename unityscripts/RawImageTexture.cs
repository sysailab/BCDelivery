using UnityEngine;
using UnityEngine.UI;

public class RawImageTexture : MonoBehaviour
{
    public void GetImageTexture(Texture imagetexture)
    {
        gameObject.GetComponent<RawImage>().texture = imagetexture;
    }
}
