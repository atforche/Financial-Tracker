import type { JSX, ReactNode } from "react";
import { Stack } from "@mui/material";

/**
 * Props for the PageLayout component.
 */
interface PageLayoutProps {
  readonly children: ReactNode;
}

/**
 * Provides the standard spacing and width for page-level content.
 */
const PageLayout = function ({ children }: PageLayoutProps): JSX.Element {
  return (
    <Stack spacing={3} sx={{ width: "100%" }}>
      {children}
    </Stack>
  );
};

export default PageLayout;
