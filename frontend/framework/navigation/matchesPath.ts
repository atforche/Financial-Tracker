import type { Route } from "next";

/**
 * Matches a route and any of its nested path segments.
 */
const matchesPath = function (pathname: string, href: Route): boolean {
  const routePathname = String(href).split("?", 1)[0] ?? "";
  return (
    pathname === routePathname ||
    (routePathname !== "/" && pathname.startsWith(`${routePathname}/`))
  );
};

export default matchesPath;
