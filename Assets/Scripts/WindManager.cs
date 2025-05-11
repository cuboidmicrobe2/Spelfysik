using UnityEngine;

public class WindManager : MonoBehaviour
{
    float windDelta = 0.1f;
    float windUpdateTime = 0.5f;
    float windStartMaxMagnitude = 3.0f;
    public float windMaxMagnitude = 10.0f;

    [SerializeField]
    Vector2 wind;
    Vector2 windUpdated;
    float time;

    public Vector2 GetWind()
    {
        return wind;
    }

    public void SetWind(Vector2 newWind)
    {
        wind = newWind;
        windUpdated = wind;
        time = 0.0f;
    }

    private void UpdateWind()
    {
        wind = Vector2.Lerp(wind, windUpdated, Time.deltaTime);
    }

    private Vector2 GetUpdatedWind()
    {
        Vector2 result = wind + Random.insideUnitCircle * windDelta;
        result = result.normalized * Mathf.Min(result.magnitude, windMaxMagnitude);
        return result;
    }

    void Start()
    {
        SetWind(Random.insideUnitCircle * windStartMaxMagnitude);
    }


    void Update()
    {
        UpdateWind();
        time += Time.deltaTime;

        if (time > windUpdateTime)
        {
            time = 0.0f;
            windUpdated = GetUpdatedWind();
        }
    }
}
