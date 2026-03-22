using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TrapZoneController : MonoBehaviour
{
    public static bool InTrapZone = false;
    public string traptype = string.Empty;
    public GameObject beam;
    public bool keepBeamVisible = true;
    public string mode = "default";
    //public GameObject FrontShield;
    //public GameObject BackShield;

    [Header("Animation Settings")]
    public float riseHeight = 5f;
    public float riseSpeed = 4f;
    public float delayBetweenObjects = 0.2f;

    private Vector3 beamHiddenPos;
    private Vector3 beamVisiblePos;

    //private Vector3 frontHiddenPos;
    //private Vector3 frontVisiblePos;

    //private Vector3 backHiddenPos;
    //private Vector3 backVisiblePos;

    private Coroutine activeRoutine;

    private void Start()
    {
        SetupObject(beam, out beamHiddenPos, out beamVisiblePos);
        //SetupObject(FrontShield, out frontHiddenPos, out frontVisiblePos);
        //SetupObject(BackShield, out backHiddenPos, out backVisiblePos);
    }

    private void SetupObject(GameObject obj, out Vector3 hidden, out Vector3 visible)
    {
        visible = obj.transform.position;
        hidden = visible - Vector3.up * riseHeight;

        if (keepBeamVisible)
        {
            obj.transform.position = visible;
            obj.SetActive(true);
        }
        else
        {
            obj.transform.position = hidden;
            obj.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            InTrapZone = true;
            if(mode == "tutorial")
            {
                TutorialManager.Instance.ShowPopup("Watch out for the Laser Beam!", 3.5f);
            }
            if (activeRoutine != null)
                StopCoroutine(activeRoutine);

            activeRoutine = StartCoroutine(ActivateTrap());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            InTrapZone = false;

            if (activeRoutine != null)
                StopCoroutine(activeRoutine);

            activeRoutine = StartCoroutine(DeActivateTrap());
            string sceneName = SceneManager.GetActiveScene().name;
            if(sceneName == "Traps-Prototype" && TutorialManager.Instance != null)
            {
                TutorialManager.Instance.ShowPopup("Congrats you made it!", 3.5f);
            }
        }
    }

    private void OnDisable()
    {
        InTrapZone = false;
    }

    private IEnumerator ActivateTrap()
    {
        if (traptype != "beam")
            yield break;

        //yield return StartCoroutine(RaiseObject(FrontShield, frontHiddenPos, frontVisiblePos));
        //yield return new WaitForSeconds(delayBetweenObjects);

        //yield return StartCoroutine(RaiseObject(BackShield, backHiddenPos, backVisiblePos));
        //yield return new WaitForSeconds(delayBetweenObjects);

        if (keepBeamVisible)
        {
            if (beam != null) beam.SetActive(true);
            var controller = beam != null ? beam.GetComponent<BeamController>() : null;
            controller?.ActivateBeam();
        }
        else
        {
            yield return StartCoroutine(RaiseObject(beam, beamHiddenPos, beamVisiblePos));
            var controller = beam != null ? beam.GetComponent<BeamController>() : null;
            controller?.ActivateBeam();
        }
    }
    private IEnumerator DeActivateTrap()
    {
        var controller = beam != null ? beam.GetComponent<BeamController>() : null;
        controller?.DeactivateBeam();

        if (!keepBeamVisible)
        {
            yield return StartCoroutine(LowerObject(beam, beamVisiblePos, beamHiddenPos));
            yield return new WaitForSeconds(delayBetweenObjects);
        }
        else
        {
            if (beam != null)
                beam.transform.position = beamVisiblePos;
        }

        //yield return StartCoroutine(LowerObject(BackShield, backVisiblePos, backHiddenPos));
        //yield return new WaitForSeconds(delayBetweenObjects);

        //yield return StartCoroutine(LowerObject(FrontShield, frontVisiblePos, frontHiddenPos));
    }

    private IEnumerator RaiseObject(GameObject obj, Vector3 from, Vector3 to)
    {
        obj.SetActive(true);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * riseSpeed;
            obj.transform.position = Vector3.Lerp(from, to, t);
            yield return null;
        }

        obj.transform.position = to;
    }

    private IEnumerator LowerObject(GameObject obj, Vector3 from, Vector3 to)
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * riseSpeed;
            obj.transform.position = Vector3.Lerp(from, to, t);
            yield return null;
        }

        obj.transform.position = to;
        obj.SetActive(false);
    }
}