import { graphql } from "@octokit/graphql";
import { Octokit } from "octokit";
import { Client as Notion } from "@notionhq/client";
import { DateTime } from "luxon";

const GH_TOKEN = process.env.GH_TOKEN;
if (!GH_TOKEN) throw new Error("GH_TOKEN is required");
const gql = graphql.defaults({ headers: { authorization: `token ${GH_TOKEN}` } });
const octo = new Octokit({ auth: GH_TOKEN });

const NOTION_TOKEN = process.env.NOTION_TOKEN;
const NOTION_DB_ID = process.env.NOTION_DB_ID;
const notion = NOTION_TOKEN ? new Notion({ auth: NOTION_TOKEN }) : null;

const ZONE = process.env.TIMEZONE || "Asia/Seoul";
const OWNER = process.env.PROJECT_OWNER;   // e.g. "jeonglee22"
const NUMBER = Number(process.env.PROJECT_NUMBER); // e.g. 7
const INCLUDE_PRS = String(process.env.INCLUDE_PRS || "false").toLowerCase() === "true";

if (!OWNER || !NUMBER) throw new Error("PROJECT_OWNER and PROJECT_NUMBER are required");

// === 정확 매핑: 'Todo' / 'In Progress' / 'Done' ===
const STATUS_TARGETS = {
  todo: new Set(["todo"]),
  inProgress: new Set(["in progress"]),
  done: new Set(["done"]) 
};

async function getProjectStatusMeta(owner, number) {
  const q = `
    query ($owner: String!, $number: Int!) {
      user(login: $owner) {
        projectV2(number: $number) {
          id
          fields(first: 50) {
            nodes {
              ... on ProjectV2SingleSelectField {
                id
                name
                options { id name }
              }
            }
          }
        }
      }
      organization(login: $owner) {
        projectV2(number: $number) {
          id
          fields(first: 50) {
            nodes {
              ... on ProjectV2SingleSelectField {
                id
                name
                options { id name }
              }
            }
          }
        }
      }
    }`;
  const data = await gql(q, { owner, number });
  const proj = data.user?.projectV2 ?? data.organization?.projectV2;
  if (!proj) throw new Error("Project not found (check owner/number)");
  const statusField = proj.fields.nodes.find(f => f && f.name === "Status");
  if (!statusField) throw new Error("Status field not found in project");

  // 옵션 이름 로깅(디버그)
  console.log("Status options:", statusField.options.map(o => o.name));

  return { projectId: proj.id, statusFieldId: statusField.id };
}

async function getItemsByStatus(projectId, statusFieldId) {
  const q = `
    query ($projectId: ID!, $after: String) {
      node(id: $projectId) {
        ... on ProjectV2 {
          items(first: 100, after: $after) {
            pageInfo { hasNextPage endCursor }
            nodes {
              id
              content {
                __typename
                ... on Issue { repository { nameWithOwner } number title url }
                ... on PullRequest { repository { nameWithOwner } number title url }
              }
              fieldValues(first: 20) {
                nodes {
                  __typename
                  ... on ProjectV2ItemFieldSingleSelectValue {
                    field { __typename ... on ProjectV2SingleSelectField { id name } }
                    name
                  }
                }
              }
            }
          }
        }
      }
    }`;

  const buckets = { todo: [], inProgress: [], done: [], unknown: [] };
  let after = null;
  do {
    const d = await gql(q, { projectId, after });
    const items = d.node.items.nodes;
    for (const it of items) {
      const fv = it.fieldValues.nodes.find(v => v?.field?.name === "Status");
      const statusName = (fv?.name || "").trim().toLowerCase();

      const c = it.content; // Issue or PullRequest or null
      if (!c) continue; // skip items without content
      if (c.__typename === "PullRequest" && !INCLUDE_PRS) continue; // PR 제외 옵션

      const row = {
        repo: c.repository.nameWithOwner,
        number: c.number,
        title: c.title,
        url: c.url
      };

      if (STATUS_TARGETS.done.has(statusName)) buckets.done.push(row);
      else if (STATUS_TARGETS.inProgress.has(statusName)) buckets.inProgress.push(row);
      else if (STATUS_TARGETS.todo.has(statusName)) buckets.todo.push(row);
      else buckets.unknown.push({ status: fv?.name || "(Unknown)", ...row });
    }
    after = d.node.items.pageInfo.hasNextPage ? d.node.items.pageInfo.endCursor : null;
  } while (after);
  return buckets;
}

// === MAIN ===
(async () => {
  const { projectId, statusFieldId } = await getProjectStatusMeta(OWNER, NUMBER);
  const buckets = await getItemsByStatus(projectId, statusFieldId);

  const now = DateTime.now().setZone(ZONE);
  const dateStr = now.minus({ days: 1 }).toISODate(); // 보고 제목용(전일 기준)

  const result = {
    window: { zone: ZONE, dateForTitle: dateStr },
    repos: { [`project:${OWNER}/${NUMBER}`]: {
      done: buckets.done,
      inProgress: buckets.inProgress,
      todo: buckets.todo,
      counts: { newIssues: 0 }
    }},
    totals: {
      done: buckets.done.length,
      inProgress: buckets.inProgress.length,
      todo: buckets.todo.length,
      newIssues: 0
    }
  };

  console.log("SUMMARY (Project-based)");
  console.log(JSON.stringify(result, null, 2));

  if (notion && NOTION_DB_ID) {
    const title = `Daily Report - ${dateStr}`;

    // 멱등성: 같은 Date의 페이지가 있으면 업데이트
    const existing = await notion.databases.query({
      database_id: NOTION_DB_ID,
      filter: { property: "Date", date: { equals: dateStr } },
      page_size: 1
    });

    let pageId = existing.results[0]?.id;
    if (!pageId) {
      const props = {
        Name: { title: [{ text: { content: title } }] },
        Date: { date: { start: dateStr } },
        Repos: { multi_select: [{ name: `project:${OWNER}/${NUMBER}` }] },
        Done: { number: result.totals.done },
        Progressed: { number: result.totals.inProgress },
        NotDone: { number: result.totals.todo },
        NewIssues: { number: result.totals.newIssues }
      };
      const page = await notion.pages.create({ parent: { database_id: NOTION_DB_ID }, properties: props });
      pageId = page.id;
    } else {
      await notion.pages.update({
        page_id: pageId,
        properties: {
          Done: { number: result.totals.done },
          Progressed: { number: result.totals.inProgress },
          NotDone: { number: result.totals.todo },
          NewIssues: { number: result.totals.newIssues }
        }
      });
    }

    const h2 = (t) => ({ heading_2: { rich_text: [{ type: "text", text: { content: t } }] } });
    const bullet = (t, url) => ({ paragraph: { rich_text: [{ type: "text", text: { content: t, link: url ? { url } : null } }] } });

    const children = [];
    children.push(h2("✅ Done"));
    for (const i of buckets.done) children.push(bullet(`[${i.repo} #${i.number}] ${i.title}`, i.url));

    children.push(h2("🚧 In Progress"));
    for (const i of buckets.inProgress) children.push(bullet(`[${i.repo} #${i.number}] ${i.title}`, i.url));

    children.push(h2("⏳ Todo"));
    for (const i of buckets.todo) children.push(bullet(`[${i.repo} #${i.number}] ${i.title}`, i.url));

    // 100 children per request 제한 → 90개씩 쪼개서 업로드
    for (let i = 0; i < children.length; i += 90) {
      await notion.blocks.children.append({ block_id: pageId, children: children.slice(i, i + 90) });
    }
    console.log("Notion page updated:", pageId);
  }
})();