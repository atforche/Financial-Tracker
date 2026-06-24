"use client";

import {
  type AccountTransaction,
  type FundTransaction,
  type IncomeTransaction,
  type SpendingTransaction,
  type Transaction,
  type TransactionAccount,
  TransactionType,
  asAccountTransaction,
  asFundTransaction,
  asIncomeTransaction,
  asSpendingTransaction,
  getPostableTransactionAccounts,
  getPostedTransactionAccounts,
} from "@/transactions/transaction";
import { Box, Button, Stack, Typography } from "@mui/material";
import type { JSX, ReactNode } from "react";
import type { AccountingPeriod } from "@/accounting-periods/types";
import DeleteTransactionForm from "@/transactions/workspace/DeleteTransactionForm";
import EastOutlined from "@mui/icons-material/EastOutlined";
import type { Fund } from "@/funds/types";
import Link from "next/link";
import SouthOutlined from "@mui/icons-material/SouthOutlined";
import SpendingTransactionDestinationViewFrame from "@/transactions/workspace/spending/SpendingTransactionDestinationViewFrame";
import SpendingTransactionSourceViewFrame from "@/transactions/workspace/spending/SpendingTransactionSourceViewFrame";
import TransactionAccountPathViewSection from "@/transactions/workspace/TransactionAccountPathViewSection";
import TransactionAccountPostAction from "@/transactions/workspace/TransactionAccountPostAction";
import TransactionDetailsViewSection from "@/transactions/workspace/TransactionDetailsViewSection";
import TransactionDisplayField from "@/transactions/workspace/TransactionDisplayField";
import TransactionFundAssignmentsViewSection from "@/transactions/workspace/TransactionFundAssignmentsViewSection";
import TransactionFundPathViewSection from "@/transactions/workspace/TransactionFundPathViewSection";
import TransactionSection from "@/transactions/workspace/TransactionSection";
import UnpostTransactionForm from "@/transactions/workspace/UnpostTransactionForm";
import dayjs from "dayjs";
import formatCurrency from "@/framework/formatCurrency";

interface ViewTransactionFormProps {
  readonly transaction: Transaction;
  readonly transactionAccountingPeriod: AccountingPeriod;
  readonly funds: Fund[];
  readonly currentUrl: string;
  readonly workspaceUrl: string;
  readonly editUrl: string;
}

interface AccountHelperContentContext {
  readonly transaction: Transaction;
  readonly currentUrl: string;
}

const createAccountHelperContentGetter = function ({
  transaction,
  currentUrl,
}: AccountHelperContentContext): (account: TransactionAccount) => ReactNode {
  const postableAccountIds = new Set(
    getPostableTransactionAccounts(transaction).map(
      (account) => account.accountId,
    ),
  );
  const postedAccountsById = new Map(
    getPostedTransactionAccounts(transaction).map((account) => [
      account.accountId,
      account.postedDate,
    ]),
  );
  const renderedPostActions = new Set<string>();

  return function accountHelperContent(account: TransactionAccount): ReactNode {
    const postedDate = postedAccountsById.get(account.accountId) ?? null;
    if (postedDate !== null) {
      return (
        <Typography variant="caption" color="text.secondary" sx={{ px: 1.75 }}>
          Posted on {dayjs(postedDate).format("MMMM D, YYYY")}
        </Typography>
      );
    }
    if (
      !postableAccountIds.has(account.accountId) ||
      renderedPostActions.has(account.accountId)
    ) {
      return null;
    }
    renderedPostActions.add(account.accountId);
    return (
      <TransactionAccountPostAction
        transactionId={transaction.id}
        accountId={account.accountId}
        defaultDate={transaction.date}
        redirectUrl={currentUrl}
      />
    );
  };
};

const renderIncomeSourceView = function (
  transaction: IncomeTransaction,
): JSX.Element {
  const grossAmount = transaction.source.incomeLines.reduce(
    (total, line) => total + line.amount,
    0,
  );
  const deductionAmount = transaction.source.incomeDeductions.reduce(
    (total, deduction) => total + deduction.amount,
    0,
  );

  return (
    <TransactionSection
      title="Income Breakdown"
      description="Review the gross income lines and deductions captured for this source."
    >
      <Stack
        sx={{
          display: "grid",
          gap: 2,
          gridTemplateColumns:
            "repeat(auto-fit, minmax(min(100%, 220px), 1fr))",
        }}
      >
        <TransactionDisplayField
          label="Gross Income"
          value={formatCurrency(grossAmount)}
        />
        <TransactionDisplayField
          label="Deductions"
          value={formatCurrency(deductionAmount)}
        />
        <TransactionDisplayField
          label="Net Income"
          value={formatCurrency(transaction.amount)}
        />
      </Stack>
      <Stack spacing={2}>
        {transaction.source.incomeLines.map((line, index) => (
          <Stack
            key={`income-line-${index}`}
            sx={{
              display: "grid",
              gap: 2,
              gridTemplateColumns: {
                xs: "1fr",
                md: "minmax(0, 1.8fr) minmax(180px, 1fr)",
              },
            }}
          >
            <TransactionDisplayField
              label={`Income Line ${index + 1}`}
              value={line.description}
            />
            <TransactionDisplayField
              label="Amount"
              value={formatCurrency(line.amount)}
            />
          </Stack>
        ))}
        {transaction.source.incomeDeductions.map((deduction, index) => (
          <Stack
            key={`income-deduction-${index}`}
            sx={{
              display: "grid",
              gap: 2,
              gridTemplateColumns: {
                xs: "1fr",
                md: "minmax(0, 1.8fr) minmax(180px, 1fr)",
              },
            }}
          >
            <TransactionDisplayField
              label={`Deduction ${index + 1}`}
              value={deduction.description}
            />
            <TransactionDisplayField
              label="Amount"
              value={formatCurrency(deduction.amount)}
            />
          </Stack>
        ))}
      </Stack>
    </TransactionSection>
  );
};

const renderIncomeView = function (
  transaction: IncomeTransaction,
  funds: Fund[],
  getAccountHelperContent: (account: TransactionAccount) => ReactNode,
): JSX.Element[] {
  return [
    renderIncomeSourceView(transaction),
    ...transaction.destinations.flatMap((destination, index) => {
      const title =
        transaction.destinations.length === 1
          ? "Money Flow"
          : `Money Flow ${index + 1}`;
      const destinationSection = (
        <TransactionAccountPathViewSection
          key={`income-path-${index}`}
          title={title}
          description="Review which tracked account receives this portion of the income and where the money originated."
          leftLabel="Source Account"
          rightLabel="Deposit To"
          leftAccount={transaction.source.account ?? null}
          rightAccount={destination.account}
          leftLocationLabel={
            transaction.source.account === null ? "Source Location" : null
          }
          leftLocationValue={transaction.source.location ?? null}
          getAccountHelperContent={getAccountHelperContent}
        />
      );
      const fundSection = (
        <TransactionFundAssignmentsViewSection
          key={`income-funds-${index}`}
          funds={funds}
          amount={destination.amount}
          fundAssignments={destination.fundAssignments}
          tone="income"
        />
      );
      return [destinationSection, fundSection];
    }),
  ];
};

const renderSpendingView = function (
  transaction: SpendingTransaction,
  funds: Fund[],
  getAccountHelperContent: (account: TransactionAccount) => ReactNode,
): JSX.Element[] {
  const sourceFrame = (
    <SpendingTransactionSourceViewFrame
      account={transaction.source.account}
      helperContent={getAccountHelperContent(transaction.source.account)}
    />
  );
  const destinationFrames = transaction.destinations.map(
    (destination, index) => (
      <SpendingTransactionDestinationViewFrame
        key={`spending-destination-${index}`}
        index={index}
        funds={funds}
        account={destination.account ?? null}
        location={destination.location ?? null}
        amount={destination.amount}
        fundAssignments={destination.fundAssignments}
        helperContent={
          destination.account === null ||
          typeof destination.account === "undefined"
            ? null
            : getAccountHelperContent(destination.account)
        }
      />
    ),
  );

  return [
    <TransactionSection
      key="spending-flow"
      title="Money Flow"
      description="Review how money moves from the source account into each destination."
    >
      <Box
        sx={{
          display: "grid",
          gap: 2,
          alignItems: "start",
          gridTemplateColumns: {
            xs: "1fr",
            lg: "minmax(280px, 0.95fr) auto minmax(320px, 1.15fr)",
          },
        }}
      >
        {sourceFrame}
        <Box
          sx={{
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            minHeight: { lg: 88 },
            color: "text.secondary",
          }}
        >
          <EastOutlined
            sx={{
              display: { xs: "none", lg: "block" },
              fontSize: 40,
            }}
          />
          <SouthOutlined
            sx={{
              display: { xs: "block", lg: "none" },
              fontSize: 32,
            }}
          />
        </Box>
        <Stack spacing={2}>{destinationFrames}</Stack>
      </Box>
    </TransactionSection>,
  ];
};

const renderAccountView = function (
  transaction: AccountTransaction,
  getAccountHelperContent: (account: TransactionAccount) => ReactNode,
): JSX.Element[] {
  return transaction.destinations.map((destination, index) => (
    <TransactionAccountPathViewSection
      key={`account-path-${index}`}
      title={
        transaction.destinations.length === 1
          ? "Transfer Path"
          : `Transfer Path ${index + 1}`
      }
      description="Review the source and destination for this account transfer."
      leftLabel="Source"
      rightLabel="Destination"
      leftAccount={transaction.source.account ?? null}
      rightAccount={destination.account ?? null}
      leftLocationLabel={
        transaction.source.account === null ? "Source Location" : null
      }
      leftLocationValue={transaction.source.location ?? null}
      rightLocationLabel={
        destination.account === null ? "Destination Location" : null
      }
      rightLocationValue={destination.location ?? null}
      getAccountHelperContent={getAccountHelperContent}
    />
  ));
};

const renderFundView = function (transaction: FundTransaction): JSX.Element[] {
  return transaction.destinations.map((destination, index) => (
    <TransactionFundPathViewSection
      key={`fund-path-${index}`}
      title={
        transaction.destinations.length === 1
          ? "Transfer Path"
          : `Transfer Path ${index + 1}`
      }
      description="Review the source fund and destination fund for this transfer."
      leftLabel="Source Fund"
      rightLabel="Destination Fund"
      leftFund={transaction.source.fund}
      rightFund={destination.fund}
    />
  ));
};

/**
 * Displays a read-only transaction detail view for the selected transaction.
 */
const ViewTransactionForm = function ({
  transaction,
  transactionAccountingPeriod,
  funds,
  currentUrl,
  workspaceUrl,
  editUrl,
}: ViewTransactionFormProps): JSX.Element {
  const spendingTransaction = asSpendingTransaction(transaction);
  const incomeTransaction = asIncomeTransaction(transaction);
  const accountTransaction = asAccountTransaction(transaction);
  const fundTransaction = asFundTransaction(transaction);
  const postedAccountCount = getPostedTransactionAccounts(transaction).length;
  const getAccountHelperContent = createAccountHelperContentGetter({
    transaction,
    currentUrl,
  });

  return (
    <Stack spacing={3} sx={{ width: "100%" }}>
      <TransactionDetailsViewSection
        accountingPeriod={transactionAccountingPeriod}
        date={transaction.date}
        description={transaction.description}
        amount={transaction.amount}
        headerAction={
          <Stack direction="row" spacing={1.25} flexWrap="wrap" useFlexGap>
            <Button component={Link} href={editUrl} variant="contained">
              Edit
            </Button>
            {postedAccountCount > 0 ? (
              <UnpostTransactionForm
                transaction={transaction}
                redirectUrl={currentUrl}
              />
            ) : null}
            <DeleteTransactionForm
              transaction={transaction}
              redirectUrl={workspaceUrl}
            />
          </Stack>
        }
      />

      {transaction.transactionType === TransactionType.Spending &&
      spendingTransaction !== null
        ? renderSpendingView(
            spendingTransaction,
            funds,
            getAccountHelperContent,
          )
        : null}

      {transaction.transactionType === TransactionType.Income &&
      incomeTransaction !== null
        ? renderIncomeView(incomeTransaction, funds, getAccountHelperContent)
        : null}

      {transaction.transactionType === TransactionType.Account &&
      accountTransaction !== null
        ? renderAccountView(accountTransaction, getAccountHelperContent)
        : null}

      {transaction.transactionType === TransactionType.Fund &&
      fundTransaction !== null
        ? renderFundView(fundTransaction)
        : null}
    </Stack>
  );
};

export default ViewTransactionForm;
