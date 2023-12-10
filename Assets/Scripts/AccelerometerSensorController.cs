using FantomLib;
using System;
using UnityEngine.Events;

public class AccelerometerSensorController : SensorControllerBase
{
    protected override SensorType sensorType
    {
        get { return SensorType.Accelerometer; }
    }

    //Callbacks
    [Serializable] public class AccelerometerSensorChangedHandler : UnityEvent<float, float, float> { }   //x, y, z [m/s^2]
    public AccelerometerSensorChangedHandler OnAccelerometerSensorChanged;



    //Callback handler for sensor values.
    protected override void ReceiveValues(string json)
    {
        if (string.IsNullOrEmpty(json))
            return;

        base.ReceiveValues(json);

        if (OnAccelerometerSensorChanged != null)
            OnAccelerometerSensorChanged.Invoke(info.values[0], info.values[1], info.values[2]);    //x, y, z [m/s^2]
    }
}
