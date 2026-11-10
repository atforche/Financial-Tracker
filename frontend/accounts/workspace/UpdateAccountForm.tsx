"use client";

import type { Account, UpdateAccountRequest } from "@/accounts/types";
import {
  type JSX,
  startTransition,
  useActionState,
  useEffect,
  useState,
} from "react";
import CreatableComboBoxEntryField from "@/framework/forms/CreatableComboBoxEntryField";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import FormDialog from "@/framework/dialog/FormDialog";
import { Stack } from "@mui/material";
import StringEntryField from "@/framework/forms/StringEntryField";
import getFinancialInstitutions from "@/accounts/workspace/getFinancialInstitutions";
import updateAccount from "@/accounts/workspace/updateAccount";
import { useRouter } from "next/navigation";

/**
 * Props for the UpdateAccountForm component.
 */
interface UpdateAccountFormProps {
  readonly account: Account;
  readonly redirectUrl: string;
  readonly onClose: () => void;
}

/**
 * Displays the action for updating the selected account.
 */
const UpdateAccountForm = function ({
  account,
  redirectUrl,
  onClose,
}: UpdateAccountFormProps): JSX.Element {
  const router = useRouter();
  const [financialInstitutions, setFinancialInstitutions] = useState<
    readonly string[]
  >([]);
  useEffect(() => {
    startTransition(async () => {
      setFinancialInstitutions(await getFinancialInstitutions());
    });
  }, []);
  const [name, setName] = useState<string>(account.name);
  const [financialInstitution, setFinancialInstitution] = useState<
    string | null
  >(account.financialInstitution);
  const [state, action, pending] = useActionState(updateAccount, {});

  useEffect(() => {
    if (state.success === true) {
      onClose();
      router.replace(redirectUrl, { scroll: false });
    }
  }, [onClose, redirectUrl, router, state.success]);

  let request: UpdateAccountRequest | null = null;
  if (name !== "") {
    request = {
      name,
      financialInstitution,
    };
  }

  return (
    <FormDialog
      open
      title="Update Account"
      onClose={onClose}
      pending={pending}
      submitLabel="Save"
      onSubmit={() => {
        if (request === null) {
          return;
        }
        startTransition(() => {
          action({ accountId: account.id, redirectUrl, request });
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
        <CreatableComboBoxEntryField
          options={financialInstitutions}
          value={financialInstitution}
          setValue={setFinancialInstitution}
        />
        <ErrorAlert
          errorMessage={state.errorTitle ?? null}
          unmappedErrors={state.unmappedErrors ?? null}
        />
      </Stack>
    </FormDialog>
  );
};

export default UpdateAccountForm;
