"use client";

import { Button, Paper, Stack, Typography } from "@mui/material";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import DeleteFundForm from "@/funds/workspace/DeleteFundForm";
import type { Fund } from "@/funds/types";
import type { FundWorkspaceAction } from "@/funds/workspace/FundWorkspace";
import type { JSX } from "react";
import UpdateFundForm from "@/funds/workspace/UpdateFundForm";

/**
 * Props for the FundWorkspaceActions component.
 */
interface FundWorkspaceActionsProps {
  readonly selectedFund: Fund | null;
  readonly requestedAction: FundWorkspaceAction | null;
}

/**
 * Displays the available fund actions for the current workspace selection.
 */
const FundWorkspaceActions = function ({
  selectedFund,
  requestedAction,
}: FundWorkspaceActionsProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const setAction = function (action: FundWorkspaceAction | null): void {
    const params = new URLSearchParams(searchParams.toString());

    if (action === null) {
      params.delete("action");
    } else {
      params.set("action", action);
    }

    const query = params.toString();
    router.replace(query === "" ? pathname : `${pathname}?${query}`, {
      scroll: false,
    });
  };

  if (selectedFund === null) {
    return (
      <Paper
        sx={{
          border: "1px solid",
          borderColor: "divider",
          borderRadius: 3,
          p: { xs: 2.5, md: 3 },
        }}
      >
        <Stack spacing={2}>
          <Typography variant="h6">Fund Actions</Typography>
          <Typography variant="body2" color="text.secondary">
            Select a fund from the workspace to update or delete it.
          </Typography>
        </Stack>
      </Paper>
    );
  }

  const activeAction = requestedAction === "delete" ? "delete" : "update";

  return (
    <Paper
      sx={{
        border: "1px solid",
        borderColor: "divider",
        borderRadius: 3,
        p: { xs: 2.5, md: 3 },
      }}
    >
      <Stack spacing={3}>
        <Stack direction="row" spacing={1.5}>
          <Button
            variant={activeAction === "update" ? "contained" : "outlined"}
            onClick={() => {
              setAction("update");
            }}
          >
            Update
          </Button>
          <Button
            color="error"
            variant={activeAction === "delete" ? "contained" : "outlined"}
            onClick={() => {
              setAction("delete");
            }}
          >
            Delete
          </Button>
        </Stack>
        {activeAction === "update" ? (
          <UpdateFundForm fund={selectedFund} redirectUrl={pathname} />
        ) : (
          <DeleteFundForm fund={selectedFund} redirectUrl={pathname} />
        )}
      </Stack>
    </Paper>
  );
};

export default FundWorkspaceActions;
