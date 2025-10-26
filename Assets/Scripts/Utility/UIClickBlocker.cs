
// UIClickBlocker.cs - Add to any UI element to prevent clicks from passing through
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

[RequireComponent(typeof(Image))]
public class UIClickBlocker : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    List<IBlockerListener> blockerListener = new();

    private void Start()
    {
        // Ensure the Image component exists and is set to raycast target
        Image image = GetComponent<Image>();
        if (image != null)
        {
            // Make the image raycast target but possibly transparent
            image.raycastTarget = true;

            // Optionally, if you want the image to be invisible but still block clicks
            // Set the alpha to nearly zero but not quite (0 would disable raycasting)
            Color color = image.color;
            if (color.a == 0)
            {
                color.a = 0.001f;
                image.color = color;
            }
        }
    }

    public void SetBlockerListener(IBlockerListener listener)
    {
        blockerListener.Add(listener);
        gameObject.SetActive(true);
    }

    public void removeListener(IBlockerListener listener)
    {
        blockerListener.Remove(listener);
        if (blockerListener.Count == 0)
        {
            gameObject.SetActive(false);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Block the click event from propagating
        eventData.Use();
        for (int i = blockerListener.Count - 1; i >= 0; i--)
        {
            blockerListener[i]?.OnBlockerClicked();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Block the pointer down event from propagating
        eventData.Use();
        for (int i = blockerListener.Count - 1; i >= 0; i--)
        {
            blockerListener[i]?.OnBlockerPointerDown();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Block the pointer up event from propagating
        eventData.Use();
        for (int i = blockerListener.Count - 1; i >= 0; i--)
        {
            blockerListener[i]?.OnBlockerPointerUp();
        }
    }
}

public interface IBlockerListener
{
    void OnBlockerClicked() { }
    void OnBlockerPointerDown() { }
    void OnBlockerPointerUp() { }
}