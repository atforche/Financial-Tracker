import { isPositiveChangeInBalance } from "@/data/accountTypes";
import type { FundAmount } from "@/data/fundTypes";
import { Stack, Typography } from "@mui/material";
import type { JSX } from "react";
import type { Transaction, TransactionAccount } from "@/data/transactionTypes";
import Breadcrumbs from "@/framework/Breadcrumbs";
import CaptionedFrame from "@/framework/view/CaptionedFrame";
import CaptionedValue from "@/framework/view/CaptionedValue";
import formatCurrency from "@/framework/formatCurrency";
import { routeBreadcrumbs } from "@/framework/routes";
import getApiClient from "@/data/getApiClient";

interface NamedEntity {
  readonly id: string;
  readonly name: string;
}

interface TransactionFund {
  readonly fundId: string;
  readonly fundName: string;
}

type TransactionWithAccounts = Transaction & {
  readonly debitAccount?: TransactionAccount | null;
  readonly creditAccount?: TransactionAccount | null;
};

type TransactionWithFunds = Transaction & {
  readonly debitFund?: TransactionFund | null;
  readonly creditFund?: TransactionFund | null;
};

interface PageProps {
  readonly params: Promise<{
    id: string;
  }>;
  readonly searchParams: Promise<{
    accountingPeriodId?: string | null;
    accountId?: string | null;
    fundId?: string | null;
  }>;
}

const formatOptionalAmount = function (value: number | null): string {
  return value === null ? "-" : formatCurrency(value);
};

const renderFundAmounts = function (
  fundAmounts: FundAmount[],
): string | JSX.Element {
  if (fundAmounts.length === 0) {
    return "-";
  }

  return (
    <Stack spacing={0.5} alignItems="flex-end">
      {fundAmounts.map((fundAmount) => (
        <Typography variant="subtitle2" key={fundAmount.fundId}>
          {`${fundAmount.fundName}: ${formatCurrency(fundAmount.amount)}`}
        </Typography>
      ))}
    </Stack>
  );
};

const getDebitAccount = function (
  transaction: Transaction,
): TransactionAccount | null {
  const transactionWithAccounts = transaction as TransactionWithAccounts;
  return transactionWithAccounts.debitAccount ?? null;
};

const getCreditAccount = function (
  transaction: Transaction,
): TransactionAccount | null {
  const transactionWithAccounts = transaction as TransactionWithAccounts;
  return transactionWithAccounts.creditAccount ?? null;
};

const getDebitFund = function (transaction: Transaction): TransactionFund | null {
  const transactionWithFunds = transaction as TransactionWithFunds;
  return transactionWithFunds.debitFund ?? null;
};

const getCreditFund = function (
  transaction: Transaction,
): TransactionFund | null {
  const transactionWithFunds = transaction as TransactionWithFunds;
  return transactionWithFunds.creditFund ?? null;
};

const getChangeInPostedBalance = function (
  account: TransactionAccount,
): JSX.Element {
  const changeInPostedBalance =
    account.newAccountBalance.postedBalance -
    account.previousAccountBalance.postedBalance;
  const isPositiveChange = isPositiveChangeInBalance(
    account.accountType,
    changeInPostedBalance,
  );

  return (
    <span style={{ color: isPositiveChange ? "green" : "red" }}>
      {changeInPostedBalance >= 0 ? "+" : "-"}{" "}
      {formatCurrency(Math.abs(changeInPostedBalance))}
    </span>
  );
};

const TransactionAccountFrame = function ({
  caption,
  account,
}: {
  readonly caption: string;
  readonly account: TransactionAccount;
}): JSX.Element {
  const showAvailableToSpend =
    account.previousAccountBalance.availableToSpend !== null ||
    account.newAccountBalance.availableToSpend !== null;

  return (
    <CaptionedFrame caption={caption}>
      <CaptionedValue caption="Name" value={account.accountName} />
      <CaptionedValue caption="Type" value={account.accountType} />
      <CaptionedValue
        caption="Posted Date"
        value={account.postedDate ?? "Unposted"}
      />
      <CaptionedValue
        caption="Fund Allocations"
        value={renderFundAmounts(account.fundAmounts)}
      />
      <br />
      <CaptionedValue
        caption="Previous Posted Balance"
        value={formatCurrency(account.previousAccountBalance.postedBalance)}
      />
      <CaptionedValue
        caption="New Posted Balance"
        value={formatCurrency(account.newAccountBalance.postedBalance)}
      />
      {showAvailableToSpend ? (
        <>
          <CaptionedValue
            caption="Previous Available to Spend"
            value={formatOptionalAmount(
              account.previousAccountBalance.availableToSpend,
            )}
          />
          <CaptionedValue
            caption="New Available to Spend"
            value={formatOptionalAmount(account.newAccountBalance.availableToSpend)}
          />
        </>
      ) : null}
      <CaptionedValue
        caption="Change in Posted Balance"
        value={getChangeInPostedBalance(account)}
      />
    </CaptionedFrame>
  );
};

const getContextAccount = function (
  transaction: Transaction,
  accountId: string,
): NamedEntity | null {
  const matchingAccount = [getDebitAccount(transaction), getCreditAccount(transaction)].find(
    (account) => account !== null && account.accountId === accountId,
  );

  return typeof matchingAccount === "undefined" || matchingAccount === null
    ? null
    : {
        id: matchingAccount.accountId,
        name: matchingAccount.accountName,
      };
};

const getContextFund = function (
  transaction: Transaction,
  fundId: string,
): NamedEntity | null {
  const matchingFund = [getDebitFund(transaction), getCreditFund(transaction)].find(
    (fund) => fund?.fundId === fundId,
  );
  if (typeof matchingFund !== "undefined" && matchingFund !== null) {
    return {
      id: matchingFund.fundId,
      name: matchingFund.fundName,
    };
  }

  const matchingFundAmount = [
    ...(getDebitAccount(transaction)?.fundAmounts ?? []),
    ...(getCreditAccount(transaction)?.fundAmounts ?? []),
  ].find((fundAmount) => fundAmount.fundId === fundId);

  return typeof matchingFundAmount === "undefined"
    ? null
    : {
        id: matchingFundAmount.fundId,
        name: matchingFundAmount.fundName,
      };
};

/**
 * Component that displays the detail view for a single transaction.
 */
const Page = async function ({
  params,
  searchParams,
}: PageProps): Promise<JSX.Element> {
  const { id } = await params;
  const { accountingPeriodId, accountId, fundId } = await searchParams;

  const apiClient = getApiClient();
  const { data: transactionData, error: transactionError } = await apiClient.GET(
    "/transactions/{transactionId}",
    {
      params: {
        path: {
          transactionId: id,
        },
      },
    },
  );

  if (typeof transactionData === "undefined") {
    throw new Error(
      `Failed to fetch transaction with ID ${id}: ${transactionError.detail}`,
    );
  }

  const debitAccount = getDebitAccount(transactionData);
  const creditAccount = getCreditAccount(transactionData);
  const debitFund = getDebitFund(transactionData);
  const creditFund = getCreditFund(transactionData);

  const providedAccountingPeriod =
    typeof accountingPeriodId === "string" && accountingPeriodId !== ""
      ? {
          id: accountingPeriodId,
          name: transactionData.accountingPeriodName,
        }
      : null;
  const providedAccount =
    typeof accountId === "string" && accountId !== ""
      ? getContextAccount(transactionData, accountId)
      : null;
  const providedFund =
    typeof fundId === "string" && fundId !== ""
      ? getContextFund(transactionData, fundId)
      : null;

  const breadcrumbContext =
    providedAccountingPeriod !== null && providedFund !== null
      ? {
          accountingPeriod: providedAccountingPeriod,
          fund: providedFund,
        }
      : providedAccountingPeriod !== null && providedAccount !== null
        ? {
            accountingPeriod: providedAccountingPeriod,
            account: providedAccount,
          }
        : providedAccount !== null
          ? {
              account: providedAccount,
            }
          : providedFund !== null
            ? {
                fund: providedFund,
              }
            : providedAccountingPeriod !== null
              ? {
                  accountingPeriod: providedAccountingPeriod,
                }
              : {
                  accountingPeriod: {
                    id: transactionData.accountingPeriodId,
                    name: transactionData.accountingPeriodName,
                  },
                };

  return (
    <Stack spacing={2}>
      <Breadcrumbs
        breadcrumbs={routeBreadcrumbs.transactions.detail(
          transactionData.id,
          breadcrumbContext,
        )}
      />
      <CaptionedFrame caption="Details">
        <CaptionedValue
          caption="Accounting Period"
          value={transactionData.accountingPeriodName}
        />
        <CaptionedValue
          caption="Type"
          value={transactionData.type ?? "Transaction"}
        />
        <CaptionedValue caption="Date" value={transactionData.date} />
        <CaptionedValue caption="Location" value={transactionData.location} />
        <CaptionedValue
          caption="Description"
          value={transactionData.description === "" ? "-" : transactionData.description}
        />
        <CaptionedValue
          caption="Amount"
          value={formatCurrency(transactionData.amount)}
        />
      </CaptionedFrame>
      {debitFund !== null && creditFund !== null ? (
        <CaptionedFrame caption="Fund Transfer">
          <CaptionedValue caption="Debit Fund" value={debitFund.fundName} />
          <CaptionedValue caption="Credit Fund" value={creditFund.fundName} />
          <CaptionedValue
            caption="Amount"
            value={formatCurrency(transactionData.amount)}
          />
        </CaptionedFrame>
      ) : null}
      <Stack direction="row" spacing={2} flexWrap="wrap" useFlexGap>
        {debitAccount !== null ? (
          <TransactionAccountFrame caption="Debit Account" account={debitAccount} />
        ) : null}
        {creditAccount !== null ? (
          <TransactionAccountFrame
            caption="Credit Account"
            account={creditAccount}
          />
        ) : null}
      </Stack>
    </Stack>
  );
};

export default Page;