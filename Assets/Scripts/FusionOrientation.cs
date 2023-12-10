using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SensorFusion : MonoBehaviour
{
    // Дані з сенсорів
    public Vector3 gyro;  // Дані з гіроскопа
    public Vector3 accel; // Дані з акселерометра
    public Vector3 mag;   // Дані з магнітомера


    // Коефіцієнти фільтрації
    public float gyroCoefficient = 0.9f;     // Коефіцієнт гіроскопа
    public float accelMagCoefficient = 0.1f; // Коефіцієнт акселерометра та магнітомера

    public TextMeshProUGUI OrientationParameter; // Текстове поле для виведення параметрів орієнтації

    public TextMeshProUGUI GyroCoef;   // Текстове поле для виведення коефіцієнта гіроскопа
    public TextMeshProUGUI MagAcCoef; // Текстове поле для виведення коефіцієнта акселерометра та магнітомера

    public Slider CoefSlider; // Слайдер для зміни коефіцієнта фільтрації

    // Фільтрована орієнтація
    private Quaternion fusedOrientation;

    private Quaternion gyroOrientation = Quaternion.identity;           // Орієнтація гіроскопа
    private Quaternion accelMagOrientation = Quaternion.identity;       // Орієнтація акселерометра та магнітомера

    private void Start()
    {
        gyro = Vector3.zero;
        accel = Vector3.zero;
        mag = Vector3.zero;
        // Підписка на події зміни даних сенсорів
        SensorManager.GetInstance().OnAccelerometerChanged += OnAccelerometerChanged;
        SensorManager.GetInstance().OnGyroscopeChanged += OnGyroscopeChanged;
        SensorManager.GetInstance().OnMagneticFieldChanged += OnMagneticFieldChanged;

        Input.gyro.enabled = true; // Включення гіроскопа вводу

        CoefSlider.onValueChanged.AddListener(OnSliderValueChanged); // Додавання слухача для слайдера
        CoefSlider.value = gyroCoefficient; // Початкове значення слайдера
    }

    private void OnSliderValueChanged(float value)
    {
        accelMagCoefficient = 1f - value;
        gyroCoefficient = value;

        // Виведення значень коефіцієнтів з обмеженням до одного знаку після коми
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

        // Виведення значень орієнтації в текстове поле
        OrientationParameter.text = $"X: {transform.rotation.eulerAngles.x} Y: {transform.rotation.eulerAngles.y} Z: {transform.rotation.eulerAngles.z}";
    }

    private void CalculateFusedOrientation()
    {
        // Розрахунок матриці обертання з даних акселерометра та магнітомера
        float[] rotationMatrix = GetRotationMatrix(accel, mag);

        // Отримання кутів орієнтації з матриці обертання
        float[] orientationAngles = GetOrientation(rotationMatrix);

        // Конвертація кутів орієнтації в кватерніон
        accelMagOrientation = Quaternion.Euler(orientationAngles[0], orientationAngles[1], orientationAngles[2]);

        gyroOrientation = Input.gyro.attitude;

        // Фільтрація орієнтації за допомогою комплементарного фільтра
        fusedOrientation = Quaternion.Slerp(Quaternion.identity, gyroOrientation, gyroCoefficient) *
            Quaternion.Slerp(Quaternion.identity, accelMagOrientation, accelMagCoefficient);

        transform.rotation = fusedOrientation; // Застосування обчисленої орієнтації до об'єкта
    }

    private float[] GetRotationMatrix(Vector3 accelerometer, Vector3 magnetometer)
    {
        // Розрахунок вектора напрямку магнітного поля
        Vector3 magneticFieldDirection = magnetometer.normalized;

        // Розрахунок вектора напрямку гравітації
        Vector3 gravityDirection = accelerometer.normalized;

        // Розрахунок векторного добутку векторів напрямку магнітного поля та гравітації
        Vector3 crossProduct = Vector3.Cross(magneticFieldDirection, gravityDirection);

        // Розрахунок величини векторного добутку
        float crossProductMagnitude = crossProduct.magnitude;

        // Розрахунок кута між вектором напрямку магнітного поля та нормалізованим вектором обертання
        float rotationAngle1 = (float)Math.Atan2(crossProductMagnitude, Vector3.Dot(magneticFieldDirection, gravityDirection));

        // Розрахунок кута між вектором напрямку гравітації та нормалізованим вектором обертання
        float rotationAngle2 = (float)Math.Atan2(gravityDirection.y, -gravityDirection.x);

        // Побудова матриці обертання за трема кутами
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
        // Розрахунок кута крену
        float rollAngle = (float)Math.Atan2(rotationMatrix[7], rotationMatrix[8]);

        // Розрахунок кута тангажу
        float pitchAngle = (float)Math.Atan2(-rotationMatrix[2], Math.Sqrt(rotationMatrix[0] * rotationMatrix[0] + rotationMatrix[1] * rotationMatrix[1]));

        // Розрахунок кута риля
        float yawAngle = (float)Math.Atan2(rotationMatrix[6], rotationMatrix[5]);

        // Повернення кутів орієнтації
        float[] orientationAngles = new float[3];
        orientationAngles[0] = rollAngle;
        orientationAngles[1] = pitchAngle;
        orientationAngles[2] = yawAngle;

        return orientationAngles;
    }
}