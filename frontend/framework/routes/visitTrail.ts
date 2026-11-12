/**
 * A visited page, with the scroll position and page state the user left it in.
 */
interface Visit {
  readonly url: string;
  scrollY: number;
  readonly state: Record<string, unknown>;
}

/**
 * How the current page was arrived at: the scroll position to show, or null when the arrival only changed the
 * current page's search parameters and the scroll position should be left alone.
 */
interface Arrival {
  readonly scrollY: number | null;
}

const storageKey = "page-visit-trail";
const maximumVisits = 50;

let trail: Visit[] | null = null;

/**
 * Checks whether a stored value is a visit.
 */
const isVisit = function (value: unknown): value is Visit {
  return (
    typeof value === "object" &&
    value !== null &&
    "url" in value &&
    typeof value.url === "string" &&
    "scrollY" in value &&
    typeof value.scrollY === "number" &&
    "state" in value &&
    typeof value.state === "object" &&
    value.state !== null
  );
};

/**
 * Gets the visit trail, reading it from session storage on first use.
 */
const getTrail = function (): Visit[] {
  if (trail === null) {
    try {
      const value: unknown = JSON.parse(
        sessionStorage.getItem(storageKey) ?? "[]",
      );
      trail = Array.isArray(value) ? value.filter(isVisit) : [];
    } catch {
      trail = [];
    }
  }
  return trail;
};

/**
 * Saves the visit trail, ignoring storage that is unavailable.
 */
const saveTrail = function (): void {
  const visits = getTrail();
  visits.splice(0, Math.max(visits.length - maximumVisits, 0));
  try {
    sessionStorage.setItem(storageKey, JSON.stringify(visits));
  } catch {
    // Restoring a page is a convenience; without storage pages start fresh
  }
};

/**
 * Builds a page key whose search parameters are sorted, so links that order them differently still match.
 */
const getPageUrl = function (pathname: string, search: string): string {
  const params = new URLSearchParams(search);
  params.sort();
  const query = params.toString();
  return query === "" ? pathname : `${pathname}?${query}`;
};

/**
 * Gets the pathname of a page key.
 */
const getPathname = function (url: string): string {
  return url.split("?")[0] ?? url;
};

/**
 * Records arriving at a page. Arriving from another page at a page visited earlier in the trail is a return, which
 * keeps that visit's scroll position and state; any other arrival starts a fresh visit. Changes that only update the
 * current page's search parameters keep the current visit.
 */
const recordArrival = function (url: string): Arrival {
  const visits = getTrail();
  const current = visits.at(-1);
  if (current !== undefined && getPathname(current.url) === getPathname(url)) {
    visits[visits.length - 1] = { ...current, url };
    saveTrail();
    return { scrollY: null };
  }
  const index = visits.findLastIndex((visit) => visit.url === url);
  if (index === -1) {
    visits.push({ url, scrollY: 0, state: {} });
    saveTrail();
    return { scrollY: 0 };
  }
  visits.splice(index + 1);
  saveTrail();
  return { scrollY: visits[index]?.scrollY ?? 0 };
};

/**
 * Gets the visit for the current page.
 */
const getCurrentVisit = function (): Visit | undefined {
  return getTrail().at(-1);
};

export { getCurrentVisit, getPageUrl, recordArrival, saveTrail };
