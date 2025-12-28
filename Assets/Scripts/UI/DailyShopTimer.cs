// using System;
// using System.Globalization;
// using System.Threading;
// using Cysharp.Threading.Tasks;
// using TMPro;
// using UnityEngine;
// using Firebase.Database;

// public class DailyShopTimer : MonoBehaviour
// {
//     [SerializeField] private TextMeshProUGUI remainText;

//     // 예: 한국 자정 고정
//     private TimeZoneInfo resetTimeZone;

//     private DateTimeOffset nextResetUtc;
//     private CancellationTokenSource cts;

//     private const string DayKeyPref = "SHOP_DAYKEY";

//     // ===== RTDB server clock =====
//     private DatabaseReference serverOffsetRef;
//     private long serverOffsetMs = 0;
//     private bool offsetReady = false;

//     private void Awake()
//     {
//         resetTimeZone = GetKoreaTimeZone();

//         // RTDB: /.info/serverTimeOffset
//         // 일부 SDK는 Child(".info")가 막힐 수 있어서 GetReference("...") 단일 경로로 잡는 게 안전한 편
//         serverOffsetRef = FirebaseDatabase.DefaultInstance.GetReference(".info/serverTimeOffset");
//     }

//     private void OnEnable()
//     {
//         // offset 구독 시작
//         serverOffsetRef.ValueChanged += OnServerOffsetChanged;

//         cts = new CancellationTokenSource();
//         RunAsync(cts.Token).Forget();
//     }

//     private void OnDisable()
//     {
//         serverOffsetRef.ValueChanged -= OnServerOffsetChanged;

//         cts?.Cancel();
//         cts?.Dispose();
//         cts = null;
//     }

//     private void OnApplicationPause(bool pause)
//     {
//         if (!pause)
//         {
//             UniTask.Void(async () =>
//             {
//                 await UniTask.Yield();
//                 await SyncAndMaybeRefreshAsync();
//             });
//         }
//     }

//     private void OnServerOffsetChanged(object sender, ValueChangedEventArgs e)
//     {
//         if (e.DatabaseError != null || e.Snapshot == null || e.Snapshot.Value == null)
//             return;

//         // /.info/serverTimeOffset : "서버시간 - 로컬시간" (ms)
//         serverOffsetMs = Convert.ToInt64(e.Snapshot.Value);
//         offsetReady = true;
//     }

//     private async UniTaskVoid RunAsync(CancellationToken token)
//     {
//         await SyncAndMaybeRefreshAsync();

//         int lastShownSec = int.MinValue;

//         while (!token.IsCancellationRequested)
//         {
//             var nowUtc = await GetNowUtcAsync(token);   // ✅ 서버 기준 now
//             var remain = nextResetUtc - nowUtc;

//             if (remain.TotalSeconds <= 0)
//             {
//                 await SyncAndMaybeRefreshAsync();
//                 lastShownSec = int.MinValue;
//                 continue;
//             }

//             int sec = Mathf.Max(0, (int)remain.TotalSeconds);
//             if (sec != lastShownSec)
//             {
//                 lastShownSec = sec;
//                 remainText.text = FormatRemain(remain);
//             }

//             await UniTask.Delay(1000, ignoreTimeScale: true, cancellationToken: token);
//         }
//     }

//     private async UniTask SyncAndMaybeRefreshAsync()
//     {
//         var nowUtc = await GetNowUtcAsync(cts.Token);
//         nextResetUtc = CalcNextResetUtc(nowUtc, resetTimeZone);

//         var todayKey = GetDayKey(nowUtc, resetTimeZone);
//         var savedKey = PlayerPrefs.GetString(DayKeyPref, "");

//         if (savedKey != todayKey)
//         {
//             GenerateAndApplyShopOffers(todayKey);

//             PlayerPrefs.SetString(DayKeyPref, todayKey);
//             PlayerPrefs.Save();
//         }
//     }

//     private static DateTimeOffset CalcNextResetUtc(DateTimeOffset nowUtc, TimeZoneInfo tz)
//     {
//         var localNow = TimeZoneInfo.ConvertTime(nowUtc, tz);

//         var nextLocalDate = localNow.Date.AddDays(1);
//         var nextLocalMidnight = new DateTime(
//             nextLocalDate.Year, nextLocalDate.Month, nextLocalDate.Day,
//             0, 0, 0, DateTimeKind.Unspecified);

//         var offset = tz.GetUtcOffset(nextLocalMidnight);
//         var nextLocal = new DateTimeOffset(nextLocalMidnight, offset);

//         return TimeZoneInfo.ConvertTime(nextLocal, TimeZoneInfo.Utc);
//     }

//     private static string GetDayKey(DateTimeOffset nowUtc, TimeZoneInfo tz)
//     {
//         var localNow = TimeZoneInfo.ConvertTime(nowUtc, tz);
//         return localNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
//     }

//     private static string FormatRemain(TimeSpan t)
//     {
//         if (t.TotalDays >= 1)
//             return $"{(int)t.TotalDays}일 {t.Hours:00}:{t.Minutes:00}:{t.Seconds:00} 남음";
//         return $"{(int)t.TotalHours:00}:{t.Minutes:00}:{t.Seconds:00} 남음";
//     }

//     private static TimeZoneInfo GetKoreaTimeZone()
//     {
//         try { return TimeZoneInfo.FindSystemTimeZoneById("Asia/Seoul"); }
//         catch { return TimeZoneInfo.FindSystemTimeZoneById("Korea Standard Time"); }
//     }

//     /// <summary>
//     /// ✅ RTDB 서버시간(추정): utcNow + serverTimeOffset
//     /// </summary>
//     private async UniTask<DateTimeOffset> GetNowUtcAsync(CancellationToken token)
//     {
//         // offset이 아직 안 들어온 초기엔 잠깐 기다렸다가(최대 2초) fallback
//         if (!offsetReady)
//         {
//             try
//             {
//                 await UniTask.WaitUntil(() => offsetReady, timeout: TimeSpan.FromSeconds(2), cancellationToken: token);
//             }
//             catch
//             {
//                 // timeout or cancel -> fallback
//             }
//         }

//         var localUtc = DateTimeOffset.UtcNow;
//         if (offsetReady)
//             return localUtc.AddMilliseconds(serverOffsetMs);

//         // 최후 fallback: 기기 시간(초기 1~2초 구간)
//         return localUtc;
//     }

//     private void GenerateAndApplyShopOffers(string dayKey)
//     {
//         Debug.Log($"[DailyShop] Refresh offers. dayKey={dayKey}");
//     }
// }
