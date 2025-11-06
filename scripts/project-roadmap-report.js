// scripts/project-roadmap-report.js (fixed)
// Roadmap timeline aware daily report
// - Done: changed to Done yesterday (KST)
// - In Progress: Status = In Progress today AND timeline intersects today
// - Todo: Status = Todo today AND timeline intersects today

import { graphql } from "@octokit/graphql";
import { Client as Notion } from "@notionhq/client";
import { DateTime } from "luxon";

// ===== Env & clients =====
const GH_TOKEN = process.env.GH_TOKEN;
if (!GH_TOKEN) throw new Error("GH_TOKEN is required");
const gql = graphql.defaults({ headers: { authorization: `token ${GH_TOKEN}` } });

const NOTION_TOKEN = process.env.NOTION_TOKEN;
const NOTION_DB_ID = process.env.NOTION_DB_ID;
const notion = NOTION_TOKEN ? new Notion({ auth: NOTION_TOKEN }) : null;

const ZONE = process.env.TIMEZONE || "Asia/Seoul";
const OWNER = process.env.PROJECT_OWNER;   // e.g. "jeonglee22"
const NUMBER = Number(process.env.PROJECT_NUMBER); // e.g. 7
const OFFSET = Number(process.env.REPORT_OFFSET_DAYS ?? 0); // 0=today, 1=yesterday, ...

// explicit field names (recommended)
const START_FIELD_NAME = process.env.DATE_START_FIELD_NAME || null;   // e.g. "Start date"
const END_FIELD_NAME   = process.env.DATE_END_FIELD_NAME   || null;   // e.g. "Target date"

if (!OWNER || !NUMBER) throw new Error("PROJECT_OWNER and PROJECT_NUMBER are required");

// ===== Time window (KST) =====
const todayStartKST = DateTime.now().setZone(ZONE).startOf("day").minus({ days: OFFSET });
const todayEndKST   = todayStartKST.plus({ days: 1 });

const yStartKST = todayStartKST.minus({ days: 1 });
const yEndKST   = todayStartKST;
const yStartUTC = yStartKST.toUTC();
const yEndUTC   = yEndKST.toUTC();

function withinYesterdayUTC(tsISO) {
  if (!tsISO) return false;
  const t = DateTime.fromISO(tsISO, { zone: "utc" });
  return t >= yStartUTC && t < yEndUTC;
}

// ===== Status constants =====
const STATUS_TODO = "todo";
const STATUS_INPROG = "in progress";
const STATUS_DONE = "done";

// ===== GraphQL helpers =====
// Discover fields; prefer explicit env names if provided
async function getProjectMeta(owner, number) {
  const q = `
    query ($owner: String!, $number: Int!) {
      repositoryOwner(login: $owner) {
        __typename
        ... on User {
          projectV2(number: $number) {
            id
            fields(first: 100) {
              nodes {
                __typename
                ... on ProjectV2SingleSelectField { id name }
                ... on ProjectV2IterationField   { id name }
                ... on ProjectV2Field            { id name }
              }
            }
          }
        }
        ... on Organization {
          projectV2(number: $number) {
            id
            fields(first: 100) {
              nodes {
                __typename
                ... on ProjectV2SingleSelectField { id name }
                ... on ProjectV2IterationField   { id name }
                ... on ProjectV2Field            { id name }
              }
            }
          }
        }
      }
    }`;

  const data = await gql(q, { owner, number });
  const proj = data.repositoryOwner?.projectV2;
  if (!proj) throw new Error("Project not found for owner/number");

  const fields = (proj.fields.nodes || [])
    .filter(Boolean)
    .map(f => ({ name: (f?.name || "").trim(), typename: f?.__typename, id: f?.id }));

  // Status = 단일선택 필드
  const statusField = fields.find(
    f => f.typename === "ProjectV2SingleSelectField" && f.name.toLowerCase() === "status"
  );
  if (!statusField) throw new Error("Status single-select field named 'Status' not found");

  // 시작/마감 필드명: ENV 우선, 없으면 후보로 추론
  const lname = s => (s || "").toLowerCase();
  let startName = process.env.DATE_START_FIELD_NAME || null;   // "Start date"
  let endName   = process.env.DATE_END_FIELD_NAME   || null;   // "Target date"

  if (!startName) {
    const cand = ["start date", "start", "시작일"];
    startName = fields.find(f => cand.includes(lname(f.name)))?.name || null;
  }
  if (!endName) {
    const cand = ["target date", "due date", "end date", "target", "due", "end", "마감일"];
    endName = fields.find(f => cand.includes(lname(f.name)))?.name || null;
  }

  console.log("Detected fields:", {
    status: statusField?.name,
    start: startName || "(none)",
    end: endName || "(none)",
  });

  return {
    projectId: proj.id,
    statusFieldName: statusField.name,
    startFieldName: startName,
    endFieldName: endName,
  };
}

// Fetch all items with Status and optional Start/End dates
async function getAllProjectItemsWithStatus(projectId, statusFieldName, startFieldName, endFieldName) {
  // Note: $startName/$endName can be null; guard in query with @include directive
  const q = `
    query ($projectId: ID!, $after: String, $statusName: String!, $startName: String!, $endName: String!) {
      node(id: $projectId) {
        ... on ProjectV2 {
          items(first: 100, after: $after) {
            pageInfo { hasNextPage endCursor }
            nodes {
              id
              content { __typename ... on Issue { repository { nameWithOwner } number title url }
                                 ... on PullRequest { repository { nameWithOwner } number title url } }
              statusNow: fieldValueByName(name: $statusName) { ... on ProjectV2ItemFieldSingleSelectValue { name updatedAt } }
              startVal: fieldValueByName(name: $startName) { ... on ProjectV2ItemFieldDateValue { date } }
              endVal:   fieldValueByName(name: $endName)   { ... on ProjectV2ItemFieldDateValue { date } }
            }
          }
        }
      }
    }`;

  const items = [];
  let after = null;
  const vars = {
    projectId,
    statusName: statusFieldName,
    withStart: Boolean(startFieldName),
    startName: startFieldName,
    withEnd: Boolean(endFieldName),
    endName: endFieldName,
    after
  };

  do {
    vars.after = after;
    const d = await gql(q, vars);
    const page = d.node.items;
    for (const it of page.nodes) {
      const c = it.content;
      if (!c) continue; // skip items without attached Issue/PR
      const statusName = (it.statusNow?.name || "").trim();
      const statusUpdatedAt = it.statusNow?.updatedAt || null;
      const startDate = it.startVal?.date || null;
      const endDate   = it.endVal?.date || null;

      items.push({
        id: it.id,
        type: c.__typename,
        repo: c.repository.nameWithOwner,
        number: c.number,
        title: c.title,
        url: c.url,
        statusName,
        statusUpdatedAt,
        startDate,
        endDate
      });
    }
    after = page.pageInfo.hasNextPage ? page.pageInfo.endCursor : null;
  } while (after);
  return items;
}

// Decide buckets with timeline filtering
function bucketize(items) {
  const done = [];
  const inProgress = [];
  const todo = [];

  for (const it of items) {
    const s = (it.statusName || "").trim().toLowerCase();

    // Timeline intersect check: if either date is missing, treat as open (no exclusion)
    if (it.startDate) {
      const sd = DateTime.fromISO(it.startDate, { zone: ZONE }).startOf("day");
      if (sd > todayEndKST) continue; // future start → exclude today
    }
    if (it.endDate) {
      const ed = DateTime.fromISO(it.endDate, { zone: ZONE }).endOf("day");
      if (ed < todayStartKST) continue; // already ended → exclude today
    }

    // 1) Done if changed to Done yesterday
    if (s === STATUS_DONE) {
      if (withinYesterdayUTC(it.statusUpdatedAt)) done.push(it);
      continue; // do not include Done elsewhere
    }

    // 2) Today snapshot buckets
    if (s === STATUS_INPROG) { inProgress.push(it); continue; }
    if (s === STATUS_TODO)   { todo.push(it); continue; }
  }
  return { done, inProgress, todo };
}

// ===== MAIN =====
(async () => {
  const { projectId, statusFieldName, startFieldName, endFieldName } = await getProjectMeta(OWNER, NUMBER);
  const items = await getAllProjectItemsWithStatus(projectId, statusFieldName, startFieldName, endFieldName);
  const buckets = bucketize(items);

  const reportDate = todayStartKST.toISODate();
  const result = {
    reportDate,
    tz: ZONE,
    windows: {
      yesterdayKST: { start: yStartKST.toISO(), end: yEndKST.toISO() },
      todayKST: { start: todayStartKST.toISO(), end: todayEndKST.toISO() }
    },
    counts: { done: buckets.done.length, inProgress: buckets.inProgress.length, todo: buckets.todo.length },
    items: buckets
  };

  console.log("SUMMARY (Roadmap-based)");
  console.log(JSON.stringify(result, null, 2));

  // ===== Notion export (optional) =====
  if (notion && NOTION_DB_ID) {
    const title = `Daily Report - ${reportDate}`;

    // Upsert by Date
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

    // append in chunks (Notion limit 100 blocks)
    for (let i = 0; i < children.length; i += 90) {
      await notion.blocks.children.append({ block_id: pageId, children: children.slice(i, i + 90) });
    }

    console.log("Notion page updated:", pageId);
  }
})();
