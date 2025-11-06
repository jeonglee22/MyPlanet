# Daily Project Roadmap → Notion

- **Done**: 전날(00:00~24:00 KST) 사이에 `Status`가 **Done**으로 바뀐 항목만
- **In Progress**: 오늘 스냅샷에서 `Status = In Progress`
- **Todo**: 오늘 스냅샷에서 `Status = Todo`

## 환경변수
- `PROJECT_OWNER` / `PROJECT_NUMBER` : 보드 식별자 (예: `jeonglee22` / `7`)
- `TIMEZONE` : 기본 `Asia/Seoul`
- `REPORT_OFFSET_DAYS` : 0=오늘 기준, 1=어제 기준(테스트/과거 재생성에 사용)
- (선택) `NOTION_TOKEN`, `NOTION_DB_ID`

## 주의
- `Status` 단일선택 필드가 **반드시 존재**해야 하며, 옵션 이름은 정확히 `Todo / In Progress / Done`이어야 합니다.
- 일부 아이템의 `statusUpdatedAt`가 null일 수 있습니다. 이 경우 Done 전환 감지는 되지 않으므로 현 상태만 반영됩니다.
