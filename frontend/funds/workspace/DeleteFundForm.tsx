"use client";

import { type JSX, startTransition, useActionState, useEffect } from "react";
import { Button } from "@mui/material";
import ConfirmActionDialog from "@/framework/dialog/ConfirmActionDialog";
import type { Fund } from "@/funds/types";
import deleteFund from "@/funds/workspace/deleteFund";
import { useRouter } from "next/navigation";

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
  const router = useRouter();
  const [state, action, pending] = useActionState(deleteFund, {});

  useEffect(() => {
    if (state.success === true) {
      router.replace(redirectUrl, { scroll: false });
    }
  }, [redirectUrl, router, state.success]);

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
      confirmColor="error"
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
