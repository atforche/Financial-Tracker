"use client";

import type {
  AccountingPeriodWithBalance,
  ExpectedIncomeSource,
} from "@/accounting-periods/types";
import { type JSX, useState } from "react";
import { Stack, Typography } from "@mui/material";
import DeleteExpectedIncomeSourceDialog from "@/accounting-periods/workspace/DeleteExpectedIncomeSourceDialog";
import ExpectedIncomeSourceActions from "@/accounting-periods/workspace/ExpectedIncomeSourceActions";
import PayrollIncomeDetails from "@/transactions/workspace/income/PayrollIncomeDetails";
import type { Route } from "next";
import { formatCurrency } from "@/framework/currencyHelpers";

/**
 * Props for the ExpectedIncomeSourceDetails component.
 */
interface ExpectedIncomeSourceDetailsProps {
  readonly accountingPeriod: AccountingPeriodWithBalance;
  readonly source: ExpectedIncomeSource;
  readonly backUrl: Route;
  readonly editUrl: Route;
  readonly periodIsOpen: boolean;
}

/**
 * Displays the complete expected-income source on its dedicated page.
 */
const ExpectedIncomeSourceDetails = function ({
  accountingPeriod,
  source,
  backUrl,
  editUrl,
  periodIsOpen,
}: ExpectedIncomeSourceDetailsProps): JSX.Element {
  const [deleteOpen, setDeleteOpen] = useState(false);
  return (
    <Stack spacing={3} sx={{ maxWidth: 1000, width: "100%" }}>
      <Stack direction={{ xs: "column", sm: "row" }} spacing={2}>
        <Typography>
          Expected income: {formatCurrency(source.expectedAmount)}
        </Typography>
        <Typography>
          Tracked per payment: {formatCurrency(source.trackedAmount)}
        </Typography>
        <Typography>
          Untracked per payment: {formatCurrency(source.untrackedAmount)}
        </Typography>
      </Stack>
      <Typography>Expected payments: {source.expectedDates.length}</Typography>
      <Typography color="text.secondary">
        Payment dates: {source.expectedDates.join(", ") || "None"}
      </Typography>
      <PayrollIncomeDetails
        stateIncomeStateCode={source.income.stateIncomeStateCode ?? null}
        setStateIncomeStateCode={null}
        earnings={source.income.earnings}
        setEarnings={null}
        deductions={source.income.employeeDeductions}
        setDeductions={null}
        contributions={source.income.employerContributions}
        setContributions={null}
        withholdings={source.income.taxWithholdings.map((item) => ({
            ...item,
            jurisdiction: {
              countryCode: item.jurisdiction.countryCode,
              subdivisionCode: item.jurisdiction.subdivisionCode ?? null,
              locality: item.jurisdiction.locality ?? null,
            },
          }))}
        setWithholdings={null}
      />
      <ExpectedIncomeSourceActions
        backUrl={backUrl}
        editUrl={editUrl}
        periodIsOpen={periodIsOpen}
        onDelete={() => {
          setDeleteOpen(true);
        }}
      />
      {deleteOpen ? (
        <DeleteExpectedIncomeSourceDialog
          accountingPeriod={accountingPeriod}
          source={source}
          redirectUrl={backUrl}
          onClose={() => {
            setDeleteOpen(false);
          }}
        />
      ) : null}
    </Stack>
  );
};

export default ExpectedIncomeSourceDetails;
