import type { AccountIdentifier } from "@/accounts/types";
import { ComboBoxEntryField } from "@/framework/forms/ComboBoxEntryField";
import type { JSX } from "react";

/**
 * Props for the AccountEntryField component.
 */
interface AccountEntryFieldProps {
  readonly label: string;
  readonly options: AccountIdentifier[];
  readonly value: AccountIdentifier | null;
  readonly setValue: ((newValue: AccountIdentifier | null) => void) | null;
  readonly errorMessage?: string | null;
  readonly filter?: ((account: AccountIdentifier) => boolean) | null;
}

/**
 * Component that presents the user with an entry field where they can select an Account.
 */
const AccountEntryField = function ({
  label,
  options,
  value,
  setValue,
  errorMessage = null,
  filter = null,
}: AccountEntryFieldProps): JSX.Element {
  return (
    <ComboBoxEntryField<AccountIdentifier>
      label={label}
      options={options
        .filter((account) => (filter ? filter(account) : true))
        .map((account) => ({
          label: account.name,
          value: { id: account.id, name: account.name },
        }))}
      value={
        value === null
          ? { label: "", value: null }
          : { label: value.name, value }
      }
      setValue={
        setValue === null
          ? null
          : (newValue): void => {
              setValue(newValue?.value ?? null);
            }
      }
      errorMessage={errorMessage}
    />
  );
};

export default AccountEntryField;
