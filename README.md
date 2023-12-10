# Lab4

Дана лабораторна робота була виконана за допомогою мови програмування C# та Unity 2022.3.12f1

Для встановлення програми на Android та тесту **lab4.apk**

Подивитися відео з поясненнями та результатом роботи програми, можно завантажив файл **lab4.mp4**

<div align="center">

**Що було реалізовано:**

</div>

Мені потрібно було реалізувати обертання поверхні на основі показань 3 датчиків апаратного магнітометра, гіроскопа та акселерометру. Для зчитування коректних даних з датчиків, був використаний Android Native Plugin, завдяки якому можна було зчитати усі дані коректно з пристрою.
Нижче наведений основний код реалізації, в якому уже враховано зчитування коректних даних з усіх датчиків

<div align="center">

**Оновний код і реалізація обертання завдяки fusion orientation**

</div>

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SensorFusion : MonoBehaviour
{
    // Sensor data
    public Vector3 gyro;  // Gyroscope data
    public Vector3 accel; // Accelerometer data
    public Vector3 mag;   // Magnetometer data

    // Filtering coefficients
    public float gyroCoefficient = 0.9f;     // Gyroscope coefficient
    public float accelMagCoefficient = 0.1f; // Accelerometer and magnetometer coefficient

    public TextMeshProUGUI OrientationParameter; // Text field for displaying orientation parameters

    public TextMeshProUGUI GyroCoef;   // Text field for displaying gyroscope coefficient
    public TextMeshProUGUI MagAcCoef; // Text field for displaying accelerometer and magnetometer coefficient

    public Slider CoefSlider; // Slider for changing filtering coefficient

    // Filtered orientation
    private Quaternion fusedOrientation;

    private Quaternion gyroOrientation = Quaternion.identity;           // Gyroscope orientation
    private Quaternion accelMagOrientation = Quaternion.identity;       // Accelerometer and magnetometer orientation

    private void Start()
    {
        gyro = Vector3.zero;
        accel = Vector3.zero;
        mag = Vector3.zero;

        // Subscribe to sensor data change events
        SensorManager.GetInstance().OnAccelerometerChanged += OnAccelerometerChanged;
        SensorManager.GetInstance().OnGyroscopeChanged += OnGyroscopeChanged;
        SensorManager.GetInstance().OnMagneticFieldChanged += OnMagneticFieldChanged;

        Input.gyro.enabled = true; // Enable input gyroscope

        CoefSlider.onValueChanged.AddListener(OnSliderValueChanged); // Add listener for slider
        CoefSlider.value = gyroCoefficient; // Initial slider value
    }

    private void OnSliderValueChanged(float value)
    {
        accelMagCoefficient = 1f - value;
        gyroCoefficient = value;

        // Display coefficients with limiting to one decimal place
        GyroCoef.text = $"gyro: {gyroCoefficient:F2}";
        MagAcCoef.text = $"magac: {accelMagCoefficient:F2}";
    }

    private void OnAccelerometerChanged(Vector3 parameter)
    {
        accel = parameter;
    }

    private void OnGyroscopeChanged(Vector3 parameter)
    {
        gyro = parameter;
    }

    private void OnMagneticFieldChanged(Vector3 parameter)
    {
        mag = parameter;
    }

    private void Update()
    {
        CalculateFusedOrientation();

        // Display orientation values in the text field
        OrientationParameter.text = $"X: {transform.rotation.eulerAngles.x} Y: {transform.rotation.eulerAngles.y} Z: {transform.rotation.eulerAngles.z}";
    }

    private void CalculateFusedOrientation()
    {
        // Calculate rotation matrix from accelerometer and magnetometer data
        float[] rotationMatrix = GetRotationMatrix(accel, mag);

        // Get orientation angles from the rotation matrix
        float[] orientationAngles = GetOrientation(rotationMatrix);

        // Convert orientation angles to a quaternion
        accelMagOrientation = Quaternion.Euler(orientationAngles[0], orientationAngles[1], orientationAngles[2]);

        gyroOrientation = Input.gyro.attitude;

        // Filter orientation using complementary filter
        fusedOrientation = Quaternion.Slerp(Quaternion.identity, gyroOrientation, gyroCoefficient) *
            Quaternion.Slerp(Quaternion.identity, accelMagOrientation, accelMagCoefficient);

        transform.rotation = fusedOrientation; // Apply calculated orientation to the object
    }

    private float[] GetRotationMatrix(Vector3 accelerometer, Vector3 magnetometer)
    {
        // Calculate magnetic field direction vector
        Vector3 magneticFieldDirection = magnetometer.normalized;

        // Calculate gravity direction vector
        Vector3 gravityDirection = accelerometer.normalized;

        // Calculate cross product of magnetic field and gravity direction vectors
        Vector3 crossProduct = Vector3.Cross(magneticFieldDirection, gravityDirection);

        // Calculate magnitude of the cross product
        float crossProductMagnitude = crossProduct.magnitude;

        // Calculate angle between the magnetic field direction vector and the normalized rotation vector
        float rotationAngle1 = (float)Math.Atan2(crossProductMagnitude, Vector3.Dot(magneticFieldDirection, gravityDirection));

        // Calculate angle between the gravity direction vector and the normalized rotation vector
        float rotationAngle2 = (float)Math.Atan2(gravityDirection.y, -gravityDirection.x);

        // Build rotation matrix from the three angles
        float[] rotationMatrix = new float[9];
        rotationMatrix[0] = (float)(Math.Cos(rotationAngle2) * Math.Cos(rotationAngle1));
        rotationMatrix[1] = (float)(Math.Sin(rotationAngle2) * Math.Cos(rotationAngle1));
        rotationMatrix[2] = -crossProductMagnitude * (float)Math.Sin(rotationAngle1);
        rotationMatrix[3] = (float)(-Math.Sin(rotationAngle2) * Math.Sin(rotationAngle1));
        rotationMatrix[4] = (float)(Math.Cos(rotationAngle2) * Math.Sin(rotationAngle1));
        rotationMatrix[5] = crossProductMagnitude * (float)Math.Cos(rotationAngle1);
        rotationMatrix[6] = (float)(Math.Cos(rotationAngle2) * Math.Sin(rotationAngle2));
        rotationMatrix[7] = (float)(-Math.Sin(rotationAngle2) * Math.Sin(rotationAngle2));
        rotationMatrix[8] = (float)Math.Cos(rotationAngle2);

        return rotationMatrix;
    }

    private float[] GetOrientation(float[] rotationMatrix)
    {
        // Calculate roll angle
        float rollAngle = (float)Math.Atan2(rotationMatrix[7], rotationMatrix[8]);

        // Calculate pitch angle
        float pitchAngle = (float)Math.Atan2(-rotationMatrix[2], Math.Sqrt(rotationMatrix[0] * rotationMatrix[0] + rotationMatrix[1] * rotationMatrix[1]));

        // Calculate yaw angle
        float yawAngle = (float)Math.Atan2(rotationMatrix[6], rotationMatrix[5]);

        // Return orientation angles
        float[] orientationAngles = new float[3];
        orientationAngles[0] = rollAngle;
        orientationAngles[1] = pitchAngle;
        orientationAngles[2] = yawAngle;

        return orientationAngles;
    }
}
