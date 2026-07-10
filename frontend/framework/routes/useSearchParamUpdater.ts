"use client";

import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { buildUrl } from "@/framework/routes/helpers";

/**
 * Updates the current URL search params and replaces the route without scrolling.
 */
const useSearchParamUpdater = function (
  pageParamNames: readonly string[],
): (updater: (params: URLSearchParams) => void) => void {
  const pathname = usePathname();
  const router = useRouter();
  const searchParams = useSearchParams();

  return function (updater: (params: URLSearchParams) => void): void {
    const params = new URLSearchParams(searchParams.toString());
    updater(params);
    pageParamNames.forEach((pageParamName) => {
      params.delete(pageParamName);
    });
    router.replace(buildUrl(pathname, params), { scroll: false });
  };
};

export default useSearchParamUpdater;
