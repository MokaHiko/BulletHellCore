using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollingUITexture : MonoBehaviour
{
    [SerializeField]
    public float speed = 1.0f;

    [SerializeField]
    public RawImage image;

    void Start()
    {
        image.mainTexture.wrapMode = TextureWrapMode.Repeat;
    }

    void Update()
    {
        m_currentscroll += speed * Time.deltaTime;
        Rect rect = image.uvRect;
        rect.width = 0.035f;
        rect.height = 0.5f;
        rect.x = m_currentscroll;
        rect.y = m_currentscroll;
        image.uvRect = rect;
    }

    float m_currentscroll = 0;
}
