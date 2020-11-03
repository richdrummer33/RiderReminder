using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NotificationSamples;

[RequireComponent(typeof(GameNotificationsManager))]
public class NotificationController : MonoBehaviour
{
    Vector3 lastLocationAndAltitude = Vector3.zero;

    float lastTimestamp;
    bool notificationStarted = false;

    public float driveSpeedThresholdMilesPerHour = 20f; // The speed above which the app will trigger a notification to start the app
    private float driveSpeedMetersPerSecond;
    public float stopSpeedThresholdMilesPerHour = 10f; // The speed below which the tripEndTimer will start counting up
    private float stopSpeedMetersPerSecond;

    public float tripEndTime = 7f; // Time (in minutes) it takes to send the user a notification to check the back seat, after the trip ended
    float tripEndTimeSeconds; //  Time (in seconds) it takes to send the user a notification to check the back seat, after the trip ended
    float tripEndTimer;

    bool endTripNotificationEnabled = false;

    [SerializeField] GameNotificationsManager notificationManager;

    void Start()
    {   
        SetUpMetrics();

#if UNITY_IOS
        notificationManager.Initialize();
        StartCoroutine(StartTrackingGps());
        print("StartTrackingGps() just started");
#elif UNITY_ANDROID
        var channel = new GameNotificationChannel("RiderReminder ", "Rider Reminder Channel", "All notifications");
        notificationManager.Initialize(channel);
#endif

        StartDriveNotification();
    }

    void SetUpMetrics()
    {
        float kph;

        kph = driveSpeedThresholdMilesPerHour * 1.6f;
        driveSpeedMetersPerSecond = kph * 0.277778f;
        print("driveSpeedMetersPerSecond = " + driveSpeedMetersPerSecond);

        kph = stopSpeedThresholdMilesPerHour * 1.6f;
        stopSpeedMetersPerSecond = stopSpeedThresholdMilesPerHour * 0.277778f;
        print("stopSpeedMetersPerSecond = " + stopSpeedMetersPerSecond);

        tripEndTimeSeconds = tripEndTime * 60f; // Convert mins to seconds
        print("tripEndTimeSeconds = " + tripEndTimeSeconds);
    }

    IEnumerator StartTrackingGps() //GPS -- NOTE: MUST HAVE PERMISSIONS ENABLED (LOOK INTO THIS)
    {
#region GPS Initialize

        Input.location.Start(1f, 5f);
        print("Input.location.Start(ed)");

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Service didn't initialize in 20 seconds
        if (maxWait < 1)
        {
            print("Timed out");
            yield break;
        }

        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            print("Unable to determine device location");
            yield break;
        }
        else
        {
            // Access granted and location value could be retrieved
            print("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);
        }

#endregion

        float delay = 1f;

        while (true) // loop forever
        {
            yield return new WaitForSeconds(delay); // just a delay (optional) 

            // Should probably check horizontal and vertical accuracy BEFORE calculating distance and determining speed

            Vector3 newLocationAndAltitude = new Vector3(Input.location.lastData.latitude, Input.location.lastData.longitude, Input.location.lastData.altitude);
            float newTimestamp = (float)Input.location.lastData.timestamp; // Record current time
            print("newTimestamp = " + newTimestamp);

            float latitudeChange = newLocationAndAltitude.x - lastLocationAndAltitude.x; // How many degrees of latitude we have moved
            float longtitudeChange = newLocationAndAltitude.y - lastLocationAndAltitude.y; // How many degrees of longtitude we have moved
            print("latitudeChange = " + newTimestamp + ", longtitudeChange = " + longtitudeChange);

            // Calculate speed ---> Calculate differenec between new position and last position AND new time vs last time to get the speed
            float distance = Mathf.Sqrt(Mathf.Pow(latitudeChange * 111133f, 2) + Mathf.Pow(longtitudeChange * 78847f, 2)); // This is distance traveled since the last GPS location update
            float time = newTimestamp - lastTimestamp;
            print("time = " + time + ", distance = " + distance);

            float currentSpeed = distance / time; // Calculate speed here!
            print("currentSpeed = " + currentSpeed);

            // Take note of location
            lastLocationAndAltitude = newLocationAndAltitude;

            // Take note of the time
            lastTimestamp = newTimestamp;// converting the Input.location.lastData.timestamp to a floating point value and saving that

            if (currentSpeed > driveSpeedMetersPerSecond && !notificationStarted)
            {
                print("We have exceeded the speed threshold! StartDriveNotification started");
                StartDriveNotification();
                notificationStarted = true;
            }
            else if (currentSpeed < stopSpeedMetersPerSecond && notificationStarted && endTripNotificationEnabled)
            {
                tripEndTimer += delay; // Counting timer

                if (tripEndTimer > tripEndTimeSeconds)
                {
                    print("Calling EndTripNotification");
                    EndTripNotification();
                }
            }
            // If speed > some value, run notification code
        }
    }

    public void EnableDisableEndTripReminder(bool isEnabled) // test
    {
        endTripNotificationEnabled = isEnabled;
        print("EnableDisableEndTripReminder = " + isEnabled);
    }

    public void StartNotification()
    {
        print("(Override button) StartDriveNotification started");
        
        StartDriveNotification();

        notificationStarted = true;
    }

    public void EndNotification()
    {
        print("(Override button) Calling EndTripNotification");
        
        EndTripNotification();
    }

    private void StartDriveNotification()
    {
        IGameNotification startNotification = notificationManager.CreateNotification();

        startNotification.Title = "Rider Reminder";
        startNotification.Body = "Click to launch app";

        startNotification.DeliveryTime = System.DateTime.Now.AddSeconds(3f);

        notificationManager.ScheduleNotification(startNotification);
    }

    private void EndTripNotification()
    {
        IGameNotification endNotification = notificationManager.CreateNotification();

        endNotification.Title = "Rider Reminder";
        endNotification.Body = "Check the Rear Seat";

        endNotification.DeliveryTime = System.DateTime.Now.AddSeconds(3f);

        notificationManager.ScheduleNotification(endNotification);
    }
}