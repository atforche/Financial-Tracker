"use client";

import {
  type AccountTransaction,
  asAccountTransaction,
} from "@/transactions/accountTransaction";
import { Box, Button, Stack, Typography } from "@mui/material";
import {
  type FundTransaction,
  asFundTransaction,
} from "@/transactions/fundTransaction";
import {
  type IncomeTransaction,
  asIncomeTransaction,
} from "@/transactions/incomeTransaction";
import type { JSX, ReactNode } from "react";
import {
  type SpendingTransaction,
  asSpendingTransaction,
} from "@/transactions/spendingTransaction";
import {
  type Transaction,
  type TransactionAccount,
  TransactionType,
} from "@/transactions/transaction";
import {
  getPostableTransactionAccounts,
  getPostedTransactionAccounts,
} from "@/transactions/postingHelpers";
import AccountTransactionDestinationViewFrame from "@/transactions/workspace/account/AccountTransactionDestinationViewFrame";
import AccountTransactionSourceViewFrame from "@/transactions/workspace/account/AccountTransactionSourceViewFrame";
import type { AccountingPeriod } from "@/accounting-periods/types";
import DeleteTransactionForm from "@/transactions/workspace/DeleteTransactionForm";
import EastOutlined from "@mui/icons-material/EastOutlined";
import type { Fund } from "@/funds/types";
import FundTransactionDestinationViewFrame from "@/transactions/workspace/fund/FundTransactionDestinationViewFrame";
import FundTransactionSourceViewFrame from "@/transactions/workspace/fund/FundTransactionSourceViewFrame";
import IncomeTransactionDestinationViewFrame from "@/transactions/workspace/income/IncomeTransactionDestinationViewFrame";
import IncomeTransactionSourceViewFrame from "@/transactions/workspace/income/IncomeTransactionSourceViewFrame";
import Link from "next/link";
import SouthOutlined from "@mui/icons-material/SouthOutlined";
import SpendingTransactionDestinationViewFrame from "@/transactions/workspace/spending/SpendingTransactionDestinationViewFrame";
import SpendingTransactionSourceViewFrame from "@/transactions/workspace/spending/SpendingTransactionSourceViewFrame";
import TransactionAccountPostAction from "@/transactions/workspace/TransactionAccountPostAction";
import TransactionDetailsViewSection from "@/transactions/workspace/TransactionDetailsViewSection";
import TransactionSection from "@/transactions/workspace/TransactionSection";
import UnpostTransactionForm from "@/transactions/workspace/UnpostTransactionForm";
import dayjs from "dayjs";

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

interface TransactionFlowSectionProps {
  readonly title: string;
  readonly description: string;
  readonly sourceFrame: JSX.Element;
  readonly destinationFrames: JSX.Element[];
}

const TransactionFlowSection = function ({
  title,
  description,
  sourceFrame,
  destinationFrames,
}: TransactionFlowSectionProps): JSX.Element {
  return (
    <TransactionSection title={title} description={description}>
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
    </TransactionSection>
  );
};

const renderIncomeView = function (
  transaction: IncomeTransaction,
  funds: Fund[],
): JSX.Element {
  return (
    <TransactionFlowSection
      title="Income Flow"
      description="Review how this income source flows into its destination accounts and fund assignments."
      sourceFrame={
        <IncomeTransactionSourceViewFrame
          account={transaction.source.account ?? null}
          location={transaction.source.location ?? null}
          incomeLines={transaction.source.incomeLines}
          incomeDeductions={transaction.source.incomeDeductions}
        />
      }
      destinationFrames={transaction.destinations.map((destination, index) => (
        <IncomeTransactionDestinationViewFrame
          key={`income-destination-${index}`}
          index={index}
          funds={funds}
          account={destination.account}
          amount={destination.amount}
          fundAssignments={destination.fundAssignments}
        />
      ))}
    />
  );
};

const renderSpendingView = function (
  transaction: SpendingTransaction,
  funds: Fund[],
  getAccountHelperContent: (account: TransactionAccount) => ReactNode,
): JSX.Element {
  return (
    <TransactionFlowSection
      title="Spending Flow"
      description="Review how money moves from the source account into each destination."
      sourceFrame={
        <SpendingTransactionSourceViewFrame
          account={transaction.source.account}
          helperContent={getAccountHelperContent(transaction.source.account)}
        />
      }
      destinationFrames={transaction.destinations.map((destination, index) => (
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
      ))}
    />
  );
};

const renderAccountView = function (
  transaction: AccountTransaction,
): JSX.Element {
  return (
    <TransactionFlowSection
      title="Transfer Flow"
      description="Review the source and destination for this account transfer."
      sourceFrame={
        <AccountTransactionSourceViewFrame
          account={transaction.source.account ?? null}
          location={transaction.source.location ?? ""}
          amount={transaction.amount}
        />
      }
      destinationFrames={transaction.destinations.map((destination, index) => (
        <AccountTransactionDestinationViewFrame
          key={`account-destination-${index}`}
          index={index}
          account={destination.account ?? null}
          location={destination.location ?? ""}
          amount={destination.amount}
        />
      ))}
    />
  );
};

const renderFundView = function (transaction: FundTransaction): JSX.Element {
  return (
    <TransactionFlowSection
      title="Transfer Flow"
      description="Review the source fund and destination fund for this transfer."
      sourceFrame={
        <FundTransactionSourceViewFrame
          fund={transaction.source.fund}
          amount={transaction.amount}
        />
      }
      destinationFrames={transaction.destinations.map((destination, index) => (
        <FundTransactionDestinationViewFrame
          key={`fund-destination-${index}`}
          index={index}
          fund={destination.fund}
          amount={destination.fund.newFundBalance.postedBalance}
        />
      ))}
    />
  );
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
        ? renderIncomeView(incomeTransaction, funds)
        : null}

      {transaction.transactionType === TransactionType.Account &&
      accountTransaction !== null
        ? renderAccountView(accountTransaction)
        : null}

      {transaction.transactionType === TransactionType.Fund &&
      fundTransaction !== null
        ? renderFundView(fundTransaction)
        : null}
    </Stack>
  );
};

export default ViewTransactionForm;
