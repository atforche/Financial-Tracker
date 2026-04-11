"use client";

import {
  type AccountingPeriod,
  getDefaultDate,
} from "@/data/accountingPeriodTypes";
import {
  type CreateTransactionFormSearchParams,
  getDefaultAccountingPeriod,
  getDefaultCreditAccount,
  getDefaultDebitAccount,
  getInitialToggleState,
} from "@/app/transactions/create/createTransactionFormSearchParams";
import type { FundAmount, FundIdentifier } from "@/data/fundTypes";
import { type JSX, useState } from "react";
import type { AccountIdentifier } from "@/data/accountTypes";
import Breadcrumbs from "@/framework/Breadcrumbs";
import CreateTransactionAccountFrame from "@/app/transactions/create/CreateTransactionAccountFrame";
import CreateTransactionActionsFrame from "@/app/transactions/create/CreateTransactionActionsFrame";
import CreateTransactionDetailsFrame from "@/app/transactions/create/CreateTransactionDetailsFrame";
import type { Dayjs } from "dayjs";
import { Stack } from "@mui/material";
import type ToggleState from "@/app/transactions/create/toggleState";
import getBreadcrumbs from "@/app/transactions/create/getBreadcrumbs";

/**
 * Props for the CreateTransactionForm component.
 */
interface CreateTransactionFormProps {
  readonly searchParams: CreateTransactionFormSearchParams;
  readonly accountingPeriods: AccountingPeriod[];
  readonly accounts: AccountIdentifier[];
  readonly funds: FundIdentifier[];
}

/**
 * Component that displays the form for creating a transaction.
 */
const CreateTransactionForm = function ({
  searchParams,
  accountingPeriods,
  accounts,
  funds,
}: CreateTransactionFormProps): JSX.Element {
  const [accountingPeriod, setAccountingPeriod] =
    useState<AccountingPeriod | null>(
      getDefaultAccountingPeriod(accountingPeriods, searchParams),
    );
  const [date, setDate] = useState<Dayjs | null>(null);
  const [location, setLocation] = useState<string>("");
  const [description, setDescription] = useState<string>("");
  const [debitAccount, setDebitAccount] = useState<AccountIdentifier | null>(
    getDefaultDebitAccount(accounts, searchParams),
  );
  const [debitFundAmounts, setDebitFundAmounts] = useState<FundAmount[]>([]);
  const [debitPostedDate, setDebitPostedDate] = useState<Dayjs | null>(null);
  const [creditAccount, setCreditAccount] = useState<AccountIdentifier | null>(
    getDefaultCreditAccount(accounts, searchParams),
  );
  const [creditFundAmounts, setCreditFundAmounts] = useState<FundAmount[]>([]);
  const [creditPostedDate, setCreditPostedDate] = useState<Dayjs | null>(null);
  const [toggleState, setToggleState] = useState<ToggleState>(
    getInitialToggleState(searchParams),
  );

  const breadcrumbs = getBreadcrumbs(
    searchParams,
    accountingPeriods,
    accounts,
    funds,
  );
  const redirectUrl = breadcrumbs.at(-2)?.href ?? "/transactions";

  return (
    <Stack spacing={2}>
      <Breadcrumbs breadcrumbs={breadcrumbs} />
      <CreateTransactionDetailsFrame
        searchParams={searchParams}
        accountingPeriods={accountingPeriods}
        accountingPeriod={accountingPeriod}
        setAccountingPeriod={setAccountingPeriod}
        date={date}
        setDate={setDate}
        location={location}
        setLocation={setLocation}
        description={description}
        setDescription={setDescription}
      />
      <CreateTransactionAccountFrame
        searchParams={searchParams}
        accounts={accounts}
        funds={funds}
        toggleState={toggleState}
        setToggleState={setToggleState}
        debitAccount={debitAccount}
        setDebitAccount={setDebitAccount}
        debitFundAmounts={debitFundAmounts}
        setDebitFundAmounts={setDebitFundAmounts}
        creditAccount={creditAccount}
        setCreditAccount={setCreditAccount}
        creditFundAmounts={creditFundAmounts}
        setCreditFundAmounts={setCreditFundAmounts}
        date={date ?? getDefaultDate(accountingPeriod)}
        debitPostedDate={debitPostedDate}
        setDebitPostedDate={setDebitPostedDate}
        creditPostedDate={creditPostedDate}
        setCreditPostedDate={setCreditPostedDate}
      />
      <CreateTransactionActionsFrame
        redirectUrl={redirectUrl}
        toggleState={toggleState}
        accountingPeriod={accountingPeriod}
        date={date}
        location={location}
        description={description}
        debitAccount={debitAccount}
        debitFundAmounts={debitFundAmounts}
        creditAccount={creditAccount}
        creditFundAmounts={creditFundAmounts}
        debitPostedDate={debitPostedDate}
        creditPostedDate={creditPostedDate}
      />
    </Stack>
  );
};

export default CreateTransactionForm;
