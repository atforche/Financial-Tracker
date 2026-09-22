import { createRequire } from "node:module";
import { readFileSync, writeFileSync } from "node:fs";

const requireFrontend = createRequire(
  new URL("../frontend/package.json", import.meta.url),
);
const { chromium } = requireFrontend("@playwright/test");

const fixturePath = process.env["BASELINE_FIXTURE"];
const apiLogPath = process.env["BASELINE_API_LOG"];
const outputPath = process.env["BASELINE_OUTPUT"];
const frontendUrl =
  process.env["BASELINE_FRONTEND_URL"] ?? "http://localhost:13001";

if (!fixturePath || !apiLogPath || !outputPath) {
  throw new Error(
    "BASELINE_FIXTURE, BASELINE_API_LOG, and BASELINE_OUTPUT are required.",
  );
}

const ids = JSON.parse(readFileSync(fixturePath, "utf8"));
const periodMode = `?mode=accounting-period&startAccountingPeriodId=${ids.periodId}&endAccountingPeriodId=${ids.periodId}`;
const allRoutes = [
  "/",
  "/accounts/workspace",
  `/accounts/workspace/${ids.accountId}`,
  "/accounts/trends",
  "/funds/workspace",
  `/funds/workspace/${ids.fundId}`,
  "/funds/workspace/create",
  "/funds/workspace/onboard",
  "/funds/trends",
  "/accounting-periods/workspace",
  `/accounting-periods/workspace/${ids.periodId}`,
  `/accounting-periods/workspace/${ids.periodId}/plan`,
  "/accounting-periods/trends",
  `/accounting-periods/workspace/${ids.periodId}/expected-income-sources/create`,
  `/accounting-periods/workspace/${ids.periodId}/expected-income-sources/${ids.sourceId}`,
  `/accounting-periods/workspace/${ids.periodId}/expected-income-sources/${ids.sourceId}/edit`,
  "/account-goals/workspace",
  `/account-goals/workspace/${ids.accountId}`,
  "/account-goals/trends",
  "/fund-goals/workspace",
  `/fund-goals/workspace/${ids.fundId}`,
  "/fund-goals/trends",
  "/locations",
  `/locations/${ids.locationId}`,
  "/locations/trends",
  "/transactions/workspace",
  "/transactions/workspace/create",
  `/transactions/workspace/${ids.transactionId}`,
  `/transactions/workspace/${ids.transactionId}/edit`,
  "/admin/users",
  "/" + periodMode,
  "/accounts/trends" + periodMode,
  "/funds/trends" + periodMode,
  "/locations/trends" + periodMode,
  `/transactions/workspace?accountingPeriodIds=${ids.periodId}`,
];
const startIndex = Number(process.env["BASELINE_ROUTE_START"] ?? "0");
const endIndex = Number(
  process.env["BASELINE_ROUTE_END"] ?? String(allRoutes.length),
);
if (
  !Number.isInteger(startIndex) ||
  !Number.isInteger(endIndex) ||
  startIndex < 0 ||
  endIndex > allRoutes.length ||
  startIndex >= endIndex
) {
  throw new Error(
    "BASELINE_ROUTE_START and BASELINE_ROUTE_END must select a nonempty route batch.",
  );
}
const routes = allRoutes.slice(startIndex, endIndex);

const readLog = () => readFileSync(apiLogPath);

const callsFromLog = (segment) => {
  const starts = [
    ...segment.matchAll(/Request starting HTTP\/\S+ (GET|POST) (\S+) -/gu),
  ].map((match) => ({ method: match[1], url: match[2] }));
  const finishes = [
    ...segment.matchAll(
      /Request finished HTTP\/\S+ (GET|POST) (\S+) - (\d+) .*? ([\d.]+)ms/gu,
    ),
  ].map((match) => ({
    method: match[1],
    url: match[2],
    status: Number(match[3]),
    durationMs: Number(match[4]),
  }));
  return starts.map((start) => {
    const finishIndex = finishes.findIndex(
      (finish) => finish.method === start.method && finish.url === start.url,
    );
    const finish = finishIndex < 0 ? null : finishes.splice(finishIndex, 1)[0];
    const parsed = new URL(start.url);
    return {
      method: start.method,
      path: parsed.pathname + parsed.search,
      status: finish?.status ?? null,
      backendDurationMs: finish?.durationMs ?? null,
    };
  });
};

const browser = await chromium.launch({ headless: true });
try {
  const page = await browser.newPage();
  await page.goto(frontendUrl + "/accounts/workspace");
  await page.getByRole("button", { name: "Continue as Administrator" }).click();
  await page.waitForURL("**/accounts/workspace");

  const results = [];
  for (const route of routes) {
    const before = readLog().length;
    let pageStatus = null;
    let error = null;
    try {
      const response = await page.goto(frontendUrl + route, {
        waitUntil: "networkidle",
        timeout: 30000,
      });
      pageStatus = response?.status() ?? null;
      await page.waitForTimeout(150);
    } catch (cause) {
      error =
        cause instanceof Error ? cause.message.split("\n")[0] : String(cause);
    }
    const segment = readLog().subarray(before).toString("utf8");
    const calls = callsFromLog(segment);
    const renderedError =
      (await page.getByText("Unable to load this page").count()) > 0;
    results.push({
      route,
      finalPath: new URL(page.url()).pathname,
      pageStatus,
      error,
      renderedError,
      apiRequestCount: calls.length,
      backendDurationSumMs: Math.round(
        calls.reduce((sum, call) => sum + (call.backendDurationMs ?? 0), 0),
      ),
      calls,
    });
    console.log(
      `${route} | ${calls.length} API requests | ${pageStatus ?? "no response"}${error ? ` | ${error}` : ""}`,
    );
  }
  writeFileSync(outputPath, JSON.stringify(results, null, 2));
  if (
    results.some(
      (result) =>
        result.pageStatus !== 200 ||
        result.error ||
        result.renderedError ||
        result.calls.some((call) => call.status !== 200),
    )
  ) {
    process.exitCode = 1;
  }
} finally {
  await browser.close();
}
