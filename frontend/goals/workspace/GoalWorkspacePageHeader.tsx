import { Button, Stack, Typography } from "@mui/material";
import ArrowBack from "@mui/icons-material/ArrowBack";
import type { JSX } from "react";
import Link from "next/link";

/**
 * Props for the GoalWorkspacePageHeader component.
 */
interface GoalWorkspacePageHeaderProps {
  readonly backHref: string;
  readonly title: string;
}

/**
 * Displays the title and return navigation for a goal workspace page.
 */
const GoalWorkspacePageHeader = function ({
  backHref,
  title,
}: GoalWorkspacePageHeaderProps): JSX.Element {
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
      <Typography variant="h4">{title}</Typography>
    </Stack>
  );
};

export default GoalWorkspacePageHeader;
