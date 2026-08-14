import { Button, Stack, Typography } from "@mui/material";
import ArrowBack from "@mui/icons-material/ArrowBack";
import type { JSX } from "react";
import Link from "next/link";

/**
 * Props for the TransactionWorkspacePageHeader component.
 */
interface TransactionWorkspacePageHeaderProps {
  readonly backHref: string;
  readonly backLabel?: string;
  readonly title: string;
}

/**
 * Component that displays the header for the transaction workspace page.
 */
const TransactionWorkspacePageHeader = function ({
  backHref,
  backLabel = "Back to Workspace",
  title,
}: TransactionWorkspacePageHeaderProps): JSX.Element {
  return (
    <Stack spacing={2.5}>
      <Link
        href={backHref}
        style={{ alignSelf: "flex-start", textDecoration: "none" }}
      >
        <Button component="span" startIcon={<ArrowBack />}>
          {backLabel}
        </Button>
      </Link>
      <Typography variant="h4">{title}</Typography>
    </Stack>
  );
};

export default TransactionWorkspacePageHeader;
