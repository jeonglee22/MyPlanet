import { graphql } from "@octokit/graphql";
import { Client as Notion } from "@notionhq/client";
import { DateTime } from "luxon";

const GH_TOKEN = process.env.GH_TOKEN;
if (!GH_TOKEN) throw new Error("GH_TOKEN is required");
const gql = graphql.defaults({ headers: { authorization: `token ${GH_TOKEN}` } });

const NOTION_TOKEN = process.env.NOTION_TOKEN;
const NOTION_DB_ID = process.env.NOTION_DB_ID;
const notion = NOTION_TOKEN ? new Notion({ auth: NOTION_TOKEN }) : null;

const ZONE = process.env.TIMEZONE || "Asia/Seoul";
const OWNER = process.env.PROJECT_OWNER;   // e.g. "jeonglee22"
const NUMBER = Number(process.env.PROJECT_NUMBER); // e.g. 7
const OFFSET = Number(process.env.REPORT_OFFSET_DAYS ?? 0); // 기준일 오프셋(0=오늘)

if (!OWNER || !NUMBER) throw new Error("PROJECT_OWNER and PROJECT_NUMBER are required");

// === 상태 이름 정확 매핑 ===
const STATUS_TODO = "todo";
const STATUS_INPROG = "in progress";
const STATUS_DONE = "done";

// === 시간창 계산 ===
// 기준일(오늘 - OFFSET)의 00:00~24:00 KST 스냅샷을 '오늘'로 간주
const todayStartKST = DateTime.now().setZone(ZONE).startOf('day').minus({ days: OFFSET });
const todayEndKST   = todayStartKST.plus({ days: 1 });
// 전날 창: 어제 00:00~오늘 00:00 (Done 전환 감지용)
const yStartKST = todayStartKST.minus({ days: 1 });
const yEndKST   = todayStartKST;
const yStartUTC = yStartKST.toUTC();
const yEndUTC   = yEndKST.toUTC();

function withinYesterdayUTC(tsISO) {
  const t = DateTime.fromISO(tsISO, { zone: 'utc' });
  return t >= yStartUTC && t < yEndUTC;
}

// === GraphQL helpers ===
async function getProjectStatusField(owner, number) {
  const q = `
    query ($owner: String!, $number: Int!) {
      repositoryOwner(login: $owner) {
        __typename
        ... on User {
          projectV2(number: $number) {
            id
            fields(first: 50) {
              nodes { ... on ProjectV2SingleSelectField { id name options { id name } } }
            }
          }
        }
        ... on Organization {
          projectV2(number: $number) {
            id
            fields(first: 50) {
              nodes { ... on ProjectV2SingleSelectField { id name options { id name } } }
            }
          }
        }
      }
    }`;
  const data = await gql(q, { owner, number });
  const proj = data.repositoryOwner?.projectV2;
  if (!proj) throw new Error("Project not found for owner/number");
  const statusField = proj.fields.nodes.find(f => f && f.name === "Status");
  if (!statusField) throw new Error("Status field not found (must be a single-select named 'Status')");
  console.log("Status options:", statusField.options.map(o => o.name));
  return { projectId: proj.id, statusFieldId: statusField.id };
}

async function getAllProjectItemsWithStatus(projectId) {
  // items + fieldValues(Status) + fieldValueByName(name:"Status").updatedAt
  const q = `
    query ($projectId: ID!, $after: String) {
      node(id: $projectId) {
        ... on ProjectV2 {
          items(first: 100, after: $after) {
            pageInfo { hasNextPage endCursor }
            nodes {
              id
              content { __typename ... on Issue { repository { nameWithOwner } number title url }
                                 ... on PullRequest { repository { nameWithOwner } number title url } }
              fieldValues(first: 20) {
                nodes { __typename ... on ProjectV2ItemFieldSingleSelectValue { field { ... on ProjectV2SingleSelectField { id name } } name } }
              }
              statusNow: fieldValueByName(name: "Status") { ... on ProjectV2ItemFieldSingleSelectValue { name updatedAt } }
            }
          }
        }
      }
    }`;

  const items = [];
  let after = null;
  do {
    const d = await gql(q, { projectId, after });
    const page = d.node.items;
    for (const it of page.nodes) {
      const c = it.content;
      if (!c) continue; // skip items without attached Issue/PR
      const statusFV   = it.fieldValues.nodes.find(v => v?.field?.name === 'Status');
      const statusName = (statusFV?.name || '').trim();
      const statusNow  = it.statusNow ? { name: it.statusNow.name || '', updatedAt: it.statusNow.updatedAt || null } : { name: statusName, updatedAt: null };
      items.push({
        id: it.id,
        type: c.__typename,
        repo: c.repository.nameWithOwner,
        number: c.number,
        title: c.title,
        url: c.url,
        statusName,
        statusUpdatedAt: statusNow.updatedAt // ISO or null
      });
    }
    after = page.pageInfo.hasNextPage ? page.pageInfo.endCursor : null;
  } while (after);
  return items;
}

function bucketize(items) {
  const done = [];
  const inProgress = [];
  const todo = [];

  for (const it of items) {
    const s = (it.statusName || '').trim().toLowerCase();
    // 1) 전날 Done으로 바뀐 항목 → Done
    if (s === STATUS_DONE) {
      if (it.statusUpdatedAt && withinYesterdayUTC(it.statusUpdatedAt)) {
        done.push(it);
        continue;
      }
      // Done이지만 전날 전환이 아니면 오늘 완료 목록에는 포함하지 않음
    }
    // 2) 오늘 스냅샷에서 In Progress → 진행 사항
    if (s === STATUS_INPROG) {
      inProgress.push(it);
      continue;
    }
    // 3) 오늘 스냅샷에서 Todo → 오늘 할 일
    if (s === STATUS_TODO) {
      todo.push(it);
      continue;
    }
  }
  return { done, inProgress, todo };
}

(async () => {
  const { projectId } = await getProjectStatusField(OWNER, NUMBER);
  const items = await getAllProjectItemsWithStatus(projectId);
  const buckets = bucketize(items);

  const reportDate = todayStartKST.toISODate();
  const result = {
    reportDate,
    tz: ZONE,
    prevWindowKST: { start: yStartKST.toISO(), end: yEndKST.toISO() },
    counts: { done: buckets.done.length, inProgress: buckets.inProgress.length, todo: buckets.todo.length },
    items: buckets
  };

  console.log("SUMMARY (Roadmap-based)");
  console.log(JSON.stringify(result, null, 2));

  if (notion && NOTION_DB_ID) {
    const title = `Daily Report - ${reportDate}`;

    const existing = await notion.databases.query({
      database_id: NOTION_DB_ID,
      filter: { property: "Date", date: { equals: reportDate } },
      page_size: 1
    });

    let pageId = existing.results[0]?.id;
    if (!pageId) {
      const props = {
        Name: { title: [{ text: { content: title } }] },
        Date: { date: { start: reportDate } },
        Repos: { multi_select: [{ name: `project:${OWNER}/${NUMBER}` }] },
        Done: { number: buckets.done.length },
        Progressed: { number: buckets.inProgress.length },
        NotDone: { number: buckets.todo.length },
        NewIssues: { number: 0 }
      };
      const page = await notion.pages.create({ parent: { database_id: NOTION_DB_ID }, properties: props });
      pageId = page.id;
    } else {
      await notion.pages.update({
        page_id: pageId,
        properties: {
          Done: { number: buckets.done.length },
          Progressed: { number: buckets.inProgress.length },
          NotDone: { number: buckets.todo.length }
        }
      });
    }

    const h2 = (t) => ({ heading_2: { rich_text: [{ type: "text", text: { content: t } }] } });
    const bullet = (t, url) => ({ paragraph: { rich_text: [{ type: "text", text: { content: t, link: url ? { url } : null } }] } });

    const children = [];
    children.push(h2("✅ Done (changed to Done yesterday)"));
    for (const i of buckets.done) children.push(bullet(`[${i.repo} #${i.number}] ${i.title}`, i.url));

    children.push(h2("🚧 In Progress (today snapshot)"));
    for (const i of buckets.inProgress) children.push(bullet(`[${i.repo} #${i.number}] ${i.title}`, i.url));

    children.push(h2("⏳ Todo (today snapshot)"));
    for (const i of buckets.todo) children.push(bullet(`[${i.repo} #${i.number}] ${i.title}`, i.url));

    for (let i = 0; i < children.length; i += 90) {
      await notion.blocks.children.append({ block_id: pageId, children: children.slice(i, i + 90) });
    }
    console.log("Notion page updated:", pageId);
  }
})();