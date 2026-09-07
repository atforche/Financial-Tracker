import { Button, Stack, Typography } from "@mui/material";
import ArrowBack from "@mui/icons-material/ArrowBack";
import type { JSX } from "react";
import Link from "next/link";

/**
 * Props for the FundGoalWorkspacePageHeader component.
 */
interface FundGoalWorkspacePageHeaderProps {
  readonly backHref: string;
}

/**
 * Displays the title and return navigation for a Fund Goal workspace page.
 */
const FundGoalWorkspacePageHeader = function ({
  backHref,
}: FundGoalWorkspacePageHeaderProps): JSX.Element {
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
      <Typography variant="h4">Fund Goal Details</Typography>
    </Stack>
  );
};

export default FundGoalWorkspacePageHeader;
