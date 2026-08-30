"use client";

import type {
  AccountGoal,
  UpdateAccountGoalRequest,
} from "@/account-goals/types";
import { Button, Stack } from "@mui/material";
import {
  type JSX,
  startTransition,
  useActionState,
  useEffect,
  useRef,
  useState,
} from "react";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import Dialog from "@/framework/dialog/Dialog";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import { focusFirstEntryControl } from "@/framework/forms/focusFirstEntryControl";
import updateAccountGoal from "@/account-goals/workspace/updateAccountGoal";
import { useWriteAccess } from "@/framework/auth/ApplicationUserProvider";

interface UpdateAccountGoalFormProps {
  readonly accountGoal: AccountGoal;
  readonly redirectUrl: string;
}

/**
 * Opens a dialog for updating an Account Goal's ending-balance bounds.
 */
const UpdateAccountGoalForm = function ({
  accountGoal,
  redirectUrl,
}: UpdateAccountGoalFormProps): JSX.Element | null {
  const canWrite = useWriteAccess();
  const [open, setOpen] = useState(false);
  const [minimumEndingBalance, setMinimumEndingBalance] = useState(
    accountGoal.minimumEndingBalance ?? null,
  );
  const [maximumEndingBalance, setMaximumEndingBalance] = useState(
    accountGoal.maximumEndingBalance ?? null,
  );
  const formRef = useRef<HTMLDivElement | null>(null);
  const [state, action, pending] = useActionState(updateAccountGoal, {});
  const reset = (): void => {
    setMinimumEndingBalance(accountGoal.minimumEndingBalance ?? null);
    setMaximumEndingBalance(accountGoal.maximumEndingBalance ?? null);
    focusFirstEntryControl(formRef.current);
  };
  useEffect(() => {
    if (state.success === true) {
      setOpen(false);
      reset();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [state.success]);
  const request: UpdateAccountGoalRequest = {
    minimumEndingBalance,
    maximumEndingBalance,
  };
  const rangeIsValid =
    minimumEndingBalance === null ||
    maximumEndingBalance === null ||
    minimumEndingBalance <= maximumEndingBalance;
  if (!canWrite) {
    return null;
  }
  return (
    <>
      <Button
        variant="contained"
        onClick={() => {
          setOpen(true);
        }}
      >
        Edit
      </Button>
      <Dialog
        open={open}
        fullWidth
        maxWidth="sm"
        title="Update Account Goal"
        {...(pending
          ? {}
          : {
              onClose: (): void => {
                setOpen(false);
                reset();
              },
            })}
        actions={
          <>
            <Button
              disabled={pending}
              onClick={() => {
                setOpen(false);
                reset();
              }}
            >
              Cancel
            </Button>
            <Button variant="outlined" disabled={pending} onClick={reset}>
              Reset
            </Button>
            <Button
              variant="contained"
              loading={pending}
              disabled={!rangeIsValid}
              onClick={() => {
                startTransition(() => {
                  action({ accountGoal, request, redirectUrl });
                });
              }}
            >
              Save
            </Button>
          </>
        }
      >
        <Stack ref={formRef} spacing={2}>
          <CurrencyEntryField
            label="Minimum Ending Balance"
            value={minimumEndingBalance}
            setValue={setMinimumEndingBalance}
            errorMessage={state.minimumEndingBalanceErrors ?? null}
          />
          <CurrencyEntryField
            label="Maximum Ending Balance"
            value={maximumEndingBalance}
            setValue={setMaximumEndingBalance}
            errorMessage={state.maximumEndingBalanceErrors ?? null}
          />
          <ErrorAlert
            errorMessage={state.errorTitle ?? null}
            unmappedErrors={state.unmappedErrors ?? null}
          />
        </Stack>
      </Dialog>
    </>
  );
};

export default UpdateAccountGoalForm;
