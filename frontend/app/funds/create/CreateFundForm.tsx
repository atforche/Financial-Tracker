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
import type { FundGoalType } from "@/data/fundTypes";
import FundGoalTypeEntryField from "@/framework/forms/FundGoalTypeEntryField";
import Link from "next/link";
import StringEntryField from "@/framework/forms/StringEntryField";
import createFund from "@/app/funds/create/createFund";
import routes from "@/framework/routes";

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
          {
            label: "Accounting Periods",
            href: routes.accountingPeriods.index,
          },
          {
            label: providedAccountingPeriod.name,
            href: routes.accountingPeriods.detail(providedAccountingPeriod.id),
          },
          {
            label: "Create Fund",
            href: routes.funds.create,
          },
        ]}
      />
    );
  }
  return (
    <Breadcrumbs
      breadcrumbs={[
        { label: "Funds", href: routes.funds.index },
        {
          label: "Create",
          href: routes.funds.create,
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
    return routes.accountingPeriods.detail(providedAccountingPeriod.id);
  }
  return routes.funds.index;
};

/**
 * Component that displays the form for creating a fund.
 */
const CreateFundForm = function ({
  accountingPeriods,
  providedAccountingPeriod = null,
}: CreateFundFormProps): JSX.Element {
  const [name, setName] = useState<string>("");
  const [description, setDescription] = useState<string>("");
  const [accountingPeriod, setAccountingPeriod] =
    useState<AccountingPeriod | null>(providedAccountingPeriod);

  const [specifyGoalAmount, setSpecifyGoalAmount] = useState<boolean>(false);
  const [goalType, setGoalType] = useState<FundGoalType | null>(null);
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
                  setGoalType(null);
                  setGoalAmount(null);
                }
                setSpecifyGoalAmount(e.target.checked);
              }}
            />
          }
          label="Set Goal Amount?"
        />
        {specifyGoalAmount ? (
          <>
            <FundGoalTypeEntryField
              label="Goal Type"
              value={goalType}
              setValue={setGoalType}
              errorMessage={state.goalTypeErrors ?? null}
            />
            <CurrencyEntryField
              label="Goal Amount"
              value={goalAmount}
              setValue={setGoalAmount}
              errorMessage={state.goalAmountErrors ?? null}
            />
          </>
        ) : null}
        <DialogActions>
          <Link href={getRedirectUrl(providedAccountingPeriod)} tabIndex={-1}>
            <Button variant="outlined">Cancel</Button>
          </Link>
          <Button
            variant="contained"
            loading={pending}
            disabled={
              name === "" ||
              accountingPeriod === null ||
              (specifyGoalAmount && (goalType === null || goalAmount === null))
            }
            onClick={() => {
              if (
                name === "" ||
                accountingPeriod === null ||
                (specifyGoalAmount &&
                  (goalType === null || goalAmount === null))
              ) {
                return;
              }
              startTransition(() => {
                action({
                  name,
                  description,
                  goalAmount,
                  goalType,
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
