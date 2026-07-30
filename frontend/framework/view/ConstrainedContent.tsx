import type { JSX, ReactNode } from "react";
import { Box } from "@mui/material";

/**
 * Props for the ConstrainedContent component.
 */
interface ConstrainedContentProps {
  readonly children: ReactNode;
  readonly maxWidth?: number;
}

/**
 * Constrains page content while allowing it to use the full available width.
 */
const ConstrainedContent = function ({
  children,
  maxWidth = 1440,
}: ConstrainedContentProps): JSX.Element {
  return <Box sx={{ maxWidth, width: "100%" }}>{children}</Box>;
};

export default ConstrainedContent;
