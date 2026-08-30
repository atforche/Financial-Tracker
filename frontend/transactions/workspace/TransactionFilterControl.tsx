import type { JSX, ReactNode } from "react";
import { Box } from "@mui/material";

/**
 * Props for the TransactionFilterControl component.
 */
interface TransactionFilterControlProps {
  readonly children: ReactNode;
  readonly expand?: boolean;
}

/**
 * Gives each transaction filter control a slot in the three-column layout.
 */
const TransactionFilterControl = function ({
  children,
  expand = true,
}: TransactionFilterControlProps): JSX.Element {
  return (
    <Box
      sx={{
        display: "flex",
        gap: 1.5,
        alignItems: "center",
        flex: expand
          ? { xs: "1 1 100%", md: "1 1 calc(33.333% - 16px)" }
          : "0 0 auto",
        minWidth: expand ? { xs: 0, md: "min(100%, max-content)" } : undefined,
        "& > *": {
          flex: expand ? 1 : "0 0 auto",
          ...(expand ? { minWidth: 0 } : {}),
        },
      }}
    >
      {children}
    </Box>
  );
};

export default TransactionFilterControl;
