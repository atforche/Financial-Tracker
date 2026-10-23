import "server-only";

/**
 * Request header that carries the path and query of the page being requested.
 */
const currentPathHeader = "x-current-path";

/**
 * Origin used to resolve relative callbacks when the public origin is not configured. Only the path of the
 * resolved URL is ever returned, so this origin is never redirected to.
 */
const fallbackOrigin = "http://localhost";

/**
 * Returns a same-origin path for a callback URL, or the home page when it points anywhere else. The
 * callback is parsed the way a browser would parse it, so characters browsers ignore, such as tabs and
 * newlines, cannot disguise an external URL as a path.
 */
const getCallbackPath = function (
  callbackUrl: string | null | undefined,
): string {
  if (typeof callbackUrl !== "string" || callbackUrl === "") {
    return "/";
  }
  let origin = fallbackOrigin;
  try {
    ({ origin } = new URL(process.env["PUBLIC_ORIGIN"] ?? ""));
  } catch {
    // Without a configured public origin, only relative callbacks resolve to this application.
  }
  try {
    const callback = new URL(callbackUrl, origin);
    if (callback.origin !== origin) {
      return "/";
    }
    // A path starting with "//" would itself be read as a link to another host.
    const path = callback.pathname.replace(/^\/+/u, "/");
    return `${path}${callback.search}${callback.hash}`;
  } catch {
    return "/";
  }
};

export { currentPathHeader };
export default getCallbackPath;
