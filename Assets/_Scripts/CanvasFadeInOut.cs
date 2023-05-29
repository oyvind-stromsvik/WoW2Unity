using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CanvasFadeInOut : MonoBehaviour
{
    public List<Image> images = new List<Image>();
    public List<TextMeshProUGUI> texts = new List<TextMeshProUGUI>();
    public float currentAlpha;
    public bool fadeOut, fadeIn;

    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform child in GetComponentsInChildren<Transform>()) {
            Image image = child.gameObject.GetComponent<Image>();
            if (image != null) {
                images.Add(image);
            }
            TextMeshProUGUI text = child.gameObject.GetComponent<TextMeshProUGUI>();            
            if (text != null) {
                texts.Add(text);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (Input.GetKeyDown(KeyCode.Q)) {
            FadeOut();
        }
        if (Input.GetKeyDown(KeyCode.W)) {
            FadeIn();
        }*/

        if (fadeOut) {
            if (currentAlpha > 0f) {
                currentAlpha -= 1f * Time.deltaTime;
                foreach (Image i in images) {
                    i.color = new Color(i.color.r, i.color.g, i.color.b, currentAlpha);
                }
                foreach (TextMeshProUGUI i in texts) {
                    i.color = new Color(i.color.r, i.color.g, i.color.b, currentAlpha);
                }
            }
            else {
                fadeOut = false;
                currentAlpha = 0f;
            }
        }

        if (fadeIn) {
            if (currentAlpha < 1f) {
                currentAlpha += 1f * Time.deltaTime;
                foreach (Image i in images) {
                    i.color = new Color(1f, 1f, 1f, currentAlpha);
                }
                foreach (TextMeshProUGUI i in texts) {
                    i.color = new Color(i.color.r, i.color.g, i.color.b, currentAlpha);
                }
            }
            else {
                fadeIn = false;
                currentAlpha = 1f;
            }
        }
    }

    // Fade out an entire UI
    public void FadeOut() {
        fadeOut = true;
    }

    public void FadeIn() {
        fadeIn = true;
    }
}
