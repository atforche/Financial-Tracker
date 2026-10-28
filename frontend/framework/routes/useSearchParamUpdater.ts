"use client";

import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { buildUrl } from "@/framework/routes/helpers";
import { useNavigationTransition } from "@/framework/routes/NavigationTransitionProvider";

/**
 * Updates the current URL search params and replaces the route without scrolling, fading the page content while the
 * new page data loads.
 */
const useSearchParamUpdater = function (
  pageParamNames: readonly string[],
): (updater: (params: URLSearchParams) => void) => void {
  const pathname = usePathname();
  const router = useRouter();
  const searchParams = useSearchParams();
  const startTransition = useNavigationTransition();

  return function (updater: (params: URLSearchParams) => void): void {
    const params = new URLSearchParams(searchParams.toString());
    updater(params);
    pageParamNames.forEach((pageParamName) => {
      params.delete(pageParamName);
    });
    startTransition(() => {
      router.replace(buildUrl(pathname, params), { scroll: false });
    });
  };
};

export default useSearchParamUpdater;
