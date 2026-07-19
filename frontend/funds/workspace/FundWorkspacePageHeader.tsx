import { Button, Stack, Typography } from "@mui/material";
import ArrowBack from "@mui/icons-material/ArrowBack";
import type { JSX } from "react";
import Link from "next/link";

/**
 * Props for the FundWorkspacePageHeader component.
 */
interface FundWorkspacePageHeaderProps {
  readonly backHref: string;
  readonly title: string;
}

/**
 * Displays the shared page header for fund workspace sub-pages.
 */
const FundWorkspacePageHeader = function ({
  backHref,
  title,
}: FundWorkspacePageHeaderProps): JSX.Element {
  return (
    <Stack spacing={2.5}>
      <Button
        component={Link}
        href={backHref}
        startIcon={<ArrowBack />}
        sx={{ alignSelf: "flex-start" }}
      >
        Back to Workspace
      </Button>
      <Typography variant="h4">{title}</Typography>
    </Stack>
  );
};

export default FundWorkspacePageHeader;
