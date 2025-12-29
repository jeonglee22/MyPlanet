using System;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

/// <summary>
/// Realtime Database의 /.info/serverTimeOffset(ms)로 서버 기준 UTC 시간을 추정한다.
/// serverUtcNow = DateTimeOffset.UtcNow + offset
/// </summary>
public sealed class FirebaseServerClock : IDisposable
{
    private const string PrefKey = "FB_SERVER_TIME_OFFSET_MS";

    private readonly DatabaseReference offsetRef;
    private bool subscribed;

    public bool IsReady { get; private set; }
    public long OffsetMs { get; private set; }

    public FirebaseServerClock()
    {
        // "/.info/serverTimeOffset"
        offsetRef = FirebaseDatabase.DefaultInstance.RootReference
            .Child(".info")
            .Child("serverTimeOffset");
    }

    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow + TimeSpan.FromMilliseconds(OffsetMs);

    /// <summary>
    /// 오프셋 초기 로드 + 실시간 구독 시작
    /// </summary>
    public void Start()
    {
        if (subscribed) return;
        subscribed = true;

        // 마지막 성공값이 있으면 즉시 적용(오프라인 대비)
        OffsetMs = PlayerPrefs.GetInt(PrefKey, 0);
        if (PlayerPrefs.HasKey(PrefKey)) IsReady = true;

        // 한번 읽기
        offsetRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled || task.Result == null) return;
            ApplySnapshot(task.Result);
        });

        // 값 변경 구독
        offsetRef.ValueChanged += OnOffsetChanged;
    }

    private void OnOffsetChanged(object sender, ValueChangedEventArgs e)
    {
        if (e.DatabaseError != null || e.Snapshot == null) return;
        ApplySnapshot(e.Snapshot);
    }

    private void ApplySnapshot(DataSnapshot snap)
    {
        // Realtime DB는 숫자가 long/double 등으로 들어올 수 있음
        try
        {
            var v = snap.Value;
            long ms;

            if (v is long l) ms = l;
            else if (v is int i) ms = i;
            else if (v is double d) ms = (long)d;
            else if (long.TryParse(v?.ToString(), out var parsed)) ms = parsed;
            else return;

            OffsetMs = ms;
            IsReady = true;

            PlayerPrefs.SetInt(PrefKey, (int)Mathf.Clamp(ms, int.MinValue, int.MaxValue));
            PlayerPrefs.Save();
        }
        catch
        {
            // ignore
        }
    }

    public void Dispose()
    {
        if (!subscribed) return;
        subscribed = false;
        offsetRef.ValueChanged -= OnOffsetChanged;
    }
}
