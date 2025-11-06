import { Octokit } from 'octokit';
import { Client as Notion } from '@notionhq/client';
import { DateTime } from 'luxon';


const GH_TOKEN = process.env.GH_TOKEN;
if (!GH_TOKEN) throw new Error('GH_TOKEN is required');
const octo = new Octokit({ auth: GH_TOKEN });


const NOTION_TOKEN = process.env.NOTION_TOKEN;
const NOTION_DB_ID = process.env.NOTION_DB_ID;
const notion = NOTION_TOKEN ? new Notion({ auth: NOTION_TOKEN }) : null;


const ZONE = process.env.TIMEZONE || 'Asia/Seoul';
const REPOS = (process.env.TARGET_REPOS || '').split(',').map(s => s.trim()).filter(Boolean);
if (REPOS.length === 0) throw new Error('TARGET_REPOS must be set (e.g., org/repo-a,org/repo-b)');
const PLAN_LABEL = process.env.PLAN_TODAY_LABEL || 'plan:today';
const EXCLUDE_ACTORS = new Set((process.env.EXCLUDE_ACTORS || '').split(',').map(s=>s.trim()).filter(Boolean));


// Window: yesterday 00:00..24:00 KST → ISO UTC
const offset = Number(process.env.REPORT_OFFSET_DAYS ?? 1); // 기본 어제
const endKST = DateTime.now().setZone(ZONE).startOf('day').plus({ days: -offset + 1 });
// offset=1 -> end=오늘 00:00, offset=0 -> end=내일 00:00(=오늘 포함)
const startKST = endKST.minus({ days: 1 });

const startUTC = startKST.toUTC();
const endUTC   = endKST.toUTC();
function withinWindow(ts) {
  const t = DateTime.fromISO(ts, { zone: 'utc' });
  return t >= startUTC && t < endUTC;
}

// const endKST = DateTime.now().setZone(ZONE).startOf('day');
// const startKST = endKST.minus({ days: 1 });
// const startUTC = startKST.toUTC();
// const endUTC = endKST.toUTC();


// function withinWindow(ts) {
// const t = DateTime.fromISO(ts, { zone: 'utc' });
// return t >= startUTC && t < endUTC;
// }


const result = {
window: {
zone: ZONE,
startKST: startKST.toISO(),
endKST: endKST.toISO()
},
repos: {},
totals: { done: 0, inProgress: 0, todo: 0, newIssues: 0 }
};


for (const full of REPOS) {
const [owner, repo] = full.split('/');
console.log(`\n=== Collecting: ${owner}/${repo} ===`);


// DONE: issues closed in window
const done = [];
for await (const page of octo.paginate.iterator(octo.rest.issues.listForRepo, {
owner, repo, state: 'closed', since: startUTC.toISO(), per_page: 100
})) {
for (const is of page.data) {
if (is.pull_request) continue; // skip PRs; include them if desired
if (is.closed_at && withinWindow(is.closed_at)) {
done.push({ number: is.number, title: is.title, url: is.html_url, labels: is.labels?.map(l=>l.name)||[], actor: is.closed_by?.login||'' });
}
}
}

// EVENTS in window (excluding 'closed') → inProgress candidates
const changedByEvents = new Map();
for await (const page of octo.paginate.iterator(octo.rest.issues.listEventsForRepo, {
owner, repo, per_page: 100
})) {
for (const ev of page.data) {
if (!ev || !ev.created_at || !ev.issue) continue;
if (!withinWindow(ev.created_at)) continue;
const actor = ev.actor?.login || '';
if (EXCLUDE_ACTORS.has(actor)) continue;
if (ev.event === 'closed') continue; // handled by DONE
const key = ev.issue.number;
const arr = changedByEvents.get(key) || [];
arr.push({ type: ev.event, actor, at: ev.created_at });
changedByEvents.set(key, arr);
}
// Stop early if events are older than window start (optional):
const last = page.data[page.data.length - 1];
if (last && last.created_at && DateTime.fromISO(last.created_at) < startUTC.minus({ days: 2 })) {
// heuristic stop to avoid deep pagination on big repos
// (remove if you need full accuracy for very active repos)
break;
}
}


// COMMENTS in window → inProgress candidates
const changedByComments = new Map();
for await (const page of octo.paginate.iterator(octo.rest.issues.listCommentsForRepo, {
owner, repo, since: startUTC.toISO(), per_page: 100
})) {
for (const c of page.data) {
if (!withinWindow(c.created_at)) continue;
const actor = c.user?.login || '';
if (EXCLUDE_ACTORS.has(actor)) continue;
const issueNum = c.issue_url.split('/').pop();
const key = parseInt(issueNum, 10);
const arr = changedByComments.get(key) || [];
arr.push({ type: 'commented', actor, at: c.created_at });
changedByComments.set(key, arr);
}
}


// IN PROGRESS = (events ∪ comments) − done
const doneSet = new Set(done.map(i => i.number));
const inProgressNums = new Set([...changedByEvents.keys(), ...changedByComments.keys()].filter(n => !doneSet.has(n)));


const inProgress = [];
for (const number of inProgressNums) {
const { data: is } = await octo.rest.issues.get({ owner, repo, issue_number: number });
if (is.pull_request) continue;
inProgress.push({ number, title: is.title, url: is.html_url, labels: is.labels?.map(l=>l.name)||[] });
}


// TODO = open issues with PLAN_LABEL that had no changes yesterday
const todo = [];
for await (const page of octo.paginate.iterator(octo.rest.issues.listForRepo, {
owner, repo, state: 'open', labels: PLAN_LABEL, per_page: 100
})) {
for (const is of page.data) {
if (is.pull_request) continue;
if (!inProgressNums.has(is.number) && !doneSet.has(is.number)) {
todo.push({ number: is.number, title: is.title, url: is.html_url, labels: is.labels?.map(l=>l.name)||[] });
}
}
}

// NEW issues in window (optional metric)
const newIssues = [];
for await (const page of octo.paginate.iterator(octo.rest.issues.listForRepo, {
owner, repo, state: 'all', since: startUTC.toISO(), per_page: 100
})) {
for (const is of page.data) {
if (is.pull_request) continue;
if (is.created_at && withinWindow(is.created_at)) newIssues.push(is.number);
}
}


result.repos[full] = { done, inProgress, todo, counts: { newIssues: newIssues.length } };
result.totals.done += done.length;
result.totals.inProgress += inProgress.length;
result.totals.todo += todo.length;
result.totals.newIssues += newIssues.length;
}


console.log('\nSUMMARY');
console.log(JSON.stringify(result, null, 2));


// ===== Notion export (optional) =====
if (notion && NOTION_DB_ID) {
const dateStr = startKST.toISODate();
const title = `Daily Report - ${dateStr}`;


// Idempotency: check if a page already exists for Date
// (simple approach: query by Date equals)
const existing = await notion.databases.query({
database_id: NOTION_DB_ID,
filter: { property: 'Date', date: { equals: dateStr } },
page_size: 1
});


let pageId = existing.results[0]?.id;
if (!pageId) {
const props = {
Name: { title: [{ text: { content: title } }] },
Date: { date: { start: dateStr } },
Done: { number: result.totals.done },
Progressed: { number: result.totals.inProgress },
NotDone: { number: result.totals.todo },
NewIssues: { number: result.totals.newIssues },
Repos: { multi_select: Object.keys(result.repos).map(name => ({ name })) }
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


// Build body blocks
const children = [];
const h2 = title => ({ heading_2: { rich_text: [{ type: 'text', text: { content: title } }] } });
const bullet = (t, url) => ({ paragraph: { rich_text: [{ type: 'text', text: { content: t, link: url ? { url } : null } }] } });


children.push(h2('✅ Done'));
for (const [full, data] of Object.entries(result.repos)) {
for (const i of data.done) children.push(bullet(`[${full} #${i.number}] ${i.title}`, i.url));
}


children.push(h2('🚧 In Progress'));
for (const [full, data] of Object.entries(result.repos)) {
for (const i of data.inProgress) children.push(bullet(`[${full} #${i.number}] ${i.title}`, i.url));
}


children.push(h2('⏳ Todo'));
for (const [full, data] of Object.entries(result.repos)) {
for (const i of data.todo) children.push(bullet(`[${full} #${i.number}] ${i.title}`, i.url));
}


// Append in chunks (Notion limit: max 100 children per request)
for (let i = 0; i < children.length; i += 90) {
await notion.blocks.children.append({ block_id: pageId, children: children.slice(i, i + 90) });
}


console.log('Notion page updated:', pageId);
}