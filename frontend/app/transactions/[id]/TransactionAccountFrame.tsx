import { Button, Stack } from "@mui/material";
import {
  type Transaction,
  type TransactionAccount,
  TransactionAccountType,
} from "@/data/transactionTypes";
import Caption from "@/framework/view/Caption";
import CaptionedFrame from "@/framework/view/CaptionedFrame";
import CaptionedValue from "@/framework/view/CaptionedValue";
import type { JSX } from "react";
import TransactionAccountBalanceFrame from "@/app/transactions/[id]/TransactionAccountBalanceFrame";
import type TransactionSearchParams from "@/app/transactions/[id]/transactionSearchParams";
import dayjs from "dayjs";

/**
 * Props for the TransactionAccountFrame component.
 */
interface TransactionAccountFrameProps {
  readonly transaction: Transaction;
  readonly transactionAccount: TransactionAccount;
  readonly searchParams: TransactionSearchParams;
  readonly isAccountingPeriodOpen: boolean;
}

/**
 * Component that presents the user with a frame showing Transaction Account details.
 */
const TransactionAccountFrame = function ({
  transaction,
  transactionAccount,
  searchParams,
  isAccountingPeriodOpen,
}: TransactionAccountFrameProps): JSX.Element {
  const postParams = new URLSearchParams();
  const unpostParams = new URLSearchParams();

  postParams.set(
    "account",
    transactionAccount.type === TransactionAccountType.Debit
      ? "debit"
      : "credit",
  );
  if (typeof searchParams.accountingPeriodId !== "undefined") {
    unpostParams.set("accountingPeriodId", searchParams.accountingPeriodId);
    postParams.set("accountingPeriodId", searchParams.accountingPeriodId);
  }
  if (typeof searchParams.accountId !== "undefined") {
    unpostParams.set("accountId", searchParams.accountId);
    postParams.set("accountId", searchParams.accountId);
  }
  if (typeof searchParams.fundId !== "undefined") {
    unpostParams.set("fundId", searchParams.fundId);
    postParams.set("fundId", searchParams.fundId);
  }
  const postHref = `/transactions/${transaction.id}/post?${postParams.toString()}`;
  const unpostHref = `/transactions/${transaction.id}/unpost?${unpostParams.toString()}`;

  return (
    <CaptionedFrame
      caption={
        transactionAccount.type === TransactionAccountType.Credit
          ? "Credit Account"
          : "Debit Account"
      }
      minWidth={500}
      maxWidth={null}
    >
      <Stack direction="row" justifyContent="space-between">
        <CaptionedValue
          caption="Account"
          value={transactionAccount.accountName}
          minWidth={500}
        />
        {transactionAccount.postedDate === null ? (
          <Button
            variant="contained"
            color="primary"
            href={postHref}
            disabled={!isAccountingPeriodOpen}
          >
            Post
          </Button>
        ) : (
          <Button
            variant="contained"
            color="primary"
            href={unpostHref}
            disabled={!isAccountingPeriodOpen}
          >
            Unpost
          </Button>
        )}
      </Stack>
      <CaptionedValue
        caption="Posted On"
        value={
          transactionAccount.postedDate === null
            ? ""
            : dayjs(transactionAccount.postedDate).format("MMMM DD, YYYY")
        }
      />
      <br />
      <Caption caption="Account Balance" />
      <TransactionAccountBalanceFrame transactionAccount={transactionAccount} />
    </CaptionedFrame>
  );
};

export default TransactionAccountFrame;
