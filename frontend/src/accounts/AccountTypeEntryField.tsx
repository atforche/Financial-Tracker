import { AccountType } from "@accounts/ApiTypes";
import ComboBoxEntryField from "@framework/dialog/ComboBoxEntryField";
import type { JSX } from "react";

/**
 * Props for the AccountTypeEntryField component.
 * @param label - Label for this Account Type Entry Field.
 * @param value - Current value for this Account Type Entry Field.
 * @param setValue - Callback to update the value in this Account Type Entry Field. If null, this field is read-only.
 */
interface AccountTypeEntryFieldProps {
  readonly label: string;
  readonly value: AccountType | null;
  readonly setValue?: ((newValue: AccountType | null) => void) | null;
}

/**
 * Component that presents the user with an entry field where they can select an Account Type.
 * @param props - Props for the AccountTypeEntryField component.
 * @returns JSX element representing the AccountTypeEntryField component.
 */
const AccountTypeEntryField = function ({
  label,
  value,
  setValue = null,
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
      setValue={
        setValue
          ? (newValue): void => {
              setValue(newValue.value);
            }
          : null
      }
    />
  );
};

export default AccountTypeEntryField;
