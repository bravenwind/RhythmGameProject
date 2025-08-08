using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System.Reflection;
using UnityEngine.SceneManagement;

public class SongManager : MonoBehaviour
{
    [System.Serializable]
    public class SongData
    {
        public Sprite coverImage;
        public string title;
        public string artist;
    }

    public List<SongData> songs;

    public Image leftImage, centerImage, rightImage, extraImage;
    public TextMeshProUGUI centerTitle, centerArtist;

    public float animDuration = 0.5f;

    private Vector3 leftPos = new Vector3(-749.6f, 128.35f, 0);
    private Vector3 centerPos = new Vector3(0, 446.77f, 0);
    private Vector3 rightPos = new Vector3(749.87f, 128.35f, 0);
    private Vector3 offscreenRight = new Vector3(1500f, 128.35f, 0); // extraImage 초기 위치
    private Vector3 offscreenLeft = new Vector3(-1500f, 128.35f, 0);

    private int currentIndex = 0;
    private bool isAnimating = false;

    private void Start()
    {
        UpdateWheelInstantly();
    }

    private void Update()
    {
        if (isAnimating) return;

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            OnRight();
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            OnLeft();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            SelectSong(centerTitle.text, centerArtist.text);
        }
    }

    public void OnLeft()
    {
        if (isAnimating) return;
        isAnimating = true;

        int nextIndex = (currentIndex + 2) % songs.Count;
        extraImage.sprite = songs[nextIndex].coverImage;
        extraImage.rectTransform.localPosition = offscreenRight;
        extraImage.transform.localScale = Vector3.one * 1.0f;
        extraImage.color = new Color(1, 1, 1, 0.5f);
        

        // 애니메이션
        leftImage.rectTransform.DOLocalMoveX(-1500, animDuration).SetEase(Ease.OutCubic);
        centerImage.rectTransform.DOLocalMove(leftPos, animDuration).SetEase(Ease.OutCubic);
        centerImage.rectTransform.DOSizeDelta(new Vector2(500, 500), animDuration).SetEase(Ease.OutCubic);
        centerImage.DOColor(new Color(1, 1, 1, 0.5f), animDuration);

        rightImage.rectTransform.DOLocalMove(centerPos, animDuration).SetEase(Ease.OutCubic);
        rightImage.rectTransform.DOSizeDelta(new Vector2(800, 800), animDuration).SetEase(Ease.OutCubic);
        rightImage.DOColor(new Color(1, 1, 1, 1.0f), animDuration);

        extraImage.rectTransform.DOLocalMove(rightPos, animDuration).SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                currentIndex = (currentIndex + 1) % songs.Count;

                // 역할 교체
                Image oldLeft = leftImage;
                leftImage = centerImage;
                centerImage = rightImage;
                rightImage = extraImage;
                extraImage = oldLeft;

                UpdateWheelInstantly();
                isAnimating = false;
            });
    }

    public void OnRight()
    {
        if (isAnimating) return;
        isAnimating = true;

        int previousIndex = (currentIndex - 2 + songs.Count) % songs.Count;
        extraImage.sprite = songs[previousIndex].coverImage;
        extraImage.rectTransform.localPosition = offscreenLeft;
        extraImage.transform.localScale = Vector3.one * 1.0f;
        extraImage.color = new Color(1, 1, 1, 0.5f);


        // 애니메이션
        rightImage.rectTransform.DOLocalMoveX(1500, animDuration).SetEase(Ease.OutCubic);
        centerImage.rectTransform.DOLocalMove(rightPos, animDuration).SetEase(Ease.OutCubic);
        centerImage.rectTransform.DOSizeDelta(new Vector2(500, 500), animDuration).SetEase(Ease.OutCubic);
        centerImage.DOColor(new Color(1, 1, 1, 0.5f), animDuration);

        leftImage.rectTransform.DOLocalMove(centerPos, animDuration).SetEase(Ease.OutCubic);
        leftImage.rectTransform.DOSizeDelta(new Vector2(800, 800), animDuration).SetEase(Ease.OutCubic);
        leftImage.DOColor(new Color(1, 1, 1, 1.0f), animDuration);

        extraImage.rectTransform.DOLocalMove(leftPos, animDuration).SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                currentIndex = (currentIndex - 1 + songs.Count) % songs.Count;

                // 역할 교체
                Image oldRight = rightImage;
                rightImage = centerImage;
                centerImage = leftImage;
                leftImage = extraImage;
                extraImage = oldRight;

                UpdateWheelInstantly();
                isAnimating = false;
            });
    }

    void UpdateWheelInstantly()
    {
        int leftIdx = (currentIndex - 1 + songs.Count) % songs.Count;
        int rightIdx = (currentIndex + 1) % songs.Count;
        int extraIdx = (currentIndex + 2) % songs.Count;

        leftImage.sprite = songs[leftIdx].coverImage;
        centerImage.sprite = songs[currentIndex].coverImage;
        rightImage.sprite = songs[rightIdx].coverImage;
        extraImage.sprite = songs[extraIdx].coverImage;

        // 위치 초기화
        leftImage.rectTransform.localPosition = leftPos;
        centerImage.rectTransform.localPosition = centerPos;
        rightImage.rectTransform.localPosition = rightPos;
        extraImage.rectTransform.localPosition = offscreenRight;

        // 스케일/투명도 초기화
        leftImage.rectTransform.sizeDelta = new Vector2(500, 500);
        centerImage.rectTransform.sizeDelta = new Vector2(800, 800);
        rightImage.rectTransform.sizeDelta = new Vector2(500, 500);
        extraImage.rectTransform.sizeDelta = new Vector2(500, 500);


        leftImage.color = new Color(1, 1, 1, 0.5f);
        centerImage.color = new Color(1, 1, 1, 1.0f);
        rightImage.color = new Color(1, 1, 1, 0.5f);
        extraImage.color = new Color(1, 1, 1, 0.5f);

        // 텍스트 갱신
        centerTitle.text = songs[currentIndex].title;
        centerArtist.text = songs[currentIndex].artist;
    }

    void SelectSong(string title, string artist)
    {
        GameManager.SelectedSongTitle = title;
        GameManager.SelectedSongArtist = artist;
        SceneManager.LoadScene("MainScene");
    }
}
