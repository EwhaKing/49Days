using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KeyPanel : MonoBehaviour
{
    [SerializeField] private GameObject keyCellPrefab;
    [SerializeField] private Transform gridParent;

    public event Action<bool> OnComplete;

    private List<char> keySequence = new List<char>();
    private List<GameObject> keyCells = new List<GameObject>();
    private int currentIndex = 0;
    private int mistakeCount = 0;

    public void StartSequence(List<char> sequence)
    {
        keySequence = sequence;
        currentIndex = 0;
        mistakeCount = 0;
        foreach (Transform child in gridParent)
        {
            Destroy(child.gameObject);
        }
        keyCells.Clear();

        foreach (char key in sequence)
        {
            GameObject cell = Instantiate(keyCellPrefab, gridParent);
            var text = cell.GetComponentInChildren<TextMeshProUGUI>();
            text.text = key.ToString();
            keyCells.Add(cell);
        }
    }

    public void ReceiveInput(char input)
    {
        if (input == keySequence[currentIndex])
        {
            keyCells[currentIndex].GetComponent<UnityEngine.UI.Image>().color = Color.green;
            currentIndex++;

            if (currentIndex >= keySequence.Count)
                StartCoroutine(DelayInvoke(true));
        }
        else
        {
            mistakeCount++;
            GameObject cell = keyCells[currentIndex];
            RectTransform rect = cell.GetComponent<RectTransform>();
            Image image = cell.GetComponent<Image>();
            StartCoroutine(IncorrectEffect(rect, image));
            currentIndex++;
            if (mistakeCount >= 4)
            {
                StartCoroutine(DelayInvoke(false));                
            }
            if (currentIndex >= keySequence.Count)
            {
                if (mistakeCount >= 4)
                {
                    StartCoroutine(DelayInvoke(false));
                }
                else
                {
                    StartCoroutine(DelayInvoke(true));
                }
            }
        }
    }

    private IEnumerator DelayInvoke(bool success)
    {
        yield return new WaitForSeconds(0.5f);
        OnComplete?.Invoke(success);
    }

    private IEnumerator IncorrectEffect(RectTransform cellTransform, Image cellImage)
    {
        cellImage.color = Color.red;

        // Cell의 흔들림 변수 설정
        Vector3 originalPos = cellTransform.localPosition;
        float shakeAmount = 10f;
        float shakeDuration = 0.5f;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float xOffset = UnityEngine.Random.Range(-shakeAmount, shakeAmount);
            cellTransform.localPosition = originalPos + new Vector3(xOffset, 0, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 흔들림 후 원래 위치로 복구
        cellTransform.localPosition = originalPos;
    }
}