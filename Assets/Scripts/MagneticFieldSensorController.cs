using FantomLib;
using System;
using UnityEngine.Events;

public class MagneticFieldSensorController : SensorControllerBase
{
    protected override SensorType sensorType
    {
        get { return SensorType.MagneticField; }
    }

    //Callbacks
    [Serializable] public class MagneticFieldSensorChangedHandler : UnityEvent<float, float, float> { }   //x, y, z [uT]
    public MagneticFieldSensorChangedHandler OnMagneticFieldSensorChanged;



    //Callback handler for sensor values.
    protected override void ReceiveValues(string json)
    {
        if (string.IsNullOrEmpty(json))
            return;

        base.ReceiveValues(json);

        if (OnMagneticFieldSensorChanged != null)
            OnMagneticFieldSensorChanged.Invoke(info.values[0], info.values[1], info.values[2]);    //[uT]
    }
}
