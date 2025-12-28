using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class DailyShopResetTimer : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI remainText;

    [Header("Mode")]
    [Tooltip("테스트: 로컬 시간 기준으로 동작 (시간 조작 가능)")]
    [SerializeField] private bool useLocalTimeForTest = false;

    [Header("Reset Timezone")]
    [Tooltip("자정 기준 타임존. 한국이면 Asia/Seoul 권장")]
    [SerializeField] private string resetTimeZoneId = "Asia/Seoul";

    [Header("Optional")]
    [Tooltip("초기화 이벤트 중복 방지용 dayKey 저장")]
    [SerializeField] private string dayKeyPref = "DAILY_SHOP_DAYKEY";

    public event Action OnDailyReset; // 여기에 상점 아이템 갱신/재롤 등 연결

    private FirebaseServerClock serverClock;
    private TimeZoneInfo resetTz;

    private DateTimeOffset nextResetUtc;
    private string lastDayKey;

    private CancellationTokenSource cts;

    private void Awake()
    {
        resetTz = SafeGetTimeZone(resetTimeZoneId);

        if (!useLocalTimeForTest)
        {
            serverClock = new FirebaseServerClock();
            serverClock.Start();
        }
    }

    private void OnEnable()
    {
        lastDayKey = PlayerPrefs.GetString(dayKeyPref, "");
        cts = new CancellationTokenSource();
        RunAsync(cts.Token).Forget();
    }

    private void OnDisable()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;

        serverClock?.Dispose();
        serverClock = null;
    }

    private void OnApplicationPause(bool pause)
    {
        if (!pause)
        {
            UniTask.Void(async () =>
            {
                await UniTask.Yield();
                RecalcNextResetUtc();
                CheckImmediateResetIfDayChanged();
            });
        }
    }

    private async UniTaskVoid RunAsync(CancellationToken token)
    {
        // 초기 계산
        RecalcNextResetUtc();
        CheckImmediateResetIfDayChanged();

        while (!token.IsCancellationRequested)
        {
            // 서버 시계 준비 안 됐으면 표시만 하고 계속 대기
            if (!useLocalTimeForTest && (serverClock == null || !serverClock.IsReady))
            {
                if (remainText) remainText.text = "--:--:-- 남음";
                await UniTask.Delay(200, DelayType.UnscaledDeltaTime, PlayerLoopTiming.Update, token);
                continue;
            }

            UpdateRemainText();

            // 자정 도달 체크
            if (GetUtcNow() >= nextResetUtc)
            {
                DoReset();
                RecalcNextResetUtc();
            }

            await UniTask.Delay(200, DelayType.UnscaledDeltaTime, PlayerLoopTiming.Update, token);
        }
    }

    private void UpdateRemainText()
    {
        var remain = nextResetUtc - GetUtcNow();
        if (remain < TimeSpan.Zero) remain = TimeSpan.Zero;

        int hh = (int)remain.TotalHours; // 0~23
        int mm = remain.Minutes;
        int ss = remain.Seconds;

        if (remainText)
            remainText.text = $"{hh:00}:{mm:00}:{ss:00} 남음";
    }

    private void DoReset()
    {
        // dayKey 갱신 + 저장
        var key = GetDayKey(GetUtcNow(), resetTz);
        if (lastDayKey == key) return;

        lastDayKey = key;
        PlayerPrefs.SetString(dayKeyPref, lastDayKey);
        PlayerPrefs.Save();

        OnDailyReset?.Invoke();
    }

    private void CheckImmediateResetIfDayChanged()
    {
        var nowKey = GetDayKey(GetUtcNow(), resetTz);

        // 앱을 오래 꺼뒀다 켜는 등 "이미 날짜가 바뀐 상태"면 즉시 갱신
        if (!string.IsNullOrEmpty(lastDayKey) && lastDayKey != nowKey)
        {
            DoReset();
        }
    }

    private void RecalcNextResetUtc()
    {
        var nowUtc = GetUtcNow();
        nextResetUtc = GetNextMidnightUtc(nowUtc, resetTz);
    }

    private DateTimeOffset GetUtcNow()
    {
        if (useLocalTimeForTest) return DateTimeOffset.UtcNow;
        return serverClock.UtcNow;
    }

    private static string GetDayKey(DateTimeOffset utcNow, TimeZoneInfo tz)
    {
        var local = TimeZoneInfo.ConvertTime(utcNow, tz);
        return local.ToString("yyyyMMdd");
    }

    private static DateTimeOffset GetNextMidnightUtc(DateTimeOffset utcNow, TimeZoneInfo tz)
    {
        // tz 로컬 날짜 기준 "다음날 00:00:00"을 UTC로 변환
        var localNow = TimeZoneInfo.ConvertTime(utcNow, tz);

        var nextLocalDate = localNow.Date.AddDays(1); // 다음날 00:00
        var nextLocalUnspec = DateTime.SpecifyKind(nextLocalDate, DateTimeKind.Unspecified);

        var nextUtc = TimeZoneInfo.ConvertTimeToUtc(nextLocalUnspec, tz);
        return new DateTimeOffset(nextUtc, TimeSpan.Zero);
    }

    private static TimeZoneInfo SafeGetTimeZone(string id)
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
        catch
        {
            // 일부 환경(Windows)에서 한국 타임존 이름이 다를 수 있어서 fallback
            try { return TimeZoneInfo.FindSystemTimeZoneById("Korea Standard Time"); }
            catch { return TimeZoneInfo.Local; }
        }
    }
}
