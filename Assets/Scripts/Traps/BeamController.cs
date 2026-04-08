using UnityEngine;

public class BeamController : MonoBehaviour
{
    public float rotationSpeed = 30f;
    public bool isActive = false;

    public Transform[] beamEmitters;
    public Transform[] laserVisual;

    public float maxDistance = 20f;

    void Update()
    {
        if (!isActive) return;

        Rotate();
        ShootBeams();
        FlickerEffect();
    }

    void Rotate()
    {
        float wobble = Mathf.Sin(Time.time * 5f) * 0.2f;
        transform.Rotate(Vector3.up * (rotationSpeed + wobble) * Time.deltaTime);
    }

    void ShootBeams()
    {
        for (int i = 0; i < beamEmitters.Length; i++)
        {
            Ray ray = new Ray(beamEmitters[i].position, beamEmitters[i].forward);
            RaycastHit hit;

            float distance = maxDistance;

            if (Physics.Raycast(ray, out hit, maxDistance))
            {
                distance = hit.distance;

                //if (hit.collider.CompareTag("Player"))
                //{
                //    Debug.Log("Hit Player from beam index: " + i);
                //}
            }

            UpdateLaserVisual(i, distance);
        }
    }

    void UpdateLaserVisual(int index, float distance)
    {
        Transform laser = laserVisual[index];

        Vector3 scale = laser.localScale;
        scale.z = distance * 1.5f;
        laser.localScale = scale;

        Vector3 pos = laser.localPosition;
        pos.z = distance/2f;
        //laser.localPosition = pos;
    }

    void FlickerEffect()
    {
        for (int i = 0; i < laserVisual.Length; i++)
        {
            float flicker = 1f + Mathf.Sin(Time.time * 50f) * 0.05f;

            Vector3 scale = laserVisual[i].localScale;
            scale.x = 0.2f * flicker;
            scale.y = 0.2f * flicker;

            laserVisual[i].localScale = scale;
        }
    }

    public void ActivateBeam()
    {
        isActive = true;

        for (int i = 0; i < laserVisual.Length; i++)
        {
            laserVisual[i].gameObject.SetActive(true);
        }
    }

    public void DeactivateBeam()
    {
        isActive = false;

        for (int i = 0; i < laserVisual.Length; i++)
        {
            laserVisual[i].gameObject.SetActive(false);
        }
    }
}