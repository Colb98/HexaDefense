using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DebugPanel : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] TMP_Dropdown dropdownDrawType;
    [SerializeField] TMP_InputField inputField;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        // Clear any existing options
        dropdownDrawType.ClearOptions();

        // Get all enum names
        string[] enumNames = Enum.GetNames(typeof(Map.DrawMode));

        // Convert to List<string>
        List<string> options = new List<string>(enumNames);

        // Add to dropdown
        dropdownDrawType.AddOptions(options);

        // (Optional) Subscribe to value change
        dropdownDrawType.onValueChanged.AddListener(OnDropdownChanged);
    }

    public void OnDropdownChanged(int index)
    {
        GameManager.Instance.GetMap().drawMode = (Map.DrawMode)index;
    }

    public void Save()
    {
        string filename = inputField.text;
        GameManager.Instance.GetMap().SaveToFile(filename);
    }

    // Drag handling methods

    RectTransform rt;
    Canvas rootCanvas;
    Vector2 pointerLocalStart;
    Vector3 panelLocalStart;

    public RectTransform clampArea;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas == null) Debug.LogWarning("DraggablePanel: no Canvas in parents.");
        if (clampArea == null && rootCanvas != null)
            clampArea = rootCanvas.transform as RectTransform;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // bring to front
        rt.SetAsLastSibling();

        // record starting positions
        var cam = (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay) ? rootCanvas.worldCamera : null;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            clampArea ?? (RectTransform)rootCanvas.transform,
            eventData.position, cam, out pointerLocalStart);

        panelLocalStart = rt.localPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        OnPointerDown(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (rootCanvas == null) return;
        var cam = (rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay) ? rootCanvas.worldCamera : null;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            clampArea ?? (RectTransform)rootCanvas.transform,
            eventData.position, cam, out Vector2 pointerLocalCurrent))
        {
            Vector3 offset = (Vector3)(pointerLocalCurrent - pointerLocalStart);
            rt.localPosition = panelLocalStart + offset;
            KeepInsideClampArea();
        }
    }

    public void OnEndDrag(PointerEventData eventData) { /* optional */ }

    void KeepInsideClampArea()
    {
        if (clampArea == null) return;

        // Convert panel corners to clampArea local space and keep anchoredPosition inside
        Vector3[] panelCorners = new Vector3[4];
        Vector3[] clampCorners = new Vector3[4];
        rt.GetLocalCorners(panelCorners);
        clampArea.GetLocalCorners(clampCorners);

        // compute panel bounds in clampArea local coordinates:
        // transform panel's local corner to world then to clampArea local
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, 0);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, 0);
        for (int i = 0; i < 4; i++)
        {
            Vector3 world = rt.TransformPoint(panelCorners[i]);
            Vector3 localInClamp = clampArea.InverseTransformPoint(world);
            min = Vector3.Min(min, localInClamp);
            max = Vector3.Max(max, localInClamp);
        }

        Vector3 clampMin = clampCorners[0];
        Vector3 clampMax = clampCorners[2];

        Vector3 shift = Vector3.zero;
        if (min.x < clampMin.x) shift.x = clampMin.x - min.x;
        if (min.y < clampMin.y) shift.y = clampMin.y - min.y;
        if (max.x > clampMax.x) shift.x = clampMax.x - max.x;
        if (max.y > clampMax.y) shift.y = clampMax.y - max.y;

        if (shift != Vector3.zero)
            rt.localPosition += shift;
    }
}
