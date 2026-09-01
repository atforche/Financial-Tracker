import type {
  Account,
  AccountBalanceEventDraft,
  AccountWithBalance,
} from "@/accounts/types";
import type { Location, LocationDraft } from "@/locations/types";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { FrameColor } from "@/framework/view/Frame";
import type { JSX } from "react";
import type { Transaction } from "@/transactions/types";
import TransactionAccountOrLocationFrame from "@/transactions/workspace/TransactionAccountOrLocationFrame";
import TransactionSourceOrDestinationFrame from "@/transactions/workspace/TransactionSourceOrDestinationFrame";

/**
 * Props for the AccountTransactionSourceFrame component.
 */
interface AccountTransactionSourceFrameProps {
  readonly accounts: AccountWithBalance[];
  readonly transaction?: Transaction | null;
  readonly account: AccountBalanceEventDraft | null;
  readonly setAccount:
    ((account: AccountBalanceEventDraft | null) => void) | null;
  readonly locations?: readonly Location[] | undefined;
  readonly location: LocationDraft | null;
  readonly setLocation: ((location: LocationDraft | null) => void) | null;
  readonly amount: number | null;
  readonly setAmount: ((amount: number | null) => void) | null;
  readonly accountFilter?: ((account: Account) => boolean) | null;
  readonly color?: FrameColor;
  readonly readOnly?: boolean;
}

/**
 * Displays a form frame for an account transaction source.
 */
const AccountTransactionSourceFrame = function ({
  accounts,
  transaction = null,
  account,
  setAccount,
  location,
  locations,
  setLocation,
  accountFilter = null,
  amount,
  setAmount,
  color = "info",
  readOnly = false,
}: AccountTransactionSourceFrameProps): JSX.Element {
  return (
    <TransactionSourceOrDestinationFrame title="Source" color={color}>
      <TransactionAccountOrLocationFrame
        accounts={accounts}
        transaction={transaction}
        account={account}
        setAccount={readOnly ? null : setAccount}
        accountCaption="Source Account"
        locationCaption="Source Location"
        entryCaption="Source"
        locations={locations}
        location={location}
        setLocation={readOnly ? null : setLocation}
        accountFilter={accountFilter}
        balanceChange={amount === null ? null : -amount}
        readOnly={readOnly}
      />
      <CurrencyEntryField
        label="Amount"
        value={amount}
        setValue={readOnly ? null : setAmount}
      />
    </TransactionSourceOrDestinationFrame>
  );
};

export default AccountTransactionSourceFrame;
