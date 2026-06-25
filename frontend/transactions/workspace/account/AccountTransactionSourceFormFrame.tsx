import type { Account, AccountIdentifier } from "@/accounts/types";
import AccountEntryField from "@/accounts/AccountEntryField";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { JSX } from "react";
import StringEntryField from "@/framework/forms/StringEntryField";
import TransactionFrame from "@/transactions/workspace/TransactionFrame";
import { Typography } from "@mui/material";

/**
 * Props for the AccountTransactionSourceFormFrame component.
 */
interface AccountTransactionSourceFormFrameProps {
  readonly accounts: Account[];
  readonly account: Account | null;
  readonly setAccount: ((account: Account | null) => void) | null;
  readonly location: string;
  readonly setLocation: ((location: string) => void) | null;
  readonly amount: number | null;
  readonly setAmount: ((amount: number | null) => void) | null;
  readonly accountFilter?: ((account: AccountIdentifier) => boolean) | null;
}

/**
 * Displays a form frame for an account transaction source.
 */
const AccountTransactionSourceFormFrame = function ({
  accounts,
  account,
  setAccount,
  location,
  setLocation,
  accountFilter = null,
  amount,
  setAmount,
}: AccountTransactionSourceFormFrameProps): JSX.Element {
  return (
    <TransactionFrame
      title="Transfer Source"
      description="Choose the source account or provide the source location for this transfer."
    >
      <AccountEntryField
        label="Source Account"
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
        label="Source Location"
        value={location}
        setValue={account === null ? setLocation : null}
      />
      <CurrencyEntryField label="Amount" value={amount} setValue={setAmount} />
    </TransactionFrame>
  );
};

export default AccountTransactionSourceFormFrame;
