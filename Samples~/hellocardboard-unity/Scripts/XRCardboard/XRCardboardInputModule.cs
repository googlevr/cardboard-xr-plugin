using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Linq;
using UnityEngine.UI;
using System;
#if !UNITY_EDITOR
using UnityEngine.XR;
#endif

public class XRCardboardInputModule : PointerInputModule
{
    [SerializeField]
    XRCardboardInputSettings settings = default;
    [SerializeField]
    UnityFloatEvent onStartHover = default;
    [SerializeField]
    UnityEvent onEndHover = default;
    [SerializeField]
    UnityEvent onClick = default;

    PointerEventData pointerEventData;
    GameObject currentTarget;
    float currentTargetClickTime = float.MaxValue;
    bool hovering;

    public override void Process()
    {
        HandleLook();
        HandleSelection();
    }

    void HandleLook()
    {
        if (pointerEventData == null)
            pointerEventData = new PointerEventData(eventSystem);
#if UNITY_EDITOR
        pointerEventData.position = new Vector2(Screen.width / 2, Screen.height / 2);
#else
        pointerEventData.position = new Vector2(XRSettings.eyeTextureWidth / 2, XRSettings.eyeTextureHeight / 2);
#endif
        pointerEventData.delta = Vector2.zero;
        var raycastResults = new List<RaycastResult>();
        eventSystem.RaycastAll(pointerEventData, raycastResults);
        raycastResults = raycastResults.OrderBy(r => !r.module.GetComponent<GraphicRaycaster>()).ToList();
        pointerEventData.pointerCurrentRaycast = FindFirstRaycast(raycastResults);
        ProcessMove(pointerEventData);
    }

    void HandleSelection()
    {
        GameObject handler;
        try
        {
            handler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(pointerEventData.pointerEnter);
            var selectable = handler.GetComponent<Selectable>();
            if (selectable && selectable.interactable == false)
                throw new NullReferenceException();
        }
        catch (NullReferenceException)
        {
            currentTarget = null;
            StopHovering();
            return;
        }

        if (currentTarget != handler)
        {
            var gazeTime = settings.GazeTime;
            currentTarget = handler;
            currentTargetClickTime = Time.realtimeSinceStartup + gazeTime;
            if (hovering)
                StopHovering();
            hovering = true;
            onStartHover?.Invoke(gazeTime);
        }

        if (Time.realtimeSinceStartup > currentTargetClickTime || Input.GetButtonDown(settings.ClickInput))
        {
            ExecuteEvents.ExecuteHierarchy(currentTarget, pointerEventData, ExecuteEvents.pointerClickHandler);
            currentTargetClickTime = float.MaxValue;
            onClick?.Invoke();
            StopHovering();
        }
    }

    void StopHovering()
    {
        if (!hovering)
            return;
        hovering = false;
        onEndHover?.Invoke();
    }
}