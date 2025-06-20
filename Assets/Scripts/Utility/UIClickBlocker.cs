
// UIClickBlocker.cs - Add to any UI element to prevent clicks from passing through
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class UIClickBlocker : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    IBlockerListener blockerListener;

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
        blockerListener = listener;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Block the click event from propagating
        eventData.Use();
        blockerListener?.OnBlockerClicked();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Block the pointer down event from propagating
        eventData.Use();
        blockerListener?.OnBlockerPointerDown();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Block the pointer up event from propagating
        eventData.Use();
        blockerListener?.OnBlockerPointerUp();
    }
}

public interface IBlockerListener
{
    void OnBlockerClicked() { }
    void OnBlockerPointerDown() { }
    void OnBlockerPointerUp() { }
}