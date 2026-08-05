import "server-only";

/**
 * Confirms a provider token has an active database-backed application user.
 */
const resolveApplicationUser = async function (
  idToken: string,
): Promise<boolean> {
  const apiUrl = process.env["API_URL"];
  if (typeof apiUrl === "undefined" || apiUrl.trim() === "") {
    throw new Error(
      "API_URL must be configured before resolving an application user.",
    );
  }

  const response = await fetch(
    new URL("/authentication/resolve-user", apiUrl).toString(),
    {
      method: "POST",
      headers: {
        Accept: "application/json",
        Authorization: `Bearer ${idToken}`,
      },
      cache: "no-store",
    },
  );
  return response.ok;
};

export default resolveApplicationUser;
