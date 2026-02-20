import type { AccountIdentifier } from "@accounts/ApiTypes";
import { ComboBoxEntryField } from "@framework/dialog/ComboBoxEntryField";
import type { JSX } from "react";
import useGetAllAccounts from "@accounts/useGetAllAccounts";

/**
 * Props for the AccountEntryField component.
 */
interface AccountEntryFieldProps {
  readonly label: string;
  readonly value: AccountIdentifier | null;
  readonly setValue?: ((newValue: AccountIdentifier | null) => void) | null;
}

/**
 * Component that presents the user with an entry field where they can select an Account.
 * @param props - Props for the AccountEntryField component.
 * @returns JSX element representing the AccountEntryField component.
 */
const AccountEntryField = function ({
  label,
  value,
  setValue = null,
}: AccountEntryFieldProps): JSX.Element {
  const { accounts, isLoading, error } = useGetAllAccounts({
    page: null,
    rowsPerPage: null,
  });
  return (
    <ComboBoxEntryField<AccountIdentifier>
      label={label}
      options={
        accounts?.map((account) => ({
          label: account.name,
          value: { id: account.id, name: account.name },
        })) ?? []
      }
      value={
        value === null
          ? { label: "", value: null }
          : { label: value.name, value }
      }
      setValue={
        setValue
          ? (newValue): void => {
              setValue(newValue.value);
            }
          : null
      }
      loading={isLoading}
      error={error}
    />
  );
};

export default AccountEntryField;
