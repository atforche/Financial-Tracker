"use client";

import type { Dispatch, JSX, RefObject, SetStateAction } from "react";
import {
  type RefundDestinationDraft,
  type RefundSourceDraft,
  buildDestinationAccountFilter,
  buildSourceAccountFilter,
  createEmptySource,
  validateDestination,
  validateSource,
} from "@/transactions/workspace/refund/helpers";
import type { AccountWithBalance } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { Dayjs } from "dayjs";
import type { FundGoalWithProgress } from "@/fund-goals/types";
import type { FundWithBalance } from "@/funds/types";
import RefundTransactionDestinationFrame from "@/transactions/workspace/refund/RefundTransactionDestinationFrame";
import RefundTransactionSourceFrame from "@/transactions/workspace/refund/RefundTransactionSourceFrame";
import TransactionForm from "@/transactions/workspace/TransactionForm";
import { getCurrencyTotal } from "@/framework/currencyHelpers";
import { updateUnassignedFundAmount } from "@/funds/assignmentPlanner/helpers";

/**
 * Represents the state of the refund transaction form.
 */
interface FormState {
  readonly success?: boolean;
  readonly transactionId?: string | null;
  readonly errorTitle?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Props for the RefundTransactionForm component.
 */
interface Props<RequestPayload> {
  readonly formRef: RefObject<HTMLDivElement | null>;
  readonly accounts: AccountWithBalance[];
  readonly funds: FundWithBalance[];
  readonly fundGoals: FundGoalWithProgress[];
  readonly accountingPeriods: AccountingPeriod[];
  readonly accountingPeriod: AccountingPeriod | null;
  readonly setAccountingPeriod?: Dispatch<
    SetStateAction<AccountingPeriod | null>
  > | null;
  readonly date: Dayjs | null;
  readonly setDate: Dispatch<SetStateAction<Dayjs | null>>;
  readonly defaultDate: Dayjs | null;
  readonly description: string;
  readonly setDescription: Dispatch<SetStateAction<string>>;
  readonly sources: RefundSourceDraft[];
  readonly setSources: Dispatch<SetStateAction<RefundSourceDraft[]>>;
  readonly destination: RefundDestinationDraft;
  readonly setDestination: Dispatch<SetStateAction<RefundDestinationDraft>>;
  readonly submitLabel: string;
  readonly state: FormState;
  readonly pending: boolean;
  readonly request: RequestPayload | null;
  readonly onReset: () => void;
  readonly onSubmit: (request: RequestPayload) => void;
}

/**
 * Displays the shared refund transaction form layout used by create and update flows.
 */
const RefundTransactionForm = function <RequestPayload>({
  formRef,
  accounts,
  funds,
  fundGoals,
  accountingPeriods,
  accountingPeriod,
  setAccountingPeriod = null,
  date,
  setDate,
  defaultDate,
  description,
  setDescription,
  sources,
  setSources,
  destination,
  setDestination,
  submitLabel,
  state,
  pending,
  request,
  onReset,
  onSubmit,
}: Props<RequestPayload>): JSX.Element {
  const currentGoals = fundGoals.filter(
    (goal) => goal.accountingPeriod?.id === accountingPeriod?.id,
  );
  const unassigned = funds.find((fund) => fund.name === "Unassigned") ?? null;
  const total = getCurrencyTotal(sources.map((source) => source.amount));
  const updateSource = (
    index: number,
    recipe: (source: RefundSourceDraft) => RefundSourceDraft,
  ): void => {
    setSources((items) =>
      items.map((item, i) => (i === index ? recipe(item) : item)),
    );
  };

  return (
    <TransactionForm
      formRef={formRef}
      accountingPeriods={accountingPeriods}
      accountingPeriod={accountingPeriod}
      setAccountingPeriod={setAccountingPeriod}
      date={date}
      setDate={setDate}
      defaultDate={defaultDate}
      description={description}
      setDescription={setDescription}
      sourceContent={
        <>
          {sources.map((source, index) => (
            <RefundTransactionSourceFrame
              key={`refund-source-${index}`}
              index={index}
              accounts={accounts}
              funds={funds}
              fundGoals={currentGoals}
              account={source.account}
              setAccount={(account) => {
                updateSource(index, (item) => ({
                  ...item,
                  account,
                  location: account === null ? item.location : null,
                }));
              }}
              location={source.location}
              setLocation={(location) => {
                updateSource(index, (item) => ({
                  ...item,
                  location,
                  account: location === null ? item.account : null,
                }));
              }}
              amount={source.amount}
              setAmount={(amount) => {
                updateSource(index, (item) => ({
                  ...item,
                  amount,
                  fundAssignments: updateUnassignedFundAmount(
                    unassigned,
                    amount,
                    item.fundAssignments,
                  ),
                }));
              }}
              fundAssignments={source.fundAssignments}
              setFundAssignments={(fundAssignments) => {
                updateSource(index, (item) => ({ ...item, fundAssignments }));
              }}
              baselineFundAssignments={source.baselineFundAssignments}
              accountFilter={buildSourceAccountFilter(
                accounts,
                sources,
                index,
                destination,
              )}
              color={validateSource(source) ? "info" : "error"}
              onAdd={
                index === 0
                  ? (): void => {
                      setSources((items) => [...items, createEmptySource()]);
                    }
                  : null
              }
              onRemove={
                sources.length > 1
                  ? (): void => {
                      setSources((items) =>
                        items.filter((_, i) => i !== index),
                      );
                    }
                  : null
              }
            />
          ))}
        </>
      }
      destinationContent={
        <RefundTransactionDestinationFrame
          accounts={accounts}
          account={destination.account}
          setAccount={(account) => {
            setDestination({ account });
          }}
          amount={total}
          filter={buildDestinationAccountFilter(accounts, sources)}
          color={validateDestination(destination, sources) ? "info" : "error"}
        />
      }
      sourceAmount={total}
      destinationAmount={total}
      destinationCount={1}
      submitLabel={submitLabel}
      state={state}
      pending={pending}
      request={request}
      onReset={onReset}
      onSubmit={onSubmit}
    />
  );
};

export default RefundTransactionForm;
