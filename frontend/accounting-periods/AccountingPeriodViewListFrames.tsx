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
import FundListFrame from "@/accounting-periods/funds/AccountingPeriodFundListFrame";
import type { Goal } from "@/goals/types";
import GoalListFrame from "@/accounting-periods/goals/AccountingPeriodGoalListFrame";
import type { JSX } from "react";
import SearchBar from "@/framework/listframe/SearchBar";
import type { Transaction } from "@/transactions/types";
import TransactionListFrame from "@/accounting-periods/transactions/AccountingPeriodTransactionListFrame";
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
  readonly fundData: AccountingPeriodFund[];
  readonly fundTotalCount: number;
  readonly goalData: Goal[];
  readonly goalTotalCount: number;
  readonly accountData: AccountingPeriodAccount[];
  readonly accountTotalCount: number;
  readonly transactionData: Transaction[];
  readonly transactionTotalCount: number;
}

/**
 * Component that displays the list frames associated with an accounting period.
 */
const AccountingPeriodListFrames = function ({
  accountingPeriod,
  fundData,
  fundTotalCount,
  goalData,
  goalTotalCount,
  accountData,
  accountTotalCount,
  transactionData,
  transactionTotalCount,
}: AccountingPeriodListFramesProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const currentDisplay =
    tryParseEnum(ToggleState, searchParams.get("display") ?? "") ??
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
            newSearchParams.set("display", parsedValue);
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
            <SearchBar paramName="fundSearch" />
            <FundListFrame
              accountingPeriod={accountingPeriod}
              data={fundData}
              totalCount={fundTotalCount}
            />
          </Stack>
        )}
        {currentDisplay === ToggleState.Goals && (
          <Stack spacing={2} sx={{ width: "100%" }}>
            <Typography variant="h6">Goals</Typography>
            <SearchBar paramName="goalSearch" />
            <GoalListFrame
              accountingPeriod={accountingPeriod}
              data={goalData}
              totalCount={goalTotalCount}
            />
          </Stack>
        )}
        {currentDisplay === ToggleState.Accounts && (
          <Stack spacing={2} sx={{ width: "100%" }}>
            <Typography variant="h6">Accounts</Typography>
            <SearchBar paramName="accountSearch" />
            <AccountListFrame
              accountingPeriod={accountingPeriod}
              data={accountData}
              totalCount={accountTotalCount}
            />
          </Stack>
        )}
        {currentDisplay === ToggleState.Transactions && (
          <Stack spacing={2} sx={{ width: "100%" }}>
            <Typography variant="h6">Transactions</Typography>
            <SearchBar paramName="transactionSearch" />
            <TransactionListFrame
              accountingPeriodId={accountingPeriod.id}
              data={transactionData}
              totalCount={transactionTotalCount}
            />
          </Stack>
        )}
      </Stack>
    </Stack>
  );
};

export default AccountingPeriodListFrames;
