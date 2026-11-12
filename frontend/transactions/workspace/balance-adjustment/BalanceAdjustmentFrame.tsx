"use client";

import type {
  AccountBalanceEvent,
  AccountBalanceEventDraft,
  AccountWithBalance,
} from "@/accounts/types";
import {
  formatCurrency,
  formatSignedCurrency,
} from "@/framework/currencyHelpers";
import AccountBalanceEventFrame from "@/transactions/workspace/AccountBalanceEventFrame";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { JSX } from "react";
import ReadOnlyField from "@/framework/forms/ReadOnlyField";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import TransactionSection from "@/transactions/workspace/TransactionSection";
import { Typography } from "@mui/material";
import dayjs from "dayjs";
import { isAdjustableAccount } from "@/transactions/workspace/balance-adjustment/helpers";

/**
 * Props for the BalanceAdjustmentFrame component.
 */
interface BalanceAdjustmentFrameProps {
  readonly accounts?: AccountWithBalance[];
  readonly account: AccountBalanceEventDraft | null;
  readonly setAccount?:
    ((account: AccountBalanceEventDraft | null) => void) | null;
  readonly balanceEvent: AccountBalanceEvent | null;
  readonly institutionBalance?: number | null;
  readonly setInstitutionBalance?: ((balance: number | null) => void) | null;
  readonly date?: string | null;
  readonly postedBalance?: number | null;
  readonly postedBalanceFailed?: boolean;
  readonly change: number | null;
  readonly incomplete?: boolean;
}

const emptyAccounts: AccountWithBalance[] = [];

/**
 * Displays a balance adjustment's account and change. While editing, the change is worked out from the balance at the
 * institution and the account's posted balance on the transaction date.
 */
const BalanceAdjustmentFrame = function ({
  accounts = emptyAccounts,
  account,
  setAccount = null,
  balanceEvent,
  institutionBalance = null,
  setInstitutionBalance = null,
  date = null,
  postedBalance = null,
  postedBalanceFailed = false,
  change,
  incomplete = false,
}: BalanceAdjustmentFrameProps): JSX.Element {
  const isEditing = setAccount !== null;
  return (
    <TransactionSection title="Balance" incomplete={incomplete}>
      <AccountBalanceEventFrame
        accounts={accounts}
        account={account}
        setAccount={setAccount}
        accountFilter={isAdjustableAccount}
        balanceEvent={balanceEvent}
      />
      {isEditing ? (
        <CurrencyEntryField
          label="Balance at institution"
          value={institutionBalance}
          setValue={setInstitutionBalance}
        />
      ) : null}
      <ResponsiveGrid minimumColumnWidth={180} spacing={2}>
        {isEditing ? (
          <ReadOnlyField
            label={
              date === null
                ? "Posted balance"
                : `Posted balance on ${dayjs(date).format("MMM D, YYYY")}`
            }
            value={
              postedBalance === null ? null : formatCurrency(postedBalance)
            }
            emptyValue="—"
          />
        ) : null}
        <ReadOnlyField
          label="Change"
          value={change === null ? null : formatSignedCurrency(change)}
          emptyValue="—"
        />
      </ResponsiveGrid>
      {isEditing && postedBalanceFailed ? (
        <Typography variant="body2" color="error">
          Couldn&apos;t load the posted balance for this account and date.
        </Typography>
      ) : null}
    </TransactionSection>
  );
};

export default BalanceAdjustmentFrame;
