using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TrapZoneController : MonoBehaviour
{
    public static bool InTrapZone = false;
    public string traptype = string.Empty;
    public GameObject beam;
    public bool keepBeamVisible = false;
    public string mode = "default";

    [Header("Animation Settings")]
    public float riseHeight = 5f;
    public float riseSpeed = 4f;
    public float delayBetweenObjects = 0.2f;

    private Vector3 beamHiddenPos;
    private Vector3 beamVisiblePos;

    private bool playerInsideZone = false;
    private bool trapTemporarilyDisabled = false;

    private Coroutine activeRoutine;

    private void Start()
    {
        SetupObject(beam, out beamHiddenPos, out beamVisiblePos);

        if (traptype == "beam")
        {
            beam.transform.position = beamVisiblePos;
            beam.SetActive(true);
            beam.GetComponent<BeamController>()?.ActivateBeam();
        }
    }

    private void Update()
    {
        if (!playerInsideZone) return;

        if (Input.GetKeyDown(KeyCode.F) && !trapTemporarilyDisabled)
        {
            if (TrapCombatAgentManager.TryActivate("beam", 5f))
            {
                TutorialManager.Instance?.OnTrapDeactivated("beam");
                if (activeRoutine != null)
                    StopCoroutine(activeRoutine);

                activeRoutine = StartCoroutine(DisableTrapWhileAgentActive());
            }
            else if (mode == "tutorial" && TutorialManager.Instance != null)
            {
                TutorialManager.Instance.ShowPopup("You need a Deactivating Agent first.", 2.5f);
            }
        }
    }

    private IEnumerator DisableTrapWhileAgentActive()
    {
        trapTemporarilyDisabled = true;

        yield return StartCoroutine(DeActivateTrap());

        yield return new WaitUntil(() => !TrapCombatAgentManager.IsActiveFor("beam"));

        if (playerInsideZone)
            yield return StartCoroutine(ActivateTrap());

        trapTemporarilyDisabled = false;
        activeRoutine = null;
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
        if (!other.CompareTag("Player"))
            return;

        InTrapZone = true;
        playerInsideZone = true;

        if (mode == "tutorial" && TutorialManager.Instance != null)
        {
            TutorialManager.Instance.HideTutorialArrow();
        }

        if (!trapTemporarilyDisabled)
            return; // beam is already visible; only re-activate if it was F-key disabled

        if (activeRoutine != null)
            StopCoroutine(activeRoutine);

        activeRoutine = StartCoroutine(ActivateTrap());
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        InTrapZone = false;
        playerInsideZone = false;

        // Don't hide/lower the beam on exit — beam stays always visible

    }

    private void OnDisable()
    {
        InTrapZone = false;
    }

    private IEnumerator ActivateTrap()
    {
        if (traptype != "beam")
            yield break;


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