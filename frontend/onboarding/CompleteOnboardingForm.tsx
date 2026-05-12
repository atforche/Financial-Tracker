"use client";

import { Button, Paper, Stack, Typography } from "@mui/material";
import { type JSX, startTransition, useActionState } from "react";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import completeOnboarding from "@/onboarding/completeOnboarding";

/**
 * Displays the action that completes the captive onboarding flow.
 */
const CompleteOnboardingForm = function (): JSX.Element {
  const [state, action, pending] = useActionState(completeOnboarding, {});

  return (
    <Paper elevation={3} sx={{ maxWidth: 640, padding: 4, width: "100%" }}>
      <Stack spacing={3}>
        <Stack spacing={1}>
          <Typography variant="h4">Set Up Financial Tracker</Typography>
          <Typography color="text.secondary">
            This workspace is still in its first-run state. Complete onboarding
            before accessing accounts, funds, transactions, or accounting
            periods.
          </Typography>
        </Stack>
        <Typography color="text.secondary">
          You can finish onboarding now with an empty setup and create your
          first accounts and funds afterward.
        </Typography>
        <Button
          variant="contained"
          loading={pending}
          onClick={() => {
            startTransition(() => {
              action();
            });
          }}
        >
          Complete Onboarding
        </Button>
        <ErrorAlert
          errorMessage={state.errorTitle ?? null}
          unmappedErrors={state.unmappedErrors ?? null}
        />
      </Stack>
    </Paper>
  );
};

export default CompleteOnboardingForm;
