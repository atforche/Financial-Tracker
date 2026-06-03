"use client";

import { Button, DialogActions, Stack } from "@mui/material";
import type { Fund, UpdateFundRequest } from "@/funds/types";
import { type JSX, startTransition, useActionState, useState } from "react";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import StringEntryField from "@/framework/forms/StringEntryField";
import updateFund from "@/funds/workspace/updateFund";

/**
 * Props for the UpdateFundForm component.
 */
interface UpdateFundFormProps {
  readonly fund: Fund;
  readonly redirectUrl: string;
}

/**
 * Component that displays the form for updating a fund.
 */
const UpdateFundForm = function ({
  fund,
  redirectUrl,
}: UpdateFundFormProps): JSX.Element {
  const [name, setName] = useState<string>(fund.name);
  const [description, setDescription] = useState<string>(fund.description);

  const [state, action, pending] = useActionState(updateFund, {});

  let request: UpdateFundRequest | null = null;
  if (name !== "") {
    request = {
      name,
      description,
    };
  }

  return (
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
      <DialogActions>
        <Button
          variant="outlined"
          onClick={() => {
            setName(fund.name);
            setDescription(fund.description);
          }}
        >
          Reset
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
          Update
        </Button>
      </DialogActions>
    </Stack>
  );
};

export default UpdateFundForm;
