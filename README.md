# Daily Issue Classifier (GitHub → Notion)


## Quick start
1. Add this repo skeleton to your project.
2. Set repository **Secrets** / **Variables**:
- Secrets: `GH_TOKEN`, (`NOTION_TOKEN`, `NOTION_DB_ID` optional)
- Variables: `TARGET_REPOS` = `org/repo-a,org/repo-b`
3. (Optional) Adjust envs in workflow: `PLAN_TODAY_LABEL`, `EXCLUDE_ACTORS`, `TIMEZONE`.
4. Push to default branch. A run occurs daily at 09:00 KST (00:00 UTC) and also supports manual dispatch.


## Classification rules
- **Done**: issues closed within yesterday's KST window
- **In Progress**: issues with any comments or non-closed events in that window
- **Todo**: open issues labeled with `plan:today` that saw no changes yesterday


> PRs are excluded by default; remove the checks if you want to include them.


## Notes
- Uses GitHub REST pagination generously; for very large orgs, consider narrowing repos or adding heuristics.
- Notion export is optional; when enabled, the script creates/updates a page for the date, then appends section blocks in chunks to respect API limits.