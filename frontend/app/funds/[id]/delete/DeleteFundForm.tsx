"use client";

import { Button, DialogActions, Stack, Typography } from "@mui/material";
import { type JSX, startTransition, useActionState } from "react";
import type { AccountingPeriod } from "@/data/accountingPeriodTypes";
import Breadcrumbs from "@/framework/Breadcrumbs";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import type { Fund } from "@/data/fundTypes";
import Link from "next/link";
import deleteFund from "@/app/funds/[id]/delete/deleteFund";
import routes from "@/framework/routes";

/**
 * Props for the DeleteFundForm component.
 */
interface DeleteFundFormProps {
  readonly fund: Fund;
  readonly accountingPeriod: AccountingPeriod | null;
}

/**
 * Gets the breadcrumbs to display at the top of the form.
 */
const getBreadcrumbs = function (
  fund: Fund,
  accountingPeriod: AccountingPeriod | null,
): JSX.Element {
  if (accountingPeriod !== null) {
    return (
      <Breadcrumbs
        breadcrumbs={[
          {
            label: "Accounting Periods",
            href: routes.accountingPeriods.index,
          },
          {
            label: accountingPeriod.name,
            href: routes.accountingPeriods.detail(accountingPeriod.id),
          },
          {
            label: fund.name,
            href: routes.accountingPeriods.fundDetail(
              accountingPeriod.id,
              fund.id,
            ),
          },
          {
            label: "Delete",
            href: routes.funds.delete(fund.id),
          },
        ]}
      />
    );
  }
  return (
    <Breadcrumbs
      breadcrumbs={[
        { label: "Funds", href: routes.funds.index },
        { label: fund.name, href: routes.funds.detail(fund.id) },
        {
          label: "Delete",
          href: routes.funds.delete(fund.id),
        },
      ]}
    />
  );
};

/**
 * Gets the URL to redirect to after the fund is deleted.
 */
const getRedirectUrl = function (
  accountingPeriod: AccountingPeriod | null,
): string {
  if (accountingPeriod !== null) {
    return routes.accountingPeriods.detail(accountingPeriod.id);
  }
  return routes.funds.index;
};

/**
 * Component that displays the form for deleting a fund.
 */
const DeleteFundForm = function ({
  fund,
  accountingPeriod,
}: DeleteFundFormProps): JSX.Element {
  const [state, action, pending] = useActionState(deleteFund, {
    fundId: fund.id,
    redirectUrl: getRedirectUrl(accountingPeriod),
  });

  return (
    <Stack spacing={2}>
      {getBreadcrumbs(fund, accountingPeriod)}
      <Stack spacing={2} sx={{ maxWidth: "500px" }}>
        <Typography>
          Are you sure you want to delete the fund &quot;
          {fund.name}&quot;?
        </Typography>
        <DialogActions>
          <Link href={getRedirectUrl(accountingPeriod)} tabIndex={-1}>
            <Button variant="outlined">Cancel</Button>
          </Link>
          <Button
            variant="contained"
            loading={pending}
            onClick={() => {
              startTransition(() => {
                action();
              });
            }}
          >
            Delete
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

export default DeleteFundForm;
