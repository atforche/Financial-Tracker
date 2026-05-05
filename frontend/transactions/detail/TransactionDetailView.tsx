import { Button, Stack } from "@mui/material";
import Breadcrumbs from "@/framework/Breadcrumbs";
import CaptionedFrame from "@/framework/view/CaptionedFrame";
import CaptionedValue from "@/framework/view/CaptionedValue";
import type { JSX } from "react";
import breadcrumbs from "@/transactions/breadcrumbs";
import formatCurrency from "@/framework/formatCurrency";
import getApiClient from "@/framework/data/getApiClient";
import { getPostableTransactionAccounts } from "@/transactions/post/PostTransactionForm";
import { getPostedTransactionAccounts } from "@/transactions/unpost/UnpostTransactionForm";
import routes from "@/transactions/routes";

/**
 * Parameters for the TransactionDetailView component.
 */
interface TransactionDetailViewParams {
  readonly id: string;
}

/**
 * Search parameters for the TransactionDetailView component.
 */
interface TransactionDetailViewSearchParams {
  readonly accountingPeriodId?: string | null;
  readonly accountId?: string | null;
  readonly fundId?: string | null;
}

/**
 * Props for the TransactionDetailView component.
 */
interface TransactionDetailViewProps {
  readonly params: Promise<TransactionDetailViewParams>;
  readonly searchParams: Promise<TransactionDetailViewSearchParams>;
}

/**
 * Component that displays the transaction detail view.
 */
const TransactionDetailView = async function ({
  params,
  searchParams,
}: TransactionDetailViewProps): Promise<JSX.Element> {
  const { id } = await params;
  const { accountingPeriodId, accountId, fundId } = await searchParams;

  const apiClient = getApiClient();
  const transactionPromise = apiClient.GET("/transactions/{transactionId}", {
    params: {
      path: {
        transactionId: id,
      },
    },
  });
  const accountingPeriodPromise =
    accountingPeriodId !== null && typeof accountingPeriodId !== "undefined"
      ? apiClient.GET("/accounting-periods/{accountingPeriodId}", {
          params: {
            path: {
              accountingPeriodId,
            },
          },
        })
      : Promise.resolve({ data: null });
  const accountPromise =
    accountId !== null && typeof accountId !== "undefined"
      ? apiClient.GET("/accounts/{accountId}", {
          params: {
            path: {
              accountId,
            },
          },
        })
      : Promise.resolve({ data: null });
  const fundPromise =
    fundId !== null && typeof fundId !== "undefined"
      ? apiClient.GET("/funds/{fundId}", {
          params: {
            path: {
              fundId,
            },
          },
        })
      : Promise.resolve({ data: null });
  const [
    { data: transaction },
    { data: accountingPeriod },
    { data: account },
    { data: fund },
  ] = await Promise.all([
    transactionPromise,
    accountingPeriodPromise,
    accountPromise,
    fundPromise,
  ]);
  if (typeof transaction === "undefined") {
    throw new Error(`Failed to fetch transaction with ID ${id}}`);
  }

  const postableAccounts = getPostableTransactionAccounts(transaction);
  const postedAccounts = getPostedTransactionAccounts(transaction);
  return (
    <Stack spacing={2}>
      <Stack
        direction="row"
        justifyContent="space-between"
        alignItems="center"
        maxWidth={1000}
      >
        <Breadcrumbs
          breadcrumbs={breadcrumbs.detail(
            transaction,
            accountingPeriod ?? null,
            account ?? null,
            fund ?? null,
          )}
        />
        <Stack direction="row" spacing={1}>
          <Button
            variant="contained"
            color="primary"
            href={routes.update(
              { id },
              {
                accountingPeriodId: accountingPeriodId ?? null,
                accountId: accountId ?? null,
                fundId: fundId ?? null,
              },
            )}
          >
            Edit
          </Button>
          {postableAccounts.length > 0 ? (
            <Button
              variant="contained"
              color="primary"
              href={routes.post(
                { id },
                {
                  accountingPeriodId: accountingPeriodId ?? null,
                  accountId: accountId ?? null,
                },
              )}
            >
              Post
            </Button>
          ) : null}
          {postedAccounts.length > 0 ? (
            <Button
              variant="contained"
              color="warning"
              href={routes.unpost(
                { id },
                {
                  accountingPeriodId: accountingPeriodId ?? null,
                  accountId: accountId ?? null,
                },
              )}
            >
              Unpost
            </Button>
          ) : null}
          <Button
            variant="contained"
            color="error"
            href={routes.delete(
              { id },
              {
                accountingPeriodId: accountingPeriodId ?? null,
                accountId: accountId ?? null,
                fundId: fundId ?? null,
              },
            )}
          >
            Delete
          </Button>
        </Stack>
      </Stack>
      <CaptionedFrame caption="Details">
        <CaptionedValue
          caption="Type"
          value={transaction.type?.toString() ?? ""}
        />
        <CaptionedValue
          caption="Accounting Period"
          value={transaction.accountingPeriodName}
        />
        <CaptionedValue caption="Date" value={transaction.date} />
        <CaptionedValue
          caption="Amount"
          value={formatCurrency(transaction.amount)}
        />
        <CaptionedValue caption="Location" value={transaction.location} />
        <CaptionedValue caption="Description" value={transaction.description} />
      </CaptionedFrame>
      {"debitAccount" in transaction || "creditAccount" in transaction ? (
        <CaptionedFrame caption="Accounts">
          {"debitAccount" in transaction &&
          transaction.debitAccount !== null ? (
            <>
              <CaptionedValue
                caption="Debit Account"
                value={transaction.debitAccount.accountName}
              />
              <CaptionedValue
                caption="Debit Posted Date"
                value={transaction.debitAccount.postedDate ?? "Not Posted"}
              />
              <CaptionedValue
                caption="Debit New Posted Balance"
                value={formatCurrency(
                  transaction.debitAccount.newAccountBalance.postedBalance,
                )}
              />
            </>
          ) : null}
          {"creditAccount" in transaction &&
          transaction.creditAccount !== null ? (
            <>
              <CaptionedValue
                caption="Credit Account"
                value={transaction.creditAccount.accountName}
              />
              <CaptionedValue
                caption="Credit Posted Date"
                value={transaction.creditAccount.postedDate ?? "Not Posted"}
              />
              <CaptionedValue
                caption="Credit New Posted Balance"
                value={formatCurrency(
                  transaction.creditAccount.newAccountBalance.postedBalance,
                )}
              />
            </>
          ) : null}
        </CaptionedFrame>
      ) : null}
      {"fundAssignments" in transaction ? (
        <CaptionedFrame caption="Fund Assignments">
          {transaction.fundAssignments.map((fundAssignment) => (
            <CaptionedValue
              key={fundAssignment.fundId}
              caption={fundAssignment.fundName}
              value={formatCurrency(fundAssignment.amount)}
            />
          ))}
        </CaptionedFrame>
      ) : null}
      {"debitFund" in transaction ? (
        <CaptionedFrame caption="Funds">
          <CaptionedValue
            caption="Debit Fund"
            value={`${transaction.debitFund.fundName} (${formatCurrency(transaction.debitFund.amount)})`}
          />
          <CaptionedValue
            caption="Credit Fund"
            value={`${transaction.creditFund.fundName} (${formatCurrency(transaction.creditFund.amount)})`}
          />
        </CaptionedFrame>
      ) : null}
    </Stack>
  );
};

export type { TransactionDetailViewParams, TransactionDetailViewSearchParams };
export default TransactionDetailView;
