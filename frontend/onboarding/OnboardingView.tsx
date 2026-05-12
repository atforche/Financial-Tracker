import { Stack, Typography } from "@mui/material";
import CompleteOnboardingForm from "@/onboarding/CompleteOnboardingForm";
import type { JSX } from "react";

/**
 * Displays the captive onboarding screen.
 */
const OnboardingView = function (): JSX.Element {
  return (
    <Stack spacing={3} alignItems="center">
      <Typography variant="overline" color="text.secondary">
        Initial Setup Required
      </Typography>
      <CompleteOnboardingForm />
    </Stack>
  );
};

export default OnboardingView;
