using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SensorFusion : MonoBehaviour
{
    // ��� � �������
    public Vector3 gyro;  // ��� � ��������
    public Vector3 accel; // ��� � �������������
    public Vector3 mag;   // ��� � ����������


    // ����������� ����������
    public float gyroCoefficient = 0.9f;     // ���������� ��������
    public float accelMagCoefficient = 0.1f; // ���������� ������������� �� ����������

    public TextMeshProUGUI OrientationParameter; // �������� ���� ��� ��������� ��������� ��������

    public TextMeshProUGUI GyroCoef;   // �������� ���� ��� ��������� ����������� ��������
    public TextMeshProUGUI MagAcCoef; // �������� ���� ��� ��������� ����������� ������������� �� ����������

    public Slider CoefSlider; // ������� ��� ���� ����������� ����������

    // Գ��������� ��������
    private Quaternion fusedOrientation;

    private Quaternion gyroOrientation = Quaternion.identity;           // �������� ��������
    private Quaternion accelMagOrientation = Quaternion.identity;       // �������� ������������� �� ����������

    private void Start()
    {
        gyro = Vector3.zero;
        accel = Vector3.zero;
        mag = Vector3.zero;
        // ϳ������ �� ��䳿 ���� ����� �������
        SensorManager.GetInstance().OnAccelerometerChanged += OnAccelerometerChanged;
        SensorManager.GetInstance().OnGyroscopeChanged += OnGyroscopeChanged;
        SensorManager.GetInstance().OnMagneticFieldChanged += OnMagneticFieldChanged;

        Input.gyro.enabled = true; // ��������� �������� �����

        CoefSlider.onValueChanged.AddListener(OnSliderValueChanged); // ��������� ������� ��� ��������
        CoefSlider.value = gyroCoefficient; // ��������� �������� ��������
    }

    private void OnSliderValueChanged(float value)
    {
        accelMagCoefficient = 1f - value;
        gyroCoefficient = value;

        // ��������� ������� ����������� � ���������� �� ������ ����� ���� ����
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

        // ��������� ������� �������� � �������� ����
        OrientationParameter.text = $"X: {transform.rotation.eulerAngles.x} Y: {transform.rotation.eulerAngles.y} Z: {transform.rotation.eulerAngles.z}";
    }

    private void CalculateFusedOrientation()
    {
        // ���������� ������� ��������� � ����� ������������� �� ����������
        float[] rotationMatrix = GetRotationMatrix(accel, mag);

        // ��������� ���� �������� � ������� ���������
        float[] orientationAngles = GetOrientation(rotationMatrix);

        // ����������� ���� �������� � ���������
        accelMagOrientation = Quaternion.Euler(orientationAngles[0], orientationAngles[1], orientationAngles[2]);

        gyroOrientation = Input.gyro.attitude;

        // Գ�������� �������� �� ��������� ���������������� �������
        fusedOrientation = Quaternion.Slerp(Quaternion.identity, gyroOrientation, gyroCoefficient) *
            Quaternion.Slerp(Quaternion.identity, accelMagOrientation, accelMagCoefficient);

        transform.rotation = fusedOrientation; // ������������ ��������� �������� �� ��'����
    }

    private float[] GetRotationMatrix(Vector3 accelerometer, Vector3 magnetometer)
    {
        // ���������� ������� �������� ��������� ����
        Vector3 magneticFieldDirection = magnetometer.normalized;

        // ���������� ������� �������� ���������
        Vector3 gravityDirection = accelerometer.normalized;

        // ���������� ���������� ������� ������� �������� ��������� ���� �� ���������
        Vector3 crossProduct = Vector3.Cross(magneticFieldDirection, gravityDirection);

        // ���������� �������� ���������� �������
        float crossProductMagnitude = crossProduct.magnitude;

        // ���������� ���� �� �������� �������� ��������� ���� �� ������������� �������� ���������
        float rotationAngle1 = (float)Math.Atan2(crossProductMagnitude, Vector3.Dot(magneticFieldDirection, gravityDirection));

        // ���������� ���� �� �������� �������� ��������� �� ������������� �������� ���������
        float rotationAngle2 = (float)Math.Atan2(gravityDirection.y, -gravityDirection.x);

        // �������� ������� ��������� �� ����� ������
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
        // ���������� ���� �����
        float rollAngle = (float)Math.Atan2(rotationMatrix[7], rotationMatrix[8]);

        // ���������� ���� �������
        float pitchAngle = (float)Math.Atan2(-rotationMatrix[2], Math.Sqrt(rotationMatrix[0] * rotationMatrix[0] + rotationMatrix[1] * rotationMatrix[1]));

        // ���������� ���� ����
        float yawAngle = (float)Math.Atan2(rotationMatrix[6], rotationMatrix[5]);

        // ���������� ���� ��������
        float[] orientationAngles = new float[3];
        orientationAngles[0] = rollAngle;
        orientationAngles[1] = pitchAngle;
        orientationAngles[2] = yawAngle;

        return orientationAngles;
    }
}