"use client";

import type { Fund, UpdateFundRequest } from "@/funds/types";
import {
  type JSX,
  startTransition,
  useActionState,
  useEffect,
  useState,
} from "react";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import FormDialog from "@/framework/dialog/FormDialog";
import { Stack } from "@mui/material";
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
    <FormDialog
      open
      title="Update Fund"
      onClose={onClose}
      pending={pending}
      submitLabel="Save"
      onSubmit={() => {
        if (request === null) {
          return;
        }
        startTransition(() => {
          action({ fundId: fund.id, redirectUrl, request });
        });
      }}
      canSubmit={request !== null}
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
    </FormDialog>
  );
};

export default UpdateFundForm;
