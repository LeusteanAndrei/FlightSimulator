using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private Fade fade;

    [SerializeField]
    private Transform ship;

    [SerializeField]
    private GameObject shipThruster;

    [SerializeField]
    private float launchDistance = 20f;

    [SerializeField]
    private float moveDuration = 4f;

    public void Launch()
    {
        StartCoroutine(LaunchSequence());
    }

    private IEnumerator LaunchSequence()
    {
        // Enable thruster particles
        if (shipThruster != null)
        {
            shipThruster.SetActive(true);
        }

        Vector3 startPos = ship.position;

        // Move forward relative to ship rotation
        Vector3 targetPos = startPos + (ship.forward * launchDistance);

        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / moveDuration);

            // Ease-in acceleration
            float easedT = t * t;

            ship.position = Vector3.Lerp(startPos, targetPos, easedT);

            yield return null;
        }

        ship.position = targetPos;

        // Fade after movement finishes
        fade.BeginFade(() =>
        {
            SceneManager.LoadScene(
                SceneManager.GetActiveScene().buildIndex + 1
            );
        });
    }
}