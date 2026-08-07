import { type Page, expect, test } from "@playwright/test";

const signInAsLocalDeveloper = async function (page: Page): Promise<void> {
  await page.goto("/accounts/workspace");
  await expect(page).toHaveURL(/\/login\?callbackUrl=/u);
  await page.getByRole("button", { name: "Continue as Administrator" }).click();
  await expect(page).toHaveURL(/\/accounts\/workspace$/u);
};

test("the browser creates a safe authenticated session and onboards an account", async ({
  page,
}) => {
  await signInAsLocalDeveloper(page);
  const onboardAccountButton = page.getByRole("button", {
    name: "Onboard Account",
  });
  await expect(onboardAccountButton).toBeVisible();

  const session: unknown = await page.evaluate(async () => {
    const response = await fetch("/api/auth/session");
    if (!response.ok) {
      throw new Error("The authenticated browser could not read its session.");
    }
    const browserSession: unknown = await response.json();
    return browserSession;
  });
  expect(session).not.toBeNull();
  expect(session).not.toHaveProperty("idToken");

  await onboardAccountButton.click();
  await page.getByRole("textbox", { name: "Name" }).fill("E2E checking");
  await page.getByRole("combobox", { name: "Type" }).click();
  await page.getByRole("option", { name: "Standard" }).click();
  await page.getByRole("textbox", { name: "Starting Balance" }).fill("125.50");
  await page
    .getByRole("button", { name: "Onboard Account", exact: true })
    .last()
    .click();

  await expect(page.getByText("E2E checking", { exact: true })).toBeVisible();
  await page.reload();
  await expect(page.getByText("E2E checking", { exact: true })).toBeVisible();
});
