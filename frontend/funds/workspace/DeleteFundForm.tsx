"use client";

import { Button, DialogActions, Stack, Typography } from "@mui/material";
import { type JSX, startTransition, useActionState } from "react";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import type { Fund } from "@/funds/types";
import deleteFund from "@/funds/workspace/deleteFund";

/**
 * Props for the DeleteFundForm component.
 */
interface DeleteFundFormProps {
  readonly fund: Fund;
  readonly redirectUrl: string;
}

/**
 * Component that displays the form for deleting a fund.
 */
const DeleteFundForm = function ({
  fund,
  redirectUrl,
}: DeleteFundFormProps): JSX.Element {
  const [state, action, pending] = useActionState(deleteFund, {
    fundId: fund.id,
    redirectUrl,
  });

  return (
    <Stack spacing={3}>
      <Typography>
        Are you sure you want to delete the fund &quot;{fund.name}&quot;?
      </Typography>
      <ErrorAlert
        errorMessage={state.errorTitle ?? null}
        unmappedErrors={state.unmappedErrors ?? null}
      />
      <DialogActions>
        <Button
          variant="contained"
          loading={pending}
          onClick={() => {
            startTransition(() => {
              action();
            });
          }}
        >
          Delete fund
        </Button>
      </DialogActions>
    </Stack>
  );
};

export default DeleteFundForm;
