"use client";

import { Button, Stack, Typography } from "@mui/material";
import type { JSX } from "react";
import Link from "next/link";
import routes from "@/transactions/routes";
import { useWriteAccess } from "@/framework/auth/ApplicationUserProvider";

/**
 * Props for the OverviewPageHeader component.
 */
interface OverviewPageHeaderProps {
  readonly title: string;
}

/**
 * Displays the Overview page heading and its primary action.
 */
const OverviewPageHeader = function ({
  title,
}: OverviewPageHeaderProps): JSX.Element {
  const canWrite = useWriteAccess();

  return (
    <Stack
      direction={{ xs: "column", sm: "row" }}
      spacing={2}
      alignItems={{ xs: "stretch", sm: "center" }}
      justifyContent="space-between"
    >
      <Typography variant="h4">{title}</Typography>
      {canWrite ? (
        <Link
          href={routes.workspaceCreate({ returnUrl: "/" })}
          style={{ textDecoration: "none" }}
        >
          <Button component="span" variant="contained">
            Add Transaction
          </Button>
        </Link>
      ) : null}
    </Stack>
  );
};

export default OverviewPageHeader;
