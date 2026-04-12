"use client";

import {
  Button,
  Checkbox,
  DialogActions,
  FormControlLabel,
  Stack,
} from "@mui/material";
import { type JSX, startTransition, useActionState, useState } from "react";
import type { AccountingPeriod } from "@/data/accountingPeriodTypes";
import AccountingPeriodEntryField from "@/framework/forms/AccountingPeriodEntryField";
import Breadcrumbs from "@/framework/Breadcrumbs";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import type { FundType } from "@/data/fundTypes";
import FundTypeEntryField from "@/framework/forms/FundTypeEntryField";
import Link from "next/link";
import StringEntryField from "@/framework/forms/StringEntryField";
import createFund from "@/app/funds/create/createFund";

/**
 * Props for the CreateFundForm component.
 */
interface CreateFundFormProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly providedAccountingPeriod?: AccountingPeriod | null;
}

/**
 * Gets the breadcrumbs to be displayed at the top of the form.
 */
const getBreadcrumbs = function (
  providedAccountingPeriod: AccountingPeriod | null,
): JSX.Element {
  if (providedAccountingPeriod !== null) {
    return (
      <Breadcrumbs
        breadcrumbs={[
          { label: "Accounting Periods", href: "/accounting-periods" },
          {
            label: providedAccountingPeriod.name,
            href: `/accounting-periods/${providedAccountingPeriod.id}`,
          },
          {
            label: "Create Fund",
            href: `/funds/create`,
          },
        ]}
      />
    );
  }
  return (
    <Breadcrumbs
      breadcrumbs={[
        { label: "Funds", href: "/funds" },
        {
          label: "Create",
          href: `/funds/create`,
        },
      ]}
    />
  );
};

/**
 * Gets the URL to redirect the user to after successfully creating a fund.
 */
const getRedirectUrl = function (
  providedAccountingPeriod: AccountingPeriod | null,
): string {
  if (providedAccountingPeriod !== null) {
    return `/accounting-periods/${providedAccountingPeriod.id}`;
  }
  return "/funds";
};

/**
 * Component that displays the form for creating a fund.
 */
const CreateFundForm = function ({
  accountingPeriods,
  providedAccountingPeriod = null,
}: CreateFundFormProps): JSX.Element {
  const [name, setName] = useState<string>("");
  const [type, setType] = useState<FundType | null>(null);
  const [description, setDescription] = useState<string>("");
  const [accountingPeriod, setAccountingPeriod] =
    useState<AccountingPeriod | null>(providedAccountingPeriod);

  const [specifyGoalAmount, setSpecifyGoalAmount] = useState<boolean>(false);
  const [goalAmount, setGoalAmount] = useState<number | null>(null);

  const [state, action, pending] = useActionState(createFund, {
    redirectUrl: getRedirectUrl(providedAccountingPeriod),
  });

  return (
    <Stack spacing={2}>
      {getBreadcrumbs(providedAccountingPeriod)}
      <Stack spacing={2} sx={{ maxWidth: "500px" }}>
        <StringEntryField
          label="Name"
          value={name}
          setValue={setName}
          errorMessage={state.nameErrors ?? null}
        />
        <FundTypeEntryField
          label="Type"
          value={type}
          setValue={setType}
          errorMessage={state.typeErrors ?? null}
        />
        <StringEntryField
          label="Description"
          value={description}
          setValue={setDescription}
          errorMessage={state.descriptionErrors ?? null}
        />
        <AccountingPeriodEntryField
          label="Accounting Period"
          options={accountingPeriods}
          value={accountingPeriod}
          setValue={
            providedAccountingPeriod === null ? setAccountingPeriod : null
          }
          errorMessage={state.accountingPeriodErrors ?? null}
        />
        <FormControlLabel
          control={
            <Checkbox
              checked={specifyGoalAmount}
              onChange={(e) => {
                if (!e.target.checked) {
                  setGoalAmount(null);
                }
                setSpecifyGoalAmount(e.target.checked);
              }}
            />
          }
          label="Set Goal Amount?"
        />
        {specifyGoalAmount ? (
          <CurrencyEntryField
            label="Goal Amount"
            value={goalAmount}
            setValue={setGoalAmount}
            errorMessage={state.goalAmountErrors ?? null}
          />
        ) : null}
        <DialogActions>
          <Link href={getRedirectUrl(providedAccountingPeriod)} tabIndex={-1}>
            <Button variant="outlined">Cancel</Button>
          </Link>
          <Button
            variant="contained"
            loading={pending}
            disabled={name === "" || type === null || accountingPeriod === null}
            onClick={() => {
              if (name === "" || type === null || accountingPeriod === null) {
                return;
              }
              startTransition(() => {
                action({
                  name,
                  type,
                  description,
                  goalAmount,
                  accountingPeriodId: accountingPeriod.id,
                });
              });
            }}
          >
            Create
          </Button>
        </DialogActions>
        <ErrorAlert
          errorMessage={state.errorTitle ?? null}
          unmappedErrors={state.unmappedErrors ?? null}
        />
      </Stack>
    </Stack>
  );
};

export default CreateFundForm;
