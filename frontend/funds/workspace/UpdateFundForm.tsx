"use client";

import { Button, Stack } from "@mui/material";
import type { Fund, UpdateFundRequest } from "@/funds/types";
import {
  type JSX,
  startTransition,
  useActionState,
  useEffect,
  useRef,
  useState,
} from "react";
import Dialog from "@/framework/dialog/Dialog";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import StringEntryField from "@/framework/forms/StringEntryField";
import { focusFirstEntryControl } from "@/framework/forms/focusFirstEntryControl";
import updateFund from "@/funds/workspace/updateFund";
import { useRouter } from "next/navigation";

interface UpdateFundFormProps {
  readonly fund: Fund;
  readonly redirectUrl: string;
}

/**
 * Displays the action for updating the selected fund.
 */
const UpdateFundForm = function ({
  fund,
  redirectUrl,
}: UpdateFundFormProps): JSX.Element {
  const router = useRouter();
  const [open, setOpen] = useState(false);
  const [fundId, setFundId] = useState<string>(fund.id);
  const [name, setName] = useState<string>(fund.name);
  const [description, setDescription] = useState<string>(fund.description);
  const formRef = useRef<HTMLDivElement | null>(null);
  if (fundId !== fund.id) {
    setFundId(fund.id);
    setName(fund.name);
    setDescription(fund.description);
  }

  const [state, action, pending] = useActionState(updateFund, {});

  const reset = function (): void {
    setName(fund.name);
    setDescription(fund.description);
    focusFirstEntryControl(formRef.current);
  };

  useEffect(() => {
    if (state.success === true) {
      setOpen(false);
      router.replace(redirectUrl, { scroll: false });
    }
  }, [redirectUrl, router, state.success]);

  const request: UpdateFundRequest | null =
    name === "" ? null : { name, description };

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
        onClose={
          pending
            ? // eslint-disable-next-line no-undefined
              undefined
            : (): void => {
                setOpen(false);
                reset();
              }
        }
        fullWidth
        maxWidth="sm"
        title="Update Fund"
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
              disabled={request === null}
              onClick={() => {
                if (request !== null) {
                  startTransition(() => {
                    action({ fundId: fund.id, redirectUrl, request });
                  });
                }
              }}
            >
              Save
            </Button>
          </>
        }
      >
        <Stack ref={formRef} spacing={3}>
          <StringEntryField
            label="Name"
            value={name}
            setValue={setName}
            errorMessage={state.nameErrors ?? null}
          />
          <StringEntryField
            label="Description"
            value={description}
            setValue={setDescription}
            errorMessage={state.descriptionErrors ?? null}
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

export default UpdateFundForm;
