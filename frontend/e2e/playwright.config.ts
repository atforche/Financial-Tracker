import { defineConfig, devices } from "@playwright/test";

const baseUrl = process.env["E2E_BASE_URL"] ?? "http://127.0.0.1:3001";

/** Configures browser tests against a running, production-built application. */
export default defineConfig({
  testDir: ".",
  fullyParallel: false,
  reporter: process.env["CI"] === "true" ? "github" : "list",
  retries: process.env["CI"] === "true" ? 1 : 0,
  use: {
    baseURL: baseUrl,
    screenshot: "only-on-failure",
    trace: "retain-on-failure",
    video: "retain-on-failure",
  },
  projects: [
    {
      name: "chromium",
      use: { ...devices["Desktop Chrome"] },
    },
  ],
});
