"use client";

import { Button, Stack } from "@mui/material";
import type { JSX } from "react";
import Link from "next/link";
import type { Route } from "next";
import { useWriteAccess } from "@/framework/auth/ApplicationUserProvider";

/**
 * Props for the ExpectedIncomeSourceActions component.
 */
interface ExpectedIncomeSourceActionsProps {
  readonly backUrl: Route;
  readonly editUrl: Route;
  readonly periodIsOpen: boolean;
  readonly onDelete: () => void;
}

/**
 * Displays the actions allowed for an expected-income source.
 */
const ExpectedIncomeSourceActions = function ({
  backUrl,
  editUrl,
  periodIsOpen,
  onDelete,
}: ExpectedIncomeSourceActionsProps): JSX.Element {
  const canManage = useWriteAccess() && periodIsOpen;
  return (
    <Stack direction="row" spacing={1} justifyContent="flex-end">
      <Link href={backUrl} style={{ textDecoration: "none" }}>
        <Button component="span">Back</Button>
      </Link>
      {canManage ? (
        <>
          <Link href={editUrl} style={{ textDecoration: "none" }}>
            <Button component="span" variant="contained">
              Edit
            </Button>
          </Link>
          <Button color="error" onClick={onDelete}>
            Delete
          </Button>
        </>
      ) : null}
    </Stack>
  );
};

export default ExpectedIncomeSourceActions;
