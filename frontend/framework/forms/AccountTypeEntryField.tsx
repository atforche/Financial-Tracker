import { AccountType } from "@/data/accountTypes";
import { ComboBoxEntryField } from "@/framework/forms/ComboBoxEntryField";
import type { JSX } from "react";

/**
 * Props for the AccountTypeEntryField component.
 */
interface AccountTypeEntryFieldProps {
  readonly label: string;
  readonly value: AccountType | null;
  readonly setValue: (newValue: AccountType | null) => void;
}

/**
 * Component that presents the user with an entry field where they can select an Account Type.
 */
const AccountTypeEntryField = function ({
  label,
  value,
  setValue,
}: AccountTypeEntryFieldProps): JSX.Element {
  return (
    <ComboBoxEntryField<AccountType>
      label={label}
      options={Object.values(AccountType).map((type) => ({
        label: type,
        value: type,
      }))}
      value={
        value === null ? { label: "", value: null } : { label: value, value }
      }
      setValue={(newValue): void => {
        setValue(newValue.value);
      }}
    />
  );
};

export default AccountTypeEntryField;
