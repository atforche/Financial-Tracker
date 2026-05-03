"use client";

import type {
  AccountingPeriod,
  AccountingPeriodAccount,
  AccountingPeriodFund,
} from "@/accounting-periods/types";
import {
  Stack,
  ToggleButton,
  ToggleButtonGroup,
  Typography,
} from "@mui/material";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import AccountListFrame from "@/accounting-periods/accounts/AccountingPeriodAccountListFrame";
import type { AccountingPeriodViewSearchParams } from "@/accounting-periods/AccountingPeriodView";
import FundListFrame from "@/accounting-periods/funds/AccountingPeriodFundListFrame";
import type { Goal } from "@/goals/types";
import GoalListFrame from "@/accounting-periods/goals/AccountingPeriodGoalListFrame";
import type { JSX } from "react";
import SearchBar from "@/framework/listframe/SearchBar";
import type { Transaction } from "@/transactions/types";
import TransactionListFrame from "@/accounting-periods/transactions/AccountingPeriodTransactionListFrame";
import nameof from "@/framework/data/nameof";
import tryParseEnum from "@/framework/data/tryParseEnum";

/**
 * Defines the possible toggle states for the accounting period list frames.
 */
enum ToggleState {
  Funds = "funds",
  Goals = "goals",
  Accounts = "accounts",
  Transactions = "transactions",
}

/**
 * Props for the AccountingPeriodViewListFrames component.
 */
interface AccountingPeriodListFramesProps {
  readonly accountingPeriod: AccountingPeriod;
  readonly funds: AccountingPeriodFund[];
  readonly fundTotalCount: number;
  readonly goals: Goal[];
  readonly goalTotalCount: number;
  readonly accounts: AccountingPeriodAccount[];
  readonly accountTotalCount: number;
  readonly transactions: Transaction[];
  readonly transactionTotalCount: number;
}

/**
 * Component that displays the list frames associated with an accounting period.
 */
const AccountingPeriodListFrames = function ({
  accountingPeriod,
  funds,
  fundTotalCount,
  goals,
  goalTotalCount,
  accounts,
  accountTotalCount,
  transactions,
  transactionTotalCount,
}: AccountingPeriodListFramesProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();
  const displaySearchParamName =
    nameof<AccountingPeriodViewSearchParams>("display");

  const currentDisplay =
    tryParseEnum(ToggleState, searchParams.get(displaySearchParamName) ?? "") ??
    ToggleState.Funds;
  return (
    <Stack spacing={2}>
      <ToggleButtonGroup
        value={currentDisplay}
        exclusive
        onChange={(_, value: unknown): void => {
          if (typeof value !== "string") {
            return;
          }
          const parsedValue = tryParseEnum(ToggleState, value);
          if (parsedValue !== null) {
            const newSearchParams = new URLSearchParams();
            newSearchParams.set(displaySearchParamName, parsedValue);
            router.replace(`${pathname}?${newSearchParams.toString()}`);
          }
        }}
      >
        <ToggleButton value={ToggleState.Funds}>Funds</ToggleButton>
        <ToggleButton value={ToggleState.Goals}>Goals</ToggleButton>
        <ToggleButton value={ToggleState.Accounts}>Accounts</ToggleButton>
        <ToggleButton value={ToggleState.Transactions}>
          Transactions
        </ToggleButton>
      </ToggleButtonGroup>
      <Stack direction="row" spacing={10}>
        {currentDisplay === ToggleState.Funds && (
          <Stack spacing={2} sx={{ width: "100%" }}>
            <Typography variant="h6">Funds</Typography>
            <SearchBar
              paramName={nameof<AccountingPeriodViewSearchParams>("search")}
            />
            <FundListFrame
              accountingPeriod={accountingPeriod}
              data={funds}
              totalCount={fundTotalCount}
            />
          </Stack>
        )}
        {currentDisplay === ToggleState.Goals && (
          <Stack spacing={2} sx={{ width: "100%" }}>
            <Typography variant="h6">Goals</Typography>
            <SearchBar
              paramName={nameof<AccountingPeriodViewSearchParams>("search")}
            />
            <GoalListFrame
              accountingPeriod={accountingPeriod}
              data={goals}
              totalCount={goalTotalCount}
            />
          </Stack>
        )}
        {currentDisplay === ToggleState.Accounts && (
          <Stack spacing={2} sx={{ width: "100%" }}>
            <Typography variant="h6">Accounts</Typography>
            <SearchBar
              paramName={nameof<AccountingPeriodViewSearchParams>("search")}
            />
            <AccountListFrame
              accountingPeriod={accountingPeriod}
              data={accounts}
              totalCount={accountTotalCount}
            />
          </Stack>
        )}
        {currentDisplay === ToggleState.Transactions && (
          <Stack spacing={2} sx={{ width: "100%" }}>
            <Typography variant="h6">Transactions</Typography>
            <SearchBar
              paramName={nameof<AccountingPeriodViewSearchParams>("search")}
            />
            <TransactionListFrame
              accountingPeriodId={accountingPeriod.id}
              data={transactions}
              totalCount={transactionTotalCount}
            />
          </Stack>
        )}
      </Stack>
    </Stack>
  );
};

export default AccountingPeriodListFrames;
export { ToggleState };
