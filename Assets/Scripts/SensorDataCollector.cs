using TMPro;
using UnityEngine;

public class SensorDataCollector : MonoBehaviour
{
    public TextMeshProUGUI displayAccelerometer;

    public TextMeshProUGUI displayMagneticField;

    public TextMeshProUGUI displayGyroscope;

    private void Start()
    {
        InitDisplayNeedIsSupport();
    }
    private void InitDisplayNeedIsSupport()
    {

    }

    public void OnAccelerometerSensorChanged(float x, float y, float z)
    {
        if (displayAccelerometer != null)
            displayAccelerometer.text = "x=" + x + ", y=" + y + ", z=" + z + " [m/s^2]";
        SensorManager.GetInstance().UpdateAccelerometer(new Vector3(x, y, z));
        
    }

    public void OnMagneticFieldSensorChanged(float x, float y, float z)
    {
        if (displayMagneticField != null)
            displayMagneticField.text = "x=" + x + ", y=" + y + ", z=" + z + " [uT]";
        SensorManager.GetInstance().UpdateMagneticField(new Vector3(x, y, z));
    }

    public void OnGyroscopeSensorChanged(float x, float y, float z)
    {
        if (displayGyroscope != null)
            displayGyroscope.text = "x=" + x + ", y=" + y + ", z=" + z + " [rad/s]";
        SensorManager.GetInstance().UpdateGyroscope(new Vector3(x, y, z));
    }




}
