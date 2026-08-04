import { expect, test } from "@playwright/test";
import { encode } from "next-auth/jwt";

const encodeBase64Url = function (value: object): string {
  return Buffer.from(JSON.stringify(value)).toString("base64url");
};

const createExpiredIdToken = function (): string {
  const header = encodeBase64Url({ alg: "none", typ: "JWT" });
  const payload = encodeBase64Url({
    exp: Math.floor(Date.now() / 1000) - 60,
    sub: "expired-e2e-subject",
  });
  return `${header}.${payload}.test-signature`;
};

test("an expired provider token is rejected by the frontend session", async ({
  page,
}) => {
  const secret = process.env["E2E_AUTH_SECRET"];
  test.skip(
    typeof secret !== "string" || secret === "",
    "The container smoke test supplies the Auth.js secret for this cookie-level test.",
  );
  if (typeof secret !== "string" || secret === "") {
    return;
  }

  await page.goto("/login");
  const applicationOrigin = new URL(page.url()).origin;
  const sessionToken = await encode({
    secret,
    salt: "authjs.session-token",
    token: {
      idToken: createExpiredIdToken(),
      name: "Expired user",
      sub: "expired-e2e-subject",
    },
  });

  await page.context().addCookies([
    {
      httpOnly: true,
      name: "authjs.session-token",
      sameSite: "Lax",
      url: applicationOrigin,
      value: sessionToken,
    },
  ]);

  await page.goto("/accounts/workspace");
  await expect(page).toHaveURL(/\/login\?callbackUrl=/u);
  await expect
    .poll(async () => {
      const cookies = await page.context().cookies();
      return cookies.some((cookie) => cookie.name === "authjs.session-token");
    })
    .toBe(false);
});
