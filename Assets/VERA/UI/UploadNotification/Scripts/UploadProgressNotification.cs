using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UploadProgressNotification : MonoBehaviour
{
    [SerializeField] private GameObject NotificationWindow;
    [SerializeField] private TextMeshProUGUI ProgressText;
    private VERALogger veraLogger;
    private Coroutine notificationCoroutine;

    void Start()
    {
        NotificationWindow.SetActive(false);
        SubscribeUploadEvents(VERALogger.Instance);
    }

    private void SubscribeUploadEvents(VERALogger logger)
    {
        veraLogger = logger;
        veraLogger.onBeginFileUpload.AddListener(StartDisplayNotificationCoroutine);
        veraLogger.onFileUploadExited.AddListener(CloseDisplayNotificationCoroutine);
    }
    
    private void StartDisplayNotificationCoroutine()
    {
        BeginNewDisplayCoroutine(DisplayNotification());
    }

    private void CloseDisplayNotificationCoroutine()
    {
        BeginNewDisplayCoroutine(CloseNotificationGracefully());
    }

    private void BeginNewDisplayCoroutine(IEnumerator coroutineToStart)
    {
        StopDisplayNotificationCoroutine();
        notificationCoroutine = StartCoroutine(coroutineToStart);
    }
    
    private void StopDisplayNotificationCoroutine()
    {
        if (notificationCoroutine != null)
        {
            StopCoroutine(notificationCoroutine);
            notificationCoroutine = null;
        }
        
        NotificationWindow.SetActive(false);
    }
    
    private IEnumerator DisplayNotification()
    {
        NotificationWindow.SetActive(true);

        ProgressText.text = "Waiting for upload to begin...";
        yield return new WaitUntil(() => veraLogger.UploadProgress > 0); // Wait until we get upload progress
        yield return StartCoroutine(ShowUploadNotificationAndProgress());
    }

    private IEnumerator CloseNotificationGracefully()
    {
        NotificationWindow.SetActive(true);
        yield return new WaitForSeconds(1); // Let the user see the notification so it's not just a flash
        // - most uploads finish almost instantaneously. 
        NotificationWindow.SetActive(false);
    }

    private IEnumerator ShowUploadNotificationAndProgress()
    {
        UpdateProgressText(); // Often times the upload is actually already done, so update text once at least.
        while (veraLogger.UploadProgress < 1)
        {
            UpdateProgressText();
            yield return null;
        }
    }

    private void UpdateProgressText()
    {
        ProgressText.text =
            $"{veraLogger.UploadProgress * veraLogger.uploadFileSizeBytes} / {veraLogger.uploadFileSizeBytes} bytes";
    }
}
