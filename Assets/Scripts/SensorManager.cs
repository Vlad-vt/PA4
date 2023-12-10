using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class SensorManager
{
    private static SensorManager _instance;

    public static SensorManager GetInstance()
    {
        if( _instance == null )
            _instance = new SensorManager();
        return _instance;
    }

    public delegate void ParameterChanged(Vector3 parameter);

    public event ParameterChanged OnAccelerometerChanged;

    public event ParameterChanged OnMagneticFieldChanged;   

    public event ParameterChanged OnGyroscopeChanged;


    public SensorManager()
    {
        
    }

    public void UpdateAccelerometer(Vector3 parameter)
    {
        OnAccelerometerChanged?.Invoke(parameter);
    }

    public void UpdateMagneticField(Vector3 parameter)
    {
        OnMagneticFieldChanged?.Invoke(parameter);
    }

    public void UpdateGyroscope(Vector3 parameter)
    {
        OnGyroscopeChanged?.Invoke(parameter);
    }
}
