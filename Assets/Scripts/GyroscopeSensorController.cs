using FantomLib;
using System;
using UnityEngine.Events;

public class GyroscopeSensorController : SensorControllerBase
{
    protected override SensorType sensorType
    {
        get { return SensorType.Gyroscope; }
    }

    //Callbacks
    [Serializable] public class GyroscopeSensorChangedHandler : UnityEvent<float, float, float> { }   //x, y, z [rad/s]
    public GyroscopeSensorChangedHandler OnGyroscopeSensorChanged;



    //Callback handler for sensor values.
    protected override void ReceiveValues(string json)
    {
        if (string.IsNullOrEmpty(json))
            return;

        base.ReceiveValues(json);

        if (OnGyroscopeSensorChanged != null)
            OnGyroscopeSensorChanged.Invoke(info.values[0], info.values[1], info.values[2]);    //[rad/s]
    }
}
