import type {
  AccountingPeriod,
  AccountingPeriodFund,
} from "@/accounting-periods/types";
import Breadcrumbs, { type Breadcrumb } from "@/framework/Breadcrumbs";
import { Button, Stack, Typography } from "@mui/material";
import GoalFrame, { type GoalFrameContext } from "@/goals/GoalFrame";
import type { AccountingPeriodFundViewSearchParams } from "@/accounting-periods/funds/AccountingPeriodFundView";
import CaptionedFrame from "@/framework/view/CaptionedFrame";
import CaptionedValue from "@/framework/view/CaptionedValue";
import type { Goal } from "@/goals/types";
import type { JSX } from "react";
import SearchBar from "@/framework/listframe/SearchBar";
import type { Transaction } from "@/transactions/types";
import TransactionListFrame from "@/funds/detail/FundTransactionListFrame";
import formatCurrency from "@/framework/formatCurrency";
import fundRoutes from "@/funds/routes";
import nameof from "@/framework/data/nameof";

/**
 * Props for the AccountingPeriodFundFrame component.
 */
interface AccountingPeriodFundFrameProps {
  readonly breadcrumbs: Breadcrumb[];
  readonly accountingPeriod: AccountingPeriod;
  readonly fund: AccountingPeriodFund;
  readonly goal: Goal | null;
  readonly transactions: Transaction[];
  readonly transactionsTotalCount: number;
  readonly context: GoalFrameContext;
}

/**
 * Component that displays the details of a single fund in the context of an accounting period, including its associated goal and transactions.
 */
const AccountingPeriodFundFrame = function ({
  breadcrumbs,
  accountingPeriod,
  fund,
  goal,
  transactions,
  transactionsTotalCount,
  context,
}: AccountingPeriodFundFrameProps): JSX.Element {
  const isSystemFund = fund.name === "Unassigned";
  return (
    <Stack spacing={2}>
      <Stack
        direction="row"
        justifyContent="space-between"
        alignItems="center"
        maxWidth={1000}
      >
        <Breadcrumbs breadcrumbs={breadcrumbs} />
        <Stack direction="row" spacing={1}>
          <Button
            variant="contained"
            color="primary"
            href={fundRoutes.update(
              { id: fund.id },
              { accountingPeriodId: accountingPeriod.id },
            )}
            disabled={isSystemFund}
          >
            Edit
          </Button>
          <Button
            variant="contained"
            color="error"
            href={fundRoutes.delete(
              { id: fund.id },
              { accountingPeriodId: accountingPeriod.id },
            )}
            disabled={isSystemFund}
          >
            Delete
          </Button>
        </Stack>
      </Stack>
      <CaptionedFrame caption="Details">
        <CaptionedValue caption="Name" value={fund.name} />
        <CaptionedValue caption="Description" value={fund.description} />
      </CaptionedFrame>
      <CaptionedFrame caption="Balance">
        <CaptionedValue
          caption="Opening Balance"
          value={formatCurrency(fund.openingBalance)}
        />
        <CaptionedValue
          caption="Amount Assigned"
          value={
            <span style={{ color: "green" }}>
              + {formatCurrency(fund.amountAssigned)}
            </span>
          }
        />
        <CaptionedValue
          caption="Amount Spent"
          value={
            <span style={{ color: "red" }}>
              - {formatCurrency(fund.amountSpent)}
            </span>
          }
        />
        <CaptionedValue
          caption="Closing Balance"
          value={formatCurrency(fund.closingBalance)}
        />
      </CaptionedFrame>
      <GoalFrame
        goal={goal ?? null}
        accountingPeriodId={accountingPeriod.id}
        isAccountingPeriodOpen={accountingPeriod.isOpen}
        isSystemFund={isSystemFund}
        fundId={fund.id}
        context={context}
      />
      <Stack spacing={2} style={{ maxWidth: 1000 }}>
        <Typography variant="h6">Transactions</Typography>
        <SearchBar
          paramName={nameof<AccountingPeriodFundViewSearchParams>("search")}
        />
        <TransactionListFrame
          fund={fund}
          data={transactions}
          totalCount={transactionsTotalCount}
        />
      </Stack>
    </Stack>
  );
};

export default AccountingPeriodFundFrame;
