# Daily Project Board → Notion (Exact Status Mapping)

- **Source of truth**: GitHub Projects (v2) 보드의 **Status** 필드 값
- **정확 매핑**: `Todo` / `In Progress` / `Done` (대소문자 무시, 정확 일치)
- **PR 포함 여부**: `INCLUDE_PRS` (기본 false)

## Secrets / Variables
- Secrets: `GH_TOKEN`, (`NOTION_TOKEN`, `NOTION_DB_ID`)
- Env/Vars: `PROJECT_OWNER=jeonglee22`, `PROJECT_NUMBER=7`, (`INCLUDE_PRS=false`)

## Notion DB properties (권장)
`Name`(Title), `Date`(Date), `Repos`(Multi-select), `Done`/`Progressed`/`NotDone`/`NewIssues`(Number)