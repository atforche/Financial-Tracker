"use client";

import { Button, Stack } from "@mui/material";
import type { Fund, UpdateFundRequest } from "@/funds/types";
import {
  type JSX,
  startTransition,
  useActionState,
  useEffect,
  useState,
} from "react";
import Dialog from "@/framework/dialog/Dialog";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import StringEntryField from "@/framework/forms/StringEntryField";
import updateFund from "@/funds/workspace/updateFund";
import { useRouter } from "next/navigation";

/**
 * Props for the UpdateFundForm component.
 */
interface UpdateFundFormProps {
  readonly fund: Fund;
  readonly redirectUrl: string;
  readonly onClose: () => void;
}

/**
 * Displays the action for updating the selected fund.
 */
const UpdateFundForm = function ({
  fund,
  redirectUrl,
  onClose,
}: UpdateFundFormProps): JSX.Element {
  const router = useRouter();
  const [name, setName] = useState<string>(fund.name);
  const [description, setDescription] = useState<string>(fund.description);
  const [state, action, pending] = useActionState(updateFund, {});

  useEffect(() => {
    if (state.success === true) {
      onClose();
      router.replace(redirectUrl, { scroll: false });
    }
  }, [onClose, redirectUrl, router, state.success]);

  const request: UpdateFundRequest | null =
    name === "" ? null : { name, description };

  return (
    <Dialog
      open
      onClose={
        pending
          ? undefined
          : (): void => {
              onClose();
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
              onClose();
            }}
          >
            Cancel
          </Button>
          <Button
            variant="contained"
            loading={pending}
            disabled={request === null}
            onClick={() => {
              if (request === null) {
                return;
              }
              startTransition(() => {
                action({ fundId: fund.id, redirectUrl, request });
              });
            }}
          >
            Save
          </Button>
        </>
      }
    >
      <Stack spacing={3}>
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
  );
};

export default UpdateFundForm;
