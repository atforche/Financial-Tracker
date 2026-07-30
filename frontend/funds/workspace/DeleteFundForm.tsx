"use client";

import { type JSX, startTransition, useActionState } from "react";
import { Button } from "@mui/material";
import ConfirmActionDialog from "@/framework/dialog/ConfirmActionDialog";
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
 * Displays the action for deleting the selected fund.
 */
const DeleteFundForm = function ({
  fund,
  redirectUrl,
}: DeleteFundFormProps): JSX.Element {
  const [state, action, pending] = useActionState(deleteFund, {});

  return (
    <ConfirmActionDialog
      trigger={(openDialog) => (
        <Button color="error" variant="outlined" onClick={openDialog}>
          Delete
        </Button>
      )}
      title="Delete Fund"
      confirmationCopy={
        <>Are you sure you want to delete the fund &quot;{fund.name}&quot;?</>
      }
      confirmLabel="Delete"
      confirmButtonProps={{ color: "error" }}
      pending={pending}
      errorTitle={state.errorTitle}
      unmappedErrors={state.unmappedErrors}
      onConfirm={() => {
        startTransition(() => {
          action({ fundId: fund.id, redirectUrl });
        });
      }}
    />
  );
};

export default DeleteFundForm;
