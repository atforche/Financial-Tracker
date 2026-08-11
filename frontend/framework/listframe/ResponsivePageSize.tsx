"use client";

import {
  desktopRowsPerPage,
  getRowsPerPage,
  mobileRowsPerPage,
} from "@/framework/listframe/page";
import { useEffect } from "react";
import { useMediaQuery } from "@mui/material";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useSearchParams } from "next/navigation";
import { useTheme } from "@mui/material/styles";

/**
 * Props for the ResponsivePageSize component.
 */
interface ResponsivePageSizeProps {
  /** The viewport width at which this page uses its desktop page size. */
  readonly desktopBreakpoint: "sm" | "md" | "lg" | "xl";
}

/**
 * Keeps the page's shared list size aligned with its responsive layout.
 */
const ResponsivePageSize = function ({
  desktopBreakpoint,
}: ResponsivePageSizeProps): null {
  const searchParams = useSearchParams();
  const updateParams = useSearchParamUpdater([]);
  const theme = useTheme();
  const isDesktopLayout = useMediaQuery(
    theme.breakpoints.up(desktopBreakpoint),
    { noSsr: true },
  );
  const rowsPerPage = getRowsPerPage(searchParams.get("pageSize"));

  useEffect(() => {
    const expectedRowsPerPage = isDesktopLayout
      ? desktopRowsPerPage
      : mobileRowsPerPage;
    if (rowsPerPage !== expectedRowsPerPage) {
      updateParams((params) => {
        params.set("pageSize", expectedRowsPerPage.toString());
      });
    }
  }, [isDesktopLayout, rowsPerPage, updateParams]);

  return null;
};

export default ResponsivePageSize;
