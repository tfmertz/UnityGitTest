using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ColorSelector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IDragHandler, IEndDragHandler
{
    public delegate void ChangeColor(Color c);
    public event ChangeColor SetColor;

    private Texture2D TextureSource; //---Texture Source [color palette]
    private Image Texture_Image; //---Current texture image
    private Vector2 TextureSize; //---Original texture size
    public Image Pointer; //---Color Pointer
    private Vector3 curPosition;
    public Color Result; //---Final Result
    private float imageWidth;
    private float imageHeight;
    private float rectWidth = 100;
    private float rectHeight = 100;
    public Image Selected_Swatch;

    Color[] Data;
    SpriteRenderer SpriteRenderer;

    public int Width { get { return SpriteRenderer.sprite.texture.width; } }
    public int Height
    {
        get { return SpriteRenderer.sprite.texture.height; }
    }

    private void Awake()
    {
        Texture_Image = GetComponent<Image>();
        TextureSource = Texture_Image.sprite.texture;
        if (!TextureSource || !Texture_Image)
        {
            Debug.LogError("Color Picker: Missing script objects. Please check all required objects.");
            if (!Texture_Image)
                Debug.LogError("Color Picker - Additional: The script is not assigned to the UI Image object!");
            this.enabled = false;
            return;
        }
        imageWidth = Texture_Image.sprite.rect.width;
        imageHeight = Texture_Image.sprite.rect.height;
        TextureSize = new Vector2(TextureSource.width, TextureSource.height);
    }

    public void OnPointerClick(PointerEventData e)
    {
        curPosition = getPos(e);
    }
    public void OnDrag(PointerEventData e)
    {

        curPosition = getPos(e);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //throw new System.NotImplementedException();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //throw new System.NotImplementedException();
    }
    public Vector3 getPos(PointerEventData e)
    {
        Vector2 localCursor;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), e.position, e.pressEventCamera, out localCursor))
        {
            Debug.Log(localCursor.x);
            float x = ((imageWidth / rectWidth) * localCursor.x)+(imageWidth / 2);
            float y = ((imageHeight / rectHeight) * localCursor.y)+(imageHeight / 2);
            Result = TextureSource.GetPixel((int)x, (int)y);
            localCursor = localCursor * GetComponent<RectTransform>().localScale;
            RectTransform rect = Pointer.transform.parent.GetComponent<RectTransform>();
            // TEMP MAKE THIS A RAY CAST
            if (localCursor.x<rectWidth && localCursor.x> (0-rectWidth) && localCursor.y < rectHeight && localCursor.y > (0 - rectHeight))
            {
                rect.anchoredPosition = localCursor;
            }
            //rect.anchoredPosition = localCursor;
        }
        Debug.Log(GetComponent<RectTransform>().transform.position.z);
        SetTheColor(Result);
        return localCursor;
    }
  
    public void SetTheColorBlack()
    {
        Result = new Color(0, 0, 0);
        RectTransform rect = Pointer.transform.parent.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector3(0,0,0);
        SetTheColor(Result);
    }
    public void SetTheColorWhite()
    {
        Result = new Color(1, 1, 1);
        
        RectTransform rect = Pointer.transform.parent.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector3(0, 0, 0);
        SetTheColor(Result);
    }
    public void SetTheColor(Color color)
    {
        Result = color;
        Pointer.color = Result;
        Selected_Swatch.color = Result;
        SetColor.Invoke(color);
    }
    void IEndDragHandler.OnEndDrag(PointerEventData eventData)
    {
        //throw new System.NotImplementedException();
    }
}
