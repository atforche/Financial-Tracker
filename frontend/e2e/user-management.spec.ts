import {
  type Browser,
  type BrowserContext,
  type Locator,
  type Page,
  expect,
  test,
} from "@playwright/test";

const signInAsLocalDeveloper = async function (page: Page): Promise<void> {
  await page.goto("/admin/users");
  await expect(page).toHaveURL(/\/login\?callbackUrl=/u);
  await page.getByRole("button", { name: "Continue as Administrator" }).click();
  await expect(page).toHaveURL((url) => url.pathname === "/admin/users");
};

const getRow = function (page: Page, email: string): Locator {
  return page.getByRole("row").filter({ hasText: email });
};

const createDevelopmentSession = async function (
  browser: Browser,
  subject: string,
): Promise<{ context: BrowserContext; page: Page }> {
  const secret = process.env["E2E_AUTH_SECRET"];
  if (typeof secret !== "string" || secret === "") {
    throw new Error("The container smoke test must supply E2E_AUTH_SECRET.");
  }

  const context = await browser.newContext({
    viewport: { width: 390, height: 844 },
  });
  const page = await context.newPage();
  await page.goto("/login");
  const { encode } = await import("next-auth/jwt");
  const sessionToken = await encode({
    secret,
    salt: "authjs.session-token",
    token: { idToken: `development:${subject}`, name: "Local developer" },
  });
  await context.addCookies([
    {
      httpOnly: true,
      name: "authjs.session-token",
      sameSite: "Lax",
      url: new URL(page.url()).origin,
      value: sessionToken,
    },
  ]);
  return { context, page };
};

const confirmAction = async function (
  page: Page,
  trigger: Locator,
  confirmationDialogName: string,
  confirmationName: string,
): Promise<void> {
  await trigger.click();
  const dialog = page.getByRole("dialog", { name: confirmationDialogName });
  await expect(dialog).toBeVisible();
  await dialog.getByRole("button", { name: confirmationName }).click();
  await expect(dialog).toBeHidden();
};

const expectReadRoute = async function (
  page: Page,
  route: string,
): Promise<void> {
  await page.goto(route);
  await expect(page).not.toHaveURL(/\/login/u);
  await expect(page.getByText("Unable to load this page")).toHaveCount(0);
};

test("list frames fit within a mobile viewport", async ({ page }) => {
  await page.setViewportSize({ width: 390, height: 844 });
  await signInAsLocalDeveloper(page);

  await expect(
    page.getByText("Application Users", { exact: true }),
  ).toBeVisible();
  await expect(page.locator("table").first()).toBeHidden();
  const applicationUsersFrame = page
    .getByText("Application Users", { exact: true })
    .locator("xpath=ancestor::div[contains(@class, 'MuiPaper-root')][1]");
  await expect(
    applicationUsersFrame.getByText("Email", { exact: true }).last(),
  ).toBeVisible();

  const dimensions = await page.evaluate(() => ({
    documentWidth: document.documentElement.scrollWidth,
    viewportWidth: document.documentElement.clientWidth,
  }));
  expect(dimensions.documentWidth).toBeLessThanOrEqual(
    dimensions.viewportWidth,
  );
});

test("an administrator manages invitations and application user access", async ({
  browser,
  page,
}) => {
  const invitedEmail = "browser-invite@example.test";
  const standardEmail = "container-smoke-standard@example.test";

  await signInAsLocalDeveloper(page);
  await page.getByLabel("Open navigation").click();
  await expect(page.getByRole("link", { name: "Manage users" })).toBeVisible();
  await expect(page.getByRole("link", { name: "User Management" })).toHaveCount(
    0,
  );
  await page.getByRole("link", { name: "Manage users" }).click();
  await expect(page.getByRole("button", { name: "Invite user" })).toBeVisible();

  const userRow = getRow(page, standardEmail);
  await expect(userRow).toContainText("Standard");
  const standardSession = await createDevelopmentSession(
    browser,
    "container-smoke-standard",
  );
  await standardSession.page.goto("/accounts/workspace");
  await expect(
    standardSession.page.getByRole("button", { name: "Onboard Account" }),
  ).toBeVisible();

  await page.getByRole("button", { name: "Invite user" }).click();
  const inviteUserDialog = page.getByRole("dialog", { name: "Invite User" });
  await inviteUserDialog
    .getByRole("textbox", { name: "Email address" })
    .fill(invitedEmail);
  await inviteUserDialog
    .getByRole("button", { name: "Send invitation" })
    .click();
  const invitationRow = getRow(page, invitedEmail);
  await expect(invitationRow).toContainText("Pending");
  await invitationRow.click();
  const invitationDialog = page.getByRole("dialog", {
    name: "Invitation Details",
  });
  await confirmAction(
    page,
    invitationDialog.getByRole("button", { name: "Revoke invitation" }),
    "Revoke Invitation",
    "Revoke",
  );
  await expect(invitationRow).toContainText("Revoked");

  await userRow.click();
  const manageUserDialog = page.getByRole("dialog", {
    name: "Manage Local standard user",
  });
  await manageUserDialog.getByRole("combobox", { name: "Role" }).click();
  await page.getByRole("option", { name: "Read only", exact: true }).click();
  await manageUserDialog.getByRole("button", { name: "Save changes" }).click();
  await expect(page.getByRole("dialog")).toHaveCount(0);
  await expect(userRow).toContainText("Read only");

  await standardSession.page
    .getByRole("button", { name: "Onboard Account" })
    .click();
  await standardSession.page
    .getByRole("textbox", { name: "Name" })
    .fill("Denied stale account");
  await standardSession.page.getByRole("combobox", { name: "Type" }).click();
  await standardSession.page.getByRole("option", { name: "Standard" }).click();
  await standardSession.page
    .getByRole("textbox", { name: "Starting Balance" })
    .fill("1");
  await standardSession.page
    .getByRole("button", { name: "Onboard Account", exact: true })
    .last()
    .click();
  await expect(
    standardSession.page.getByRole("dialog", { name: "Onboard Account" }),
  ).toBeHidden();
  await expect(
    standardSession.page.getByRole("heading", {
      name: "Denied stale account",
    }),
  ).toHaveCount(0);
  await expect(
    standardSession.page.getByRole("button", { name: "Onboard Account" }),
  ).toHaveCount(0);
  await expect(
    standardSession.page.getByRole("link", { name: "Manage users" }),
  ).toHaveCount(0);
  await standardSession.page.getByLabel("Open navigation").click();
  await expect(
    standardSession.page.getByRole("link", { name: "Manage users" }),
  ).toHaveCount(0);
  await standardSession.page.goto("/");
  await standardSession.page.getByLabel("Open navigation").click();
  await standardSession.page
    .getByRole("button", { name: "Accounts", exact: true })
    .last()
    .click();
  await expect(
    standardSession.page
      .getByRole("link", { name: "Trends", exact: true })
      .last(),
  ).toBeVisible();
  await expect(standardSession.page).toHaveURL((url) => url.pathname === "/");
  await standardSession.page
    .getByRole("link", { name: "Trends", exact: true })
    .last()
    .click();
  await expect(standardSession.page).toHaveURL(
    (url) => url.pathname === "/accounts/trends",
  );
  await standardSession.page.goto("/admin/users");
  await expect(
    standardSession.page.getByText("Administrator access required"),
  ).toBeVisible();

  await expectReadRoute(standardSession.page, "/");
  await expectReadRoute(standardSession.page, "/accounting-periods/workspace");
  await expectReadRoute(standardSession.page, "/accounts/workspace");
  await expectReadRoute(standardSession.page, "/funds/workspace");
  await expectReadRoute(standardSession.page, "/goals/workspace");
  await expectReadRoute(standardSession.page, "/transactions/workspace");
  await standardSession.context.close();

  await page.goto("/admin/users");
  const refreshedUserRow = getRow(page, standardEmail);
  await refreshedUserRow.click();
  const refreshedManageUserDialog = page.getByRole("dialog", {
    name: "Manage Local standard user",
  });
  await confirmAction(
    page,
    refreshedManageUserDialog.getByRole("button", { name: "Disable" }),
    "Disable user",
    "Disable",
  );
  await expect(refreshedUserRow).toContainText("Disabled");
  await refreshedUserRow.click();
  const enabledManageUserDialog = page.getByRole("dialog", {
    name: "Manage Local standard user",
  });
  await confirmAction(
    page,
    enabledManageUserDialog.getByRole("button", { name: "Enable" }),
    "Enable user",
    "Enable",
  );
  await expect(refreshedUserRow).toContainText("Active");
});
