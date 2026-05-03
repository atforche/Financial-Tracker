import type { Account } from "@/accounts/types";
import { ComboBoxEntryField } from "@/framework/forms/ComboBoxEntryField";
import type { Fund } from "@/funds/types";
import type { JSX } from "react";

/**
 * Props for the AccountOrFundEntryField component.
 */
interface AccountOrFundEntryFieldProps {
  readonly label: string;
  readonly options: (Account | Fund)[];
  readonly value: Account | Fund | null;
  readonly setValue: ((newValue: Account | Fund | null) => void) | null;
  readonly filter: ((option: Account | Fund) => boolean) | null;
}

/**
 * Component that presents the user with an entry field where they can select either an Account or a Fund.
 */
const AccountOrFundEntryField = function ({
  label,
  options,
  value,
  setValue,
  filter,
}: AccountOrFundEntryFieldProps): JSX.Element {
  return (
    <ComboBoxEntryField<Account | Fund>
      label={label}
      options={options
        .filter((option) => (filter ? filter(option) : true))
        .map((option) => ({
          label: option.name,
          value: option,
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
    />
  );
};

export default AccountOrFundEntryField;
