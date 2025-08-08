using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using static GameManager;

[System.Serializable]
public class NoteData
{
    public string type;
    public int time;
    public int lane;
    public int duration; // long일 경우만 사용

}

[System.Serializable]
public class NoteChart
{
    public string title;
    public string artist;
    public float bpm;
    public int note_count;
    public List<NoteData> notes;
}

public enum Judgement
{
    Perfect,
    Great,
    Bad,
    Miss
}

public class GameManager : MonoBehaviour
{
    public string songTitle;
    public string songArtist;
    public AudioSource audioSource;
    public Transform[] lanes;                  // 각 레인의 위치 (0~3 등)
    public GameObject tapNotePrefab;           // 탭 노트 프리팹
    public GameObject longNotePrefab;          // 롱 노트 프리팹
    public TextMeshProUGUI comboText;
    public float spawnOffset = 2000f;          // 미리 생성할 시간(ms) ex. 2초 전

    private NoteChart chart;
    private bool[] noteSpawned;   // 노트가 생성됐는지 여부 저장
    public static GameManager Instance;
    public static string SelectedSongTitle;
    public static string SelectedSongArtist;

    public Transform judgementLine;      // 에디터에서 드래그
    public float scrollSpeed = 10f;       // 초당 유닛 이동

    private int score = 0;

    private int perfectCount = 0;
    private int greatCount = 0;
    private int badCount = 0;
    private int missCount = 0;
    private int comboCount = 0;
    public Sprite[] judgementImages;
    public SpriteRenderer judgementImageRenderer;

    public Judgement JudgeNoteTiming(float deltaTime)
    {
        float absTime = Mathf.Abs(deltaTime);

        if (absTime <= 0.05f) return Judgement.Perfect;
        else if (absTime <= 0.1f) return Judgement.Great;
        else if (absTime <= 0.2f) return Judgement.Bad;
        else return Judgement.Miss;
    }

    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        songTitle = SelectedSongTitle;
        songArtist = SelectedSongArtist;

        // JSON 파일 로드
        TextAsset json = Resources.Load<TextAsset>(songTitle.ToString() + " - " + songArtist.ToString()+"/" + "EZ");
        chart = JsonUtility.FromJson<NoteChart>(json.text);

        // 생성 여부 배열 초기화
        noteSpawned = new bool[chart.notes.Count];

        // 노래 재생
        audioSource.Play();
    }

    void Update()
    {
        float songTime = audioSource.time * 1000f; // 현재 시간(ms)

        for (int i = 0; i < chart.notes.Count; i++)
        {
            NoteData note = chart.notes[i];

            // 아직 생성되지 않았고, 생성 타이밍이 되면 생성
            if (!noteSpawned[i] && note.time <= songTime + spawnOffset)
            {
                SpawnNote(note);
                noteSpawned[i] = true;
            }
        }

        // 키 입력 체크
        if (Input.GetKeyDown(KeyCode.D)) CheckHit(0); // 레인 0
        if (Input.GetKeyDown(KeyCode.F)) CheckHit(1); // 레인 1
        if (Input.GetKeyDown(KeyCode.J)) CheckHit(2); // 레인 2
        if (Input.GetKeyDown(KeyCode.K)) CheckHit(3); // 레인 3

    }

    void SpawnNote(NoteData note)
    {
        GameObject prefab = note.type == "long" ? longNotePrefab : tapNotePrefab;

        Transform laneTransform = lanes[note.lane];
        GameObject noteObj = Instantiate(prefab);
        noteObj.GetComponent<NoteBehavior>().Init(note);
    }

    public static int CalcScore
    (
    int allNoteCount,      // 패턴 상에 나타나는 모든 노트의 수 (단노트 1, 롱노트 2로 합산)
    int perfectNoteCount,  // 퍼펙트 판정 노트 개수
    int greatNoteCount,    // 그레잇 판정 노트 개수
    int badNoteCount,      // 배드 판정 노트 개수
    int missNoteCount      // 미스 판정 노트 개수
    )
    {
        int maxScore = 1000000;
        float[] weight = { 1.0f, 0.7f, 0.3f, 0.0f };

        float score =
            (maxScore / (float)allNoteCount * weight[0] * perfectNoteCount) +
            (maxScore / (float)allNoteCount * weight[1] * greatNoteCount) +
            (maxScore / (float)allNoteCount * weight[2] * badNoteCount) +
            (maxScore / (float)allNoteCount * weight[3] * missNoteCount);

        return Mathf.RoundToInt(score);
    }

    void CheckHit(int lane)
    {
        float currentTime = audioSource.time * 1000f;

        GameObject[] notes = GameObject.FindGameObjectsWithTag("Note");

        GameObject closestNote = null;
        float closestTime = float.MaxValue;

        foreach (GameObject noteObj in notes)
        {
            NoteBehavior nb = noteObj.GetComponent<NoteBehavior>();
            if (nb.Lane != lane) continue;

            float noteTime = nb.SpawnTime;
            float delta = (noteTime - currentTime) / 1000f;

            float absDelta = Mathf.Abs(delta);
            if (absDelta < closestTime)
            {
                closestTime = absDelta;
                closestNote = noteObj;
            }
        }

        if (closestNote != null && closestTime <= 0.2f)
        {
            NoteBehavior nb = closestNote.GetComponent<NoteBehavior>();
            float deltaTime = (closestNote.GetComponent<NoteBehavior>().SpawnTime - currentTime) / 1000f;
            Judgement result = JudgeNoteTiming(deltaTime);

            switch (result)
            {
                case Judgement.Perfect:
                    perfectCount++;
                    comboCount++;
                    judgementImageRenderer.sprite = judgementImages[0];
                    comboText.text = comboCount.ToString() + " Combo!";
                    break;
                case Judgement.Great:
                    greatCount++;
                    comboCount++;
                    judgementImageRenderer.sprite = judgementImages[1];
                    comboText.text = comboCount.ToString() + " Combo!";
                    break;
                case Judgement.Bad:
                    badCount++;
                    comboCount = 0;
                    judgementImageRenderer.sprite = judgementImages[2];
                    comboText.text = "";
                    break;
                case Judgement.Miss:
                    break;
            }

            if (nb.NoteType == "long")
            {
                nb.StartHold(); // 누르기 시작만 하고 안 지움
            }
            else
            {
                Destroy(closestNote); // 일반 노트만 제거
            }

            int totalNoteCount = chart.note_count;
            score = CalcScore(totalNoteCount, perfectCount, greatCount, badCount, missCount);

            Debug.Log($"[{lane}] 판정 결과: {result}, 현재 점수: {score}");

        }
    }

    public void OnMiss(NoteBehavior note)
    {
        missCount++;
        comboCount = 0;

        Debug.Log($"[{note.Lane}] Miss! 현재 점수: {score}");
        comboText.text = ""; // 콤보 끊김 표시 제거
        judgementImageRenderer.sprite = judgementImages[3];
    }

    public void OnLongNoteSuccess()
    {
        perfectCount++;
        comboCount++;
        comboText.text = comboCount + " Combo!";
    }
}

// Update is called once per frame




