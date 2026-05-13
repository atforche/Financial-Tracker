"use client";

import { Button, DialogActions, Stack, Typography } from "@mui/material";
import { type JSX, startTransition, useActionState, useState } from "react";
import Breadcrumbs from "@/framework/Breadcrumbs";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import Link from "next/link";
import type { OnboardFundRequest } from "@/funds/types";
import StringEntryField from "@/framework/forms/StringEntryField";
import breadcrumbs from "@/funds/breadcrumbs";
import formatCurrency from "@/framework/formatCurrency";
import onboardFund from "@/funds/onboardFund";
import routes from "@/funds/routes";

/**
 * Props for the OnboardFundForm component.
 */
interface OnboardFundFormProps {
  readonly unassignedBalance: number | null;
}

/**
 * Component that displays the form for onboarding a fund.
 */
const OnboardFundForm = function ({
  unassignedBalance,
}: OnboardFundFormProps): JSX.Element {
  const [name, setName] = useState<string>("");
  const [description, setDescription] = useState<string>("");
  const [onboardedBalance, setOnboardedBalance] = useState<number | null>(null);
  const [state, action, pending] = useActionState(onboardFund, {});

  const remainingUnassignedAmount =
    unassignedBalance === null
      ? null
      : unassignedBalance - (onboardedBalance ?? 0);

  let request: OnboardFundRequest | null = null;
  if (name !== "" && onboardedBalance !== null) {
    request = {
      name,
      description,
      onboardedBalance,
    };
  }

  return (
    <Stack spacing={2}>
      <Breadcrumbs breadcrumbs={breadcrumbs.onboard()} />
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
        <CurrencyEntryField
          label="Starting Balance"
          value={onboardedBalance}
          setValue={setOnboardedBalance}
          errorMessage={state.onboardedBalanceErrors ?? null}
        />
        {remainingUnassignedAmount !== null ? (
          <Typography
            variant="body2"
            sx={{
              color:
                remainingUnassignedAmount < 0 ? "error.main" : "text.secondary",
            }}
          >
            Remaining Unassigned Balance:{" "}
            {formatCurrency(remainingUnassignedAmount)}
          </Typography>
        ) : null}
        <DialogActions>
          <Link href={routes.index({})} tabIndex={-1}>
            <Button variant="outlined">Cancel</Button>
          </Link>
          <Button
            variant="contained"
            loading={pending}
            disabled={request === null}
            onClick={() => {
              if (request === null) {
                return;
              }
              startTransition(() => {
                action(request);
              });
            }}
          >
            Onboard
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

export default OnboardFundForm;
