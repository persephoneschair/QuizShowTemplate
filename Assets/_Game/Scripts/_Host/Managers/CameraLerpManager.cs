using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLerpManager : SingletonMonoBehaviour<CameraLerpManager>
{
    private Camera cam;

    public enum CameraPosition { Default };
    public Transform[] angles;
    public float defaultFieldOfView = 60f;
    public float defaultTransitionDuration = 2f;

    private float elapsedTime;
    private Vector3 startPos;
    private Vector3 startRot;
    private float startFov;
    private Vector3 endPos;
    private Vector3 endRot;
    private float endFov;
    private bool isMoving;
    private float duration = 2f;

    #region Init

    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    #endregion

    #region Public Functions

    public void ZoomToPosition(CameraPosition target, float fov, float transitionDuration)
    {
        if (isMoving)
            return;

        startPos = cam.transform.localPosition;
        startRot = cam.transform.localEulerAngles;
        startFov = cam.fieldOfView;

        endPos = angles[(int)target].transform.localPosition;
        endRot = angles[(int)target].transform.localEulerAngles;
        endFov = fov;

        elapsedTime = 0;
        duration = transitionDuration;
        isMoving = true;
        Invoke("EndLock", duration);
    }

    public void ZoomToPosition(CameraPosition target, float fov)
    {
        ZoomToPosition(target, defaultTransitionDuration, fov);
    }

    public void ZoomToPosition(CameraPosition target)
    {
        ZoomToPosition(target, defaultTransitionDuration, defaultFieldOfView);
    }

    #endregion

    #region Private Functions

    private void Update()
    {
        if (isMoving)
            PerformLerp();
    }

    private void PerformLerp()
    {
        elapsedTime += Time.deltaTime;

        float percentageComplete = elapsedTime / duration;

        this.gameObject.transform.localPosition = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0, 1, percentageComplete));

        float x = Mathf.LerpAngle(startRot.x, endRot.x, Mathf.SmoothStep(0, 1, percentageComplete));
        float y = Mathf.LerpAngle(startRot.y, endRot.y, Mathf.SmoothStep(0, 1, percentageComplete));
        float z = Mathf.LerpAngle(startRot.z, endRot.z, Mathf.SmoothStep(0, 1, percentageComplete));
        this.gameObject.transform.localEulerAngles = new Vector3(x, y, z);//Vector3.Lerp(startRot, endRot, Mathf.SmoothStep(0, 1, percentageComplete));

        cam.fieldOfView = Mathf.Lerp(startFov, endFov, Mathf.SmoothStep(0, 1, percentageComplete));
    }

    public void EndLock()
    {
        isMoving = false;
    }

    #endregion
}
