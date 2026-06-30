import { Button, Stack, Typography } from "@mui/material";
import ArrowBack from "@mui/icons-material/ArrowBack";
import type { JSX } from "react";
import Link from "next/link";

/**
 * Props for the TransactionWorkspacePageHeader component.
 */
interface TransactionWorkspacePageHeaderProps {
  readonly backHref: string;
  readonly title: string;
  readonly description: string;
}

/**
 * Component that displays the header for the transaction workspace page.
 */
const TransactionWorkspacePageHeader = function ({
  backHref,
  title,
  description,
}: TransactionWorkspacePageHeaderProps): JSX.Element {
  return (
    <Stack spacing={2.5}>
      <Link
        href={backHref}
        style={{ alignSelf: "flex-start", textDecoration: "none" }}
      >
        <Button component="span" startIcon={<ArrowBack />}>
          Back to Workspace
        </Button>
      </Link>
      <Stack spacing={0.75}>
        <Typography variant="h4">{title}</Typography>
        <Typography color="text.secondary">{description}</Typography>
      </Stack>
    </Stack>
  );
};

export default TransactionWorkspacePageHeader;
