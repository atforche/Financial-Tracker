import { writeFileSync } from "node:fs";

const apiUrl = process.env["BASELINE_API_URL"];
const fixturePath = process.env["BASELINE_FIXTURE"];
if (!apiUrl || !fixturePath) {
  throw new Error("BASELINE_API_URL and BASELINE_FIXTURE are required.");
}
const parsedUrl = new URL(apiUrl);
if (!["localhost", "127.0.0.1"].includes(parsedUrl.hostname)) {
  throw new Error("The baseline seeder requires a loopback API URL.");
}

const request = async (method, path, body) => {
  const response = await fetch(new URL(path, apiUrl), {
    method,
    headers: {
      Authorization: "Bearer development:local-developer",
      ...(body === undefined ? {} : { "Content-Type": "application/json" }),
    },
    ...(body === undefined ? {} : { body: JSON.stringify(body) }),
  });
  if (!response.ok) {
    throw new Error(`${method} ${path} returned ${response.status}.`);
  }
  const text = await response.text();
  return text === "" ? null : JSON.parse(text);
};

const existingPeriods = await request("GET", "/accounting-periods?limit=1");
const existingAccounts = await request(
  "GET",
  "/accounts/with-balances?limit=1",
);
if (existingPeriods.totalCount !== 0 || existingAccounts.totalCount !== 0) {
  throw new Error("Seed only a freshly migrated, empty development database.");
}

const account = await request("POST", "/accounts/onboard", {
  name: "Baseline Cash",
  financialInstitution: "Baseline Bank",
  type: "Standard",
  onboardedBalance: 100,
});
const period = await request("POST", "/accounting-periods", {
  year: 2026,
  month: 7,
});
const fund = await request("POST", "/funds", {
  name: "Baseline Groceries",
  description: "Baseline Groceries",
  accountingPeriodId: period.id,
});
const transaction = await request("POST", "/transactions", {
  type: "Spending",
  accountingPeriodId: period.id,
  date: "2026-07-15",
  description: "Baseline Market",
  amount: 20,
  source: { accountId: account.id },
  destinations: [
    {
      location: { newLocationName: "Baseline Market" },
      amount: 20,
      fundAssignments: [{ fundId: fund.id, amount: 20 }],
    },
  ],
});
await request("POST", `/transactions/${transaction.id}/post`, {
  accountId: account.id,
  date: "2026-07-15",
});
const periodWithSource = await request(
  "POST",
  `/accounting-periods/${period.id}/expected-income-sources`,
  {
    name: "Baseline Employer",
    incomeLines: [{ description: "Salary", amount: 100 }],
    incomeDeductions: [],
    untrackedTransfers: [],
    expectedDates: ["2026-07-15"],
  },
);
const source = periodWithSource.expectedIncomeSources.find(
  (item) => item.name === "Baseline Employer",
);
const locations = await request("GET", "/locations");
const location = locations.items.find(
  (item) => item.name === "Baseline Market",
);
if (!source || !location) {
  throw new Error(
    "The seeded expected income source or location was not returned.",
  );
}
writeFileSync(
  fixturePath,
  JSON.stringify(
    {
      accountId: account.id,
      periodId: period.id,
      fundId: fund.id,
      transactionId: transaction.id,
      sourceId: source.id,
      locationId: location.id,
    },
    null,
    2,
  ),
);
console.log(`Created isolated browser fixture at ${fixturePath}.`);
