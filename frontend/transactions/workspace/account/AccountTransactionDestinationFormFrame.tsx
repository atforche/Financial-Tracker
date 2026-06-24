import type { Account, AccountIdentifier } from "@/accounts/types";
import AccountEntryField from "@/accounts/AccountEntryField";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { JSX } from "react";
import StringEntryField from "@/framework/forms/StringEntryField";
import TransactionFrame from "@/transactions/workspace/TransactionFrame";
import Typography from "@mui/material/Typography";

interface AccountTransactionDestinationFrameProps {
  readonly index: number;
  readonly accounts: Account[];
  readonly account: Account | null;
  readonly setAccount: ((account: Account | null) => void) | null;
  readonly location: string;
  readonly setLocation: ((location: string) => void) | null;
  readonly amount: number | null;
  readonly setAmount: ((amount: number | null) => void) | null;
  readonly accountFilter?: ((account: AccountIdentifier) => boolean) | null;
  readonly onRemove?: (() => void) | null;
}

/**
 * Displays a form frame for an account transaction destination.
 */
const AccountTransactionDestinationFormFrame = function ({
  index,
  accounts,
  account,
  setAccount,
  location,
  setLocation,
  amount,
  setAmount,
  accountFilter = null,
  onRemove = null,
}: AccountTransactionDestinationFrameProps): JSX.Element {
  return (
    <TransactionFrame
      title={`Destination ${index + 1}`}
      description="Capture where this portion of the transfer is going."
      onRemove={onRemove}
    >
      <AccountEntryField
        label="Account"
        options={accounts}
        value={account}
        setValue={
          setAccount === null
            ? null
            : (nextValue): void => {
                setAccount(
                  accounts.find(
                    (candidate) => candidate.id === nextValue?.id,
                  ) ?? null,
                );
              }
        }
        filter={accountFilter}
      />
      <Typography variant="subtitle1">Or</Typography>
      <StringEntryField
        label="Location"
        value={location}
        setValue={account === null ? setLocation : null}
      />
      <CurrencyEntryField label="Amount" value={amount} setValue={setAmount} />
    </TransactionFrame>
  );
};

export default AccountTransactionDestinationFormFrame;
