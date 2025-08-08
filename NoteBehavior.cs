using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;

public class NoteBehavior : MonoBehaviour
{
    private bool isLongNote => type == "long";
    private bool isHolding = false;

    private float holdStartTime;
    private float holdEndTime;

    private int spawnTime;
    private int lane;
    private string type;
    private int duration;

    private float scrollSpeed = 10f;
    private float despawnY = -10f;

    private bool initialized = false;
    public int Lane => lane;
    public int SpawnTime => spawnTime;

    private float fullLength; // 원래 길이 기억
    public string NoteType => type;

    private float holdProgress = 0f; // 진행된 길이
    private bool holdCompleted = false;

    public void Init(NoteData note)
    {
        spawnTime = note.time;
        lane = note.lane;
        type = note.type;
        duration = note.type == "long" ? note.duration : 0;

        initialized = true;
    }

    void Update()
    {
        if (!initialized) return;

        float songTime = GameManager.Instance.audioSource.time * 1000f;
        float judgeY = GameManager.Instance.judgementLine.position.y;
        float x = GameManager.Instance.lanes[lane].position.x;

        if (isLongNote)
        {
            // 여기에서 isHolding을 실시간으로 갱신해야 함
            isHolding = Input.GetKey(KeyCode.D) && lane == 0
                     || Input.GetKey(KeyCode.F) && lane == 1
                     || Input.GetKey(KeyCode.J) && lane == 2
                     || Input.GetKey(KeyCode.K) && lane == 3;

            fullLength = duration / 1000f * GameManager.Instance.scrollSpeed;
            float remainingTime = (spawnTime + duration - songTime) / 1000f;
            float currentLength = fullLength;

            if (isHolding && !holdCompleted)
            {
                // 진행도 누적
                holdProgress += Time.deltaTime * GameManager.Instance.scrollSpeed;
                currentLength = fullLength - holdProgress;

                if (currentLength <= 0f)
                {
                    holdCompleted = true;
                    GameManager.Instance.OnLongNoteSuccess();
                    Destroy(gameObject);
                    return;
                }
            }

            currentLength = Mathf.Clamp(currentLength, 0f, fullLength);
            transform.localScale = new Vector3(0.38f, currentLength, 1f);

            float y = (spawnTime - songTime) / 1000f * scrollSpeed + judgeY - currentLength / 2f;
            transform.position = new Vector3(x, y, 0f);
        }
        else
        {
            // 일반 노트
            float timeToJudgeSec = (spawnTime - songTime) / 1000f;
            float y = timeToJudgeSec * GameManager.Instance.scrollSpeed + judgeY;
            transform.position = new Vector3(x, y, 0f);

            if (timeToJudgeSec < -0.2f)
            {
                GameManager.Instance.OnMiss(this);
                Destroy(gameObject);
            }
        }
    }

    public void StartHold()
    {
        isHolding = true;
    }
}