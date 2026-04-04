"use client";

import type { Fund, FundAmount, FundIdentifier } from "@/data/fundTypes";
import { type JSX, useState } from "react";
import dayjs, { type Dayjs } from "dayjs";
import type { Account } from "@/data/accountTypes";
import type { AccountingPeriod } from "@/data/accountingPeriodTypes";
import Breadcrumbs from "@/framework/Breadcrumbs";
import { Stack } from "@mui/material";
import type { Transaction } from "@/data/transactionTypes";
import UpdateTransactionAccountFrame from "@/app/transactions/[id]/update/UpdateTransactionAccountFrame";
import UpdateTransactionActionsFrame from "@/app/transactions/[id]/update/UpdateTransactionActionsFrame";
import UpdateTransactionDetailsFrame from "@/app/transactions/[id]/update/UpdateTransactionDetailsFrame";
import getBreadcrumbs from "@/app/transactions/[id]/update/getBreadcrumbs";

/**
 * Props for the UpdateTransactionForm component.
 */
interface UpdateTransactionFormProps {
  readonly transaction: Transaction;
  readonly accountingPeriod: AccountingPeriod;
  readonly funds: FundIdentifier[];
  readonly providedAccountingPeriod?: AccountingPeriod | null;
  readonly providedAccount?: Account | null;
  readonly providedFund?: Fund | null;
}

/**
 * Component that displays the form for updating a transaction.
 */
const UpdateTransactionForm = function ({
  transaction,
  accountingPeriod,
  funds,
  providedAccountingPeriod = null,
  providedAccount = null,
  providedFund = null,
}: UpdateTransactionFormProps): JSX.Element {
  const [date, setDate] = useState<Dayjs | null>(dayjs(transaction.date));
  const [location, setLocation] = useState<string>(transaction.location);
  const [description, setDescription] = useState<string>(
    transaction.description,
  );
  const [debitFundAmounts, setDebitFundAmounts] = useState<FundAmount[]>(
    transaction.debitAccount?.fundAmounts.map((fa) => ({
      fundId: fa.fundId,
      fundName: fa.fundName,
      amount: fa.amount,
    })) ?? [],
  );
  const [creditFundAmounts, setCreditFundAmounts] = useState<FundAmount[]>(
    transaction.creditAccount?.fundAmounts.map((fa) => ({
      fundId: fa.fundId,
      fundName: fa.fundName,
      amount: fa.amount,
    })) ?? [],
  );

  const breadcrumbs = getBreadcrumbs(
    transaction,
    providedAccountingPeriod,
    providedAccount,
    providedFund,
  );
  const redirectUrl = breadcrumbs[breadcrumbs.length - 2]?.href ?? "/";

  return (
    <Stack spacing={2}>
      <Breadcrumbs
        breadcrumbs={getBreadcrumbs(
          transaction,
          providedAccountingPeriod,
          providedAccount,
          providedFund,
        )}
      />
      <UpdateTransactionDetailsFrame
        accountingPeriod={accountingPeriod}
        date={date}
        setDate={setDate}
        location={location}
        setLocation={setLocation}
        description={description}
        setDescription={setDescription}
      />
      <UpdateTransactionAccountFrame
        transaction={transaction}
        funds={funds}
        debitFundAmounts={debitFundAmounts}
        setDebitFundAmounts={setDebitFundAmounts}
        creditFundAmounts={creditFundAmounts}
        setCreditFundAmounts={setCreditFundAmounts}
      />
      <UpdateTransactionActionsFrame
        redirectUrl={redirectUrl}
        transaction={transaction}
        date={date}
        location={location}
        description={description}
        debitFundAmounts={debitFundAmounts}
        creditFundAmounts={creditFundAmounts}
      />
    </Stack>
  );
};

export default UpdateTransactionForm;
