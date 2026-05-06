import type { Breadcrumb } from "@/framework/Breadcrumbs";
import type { Route } from "next";

/**
 * Defines the structure of the navigation context, which can be used to manage state and behavior related to navigation in the application.
 */
interface NavigationContext {
  /**
   * Populates the navigation context with data from the API.
   */
  populate: () => Promise<void>;

  /**
   * Gets the route for the current navigation context.
   */
  getRoute: () => Route;

  /**
   * Gets the redirect target for the current navigation context.
   */
  getRedirect: () => Route;

  /**
   * Gets the breadcrumbs for the current navigation context.
   */
  getBreadcrumbs: () => Breadcrumb[];
}

/**
 * Converts an arbitrary object to a URLSearchParams instance.
 * Handles nested objects and arrays by serializing them as JSON strings.
 */
const objectToSearchParams = function (
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  obj: Record<string, any> | null,
): URLSearchParams {
  const params = new URLSearchParams();
  if (!obj) {
    return params;
  }
  for (const [key, value] of Object.entries(obj)) {
    if (typeof value === "undefined" || value === null) {
      continue;
    }

    if (typeof value === "object") {
      params.set(key, JSON.stringify(value));
    } else {
      params.set(key, String(value));
    }
  }

  return params;
};

export { type NavigationContext, objectToSearchParams };
