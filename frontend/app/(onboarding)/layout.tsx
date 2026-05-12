import { Box } from "@mui/material";
import type { JSX } from "react";
import getOnboardingEligibility from "@/onboarding/getOnboardingEligibility";
import { redirect } from "next/navigation";

/**
 * Layout for the onboarding-only route group.
 */
const OnboardingLayout = async function ({
  children,
}: Readonly<{
  children: JSX.Element;
}>): Promise<JSX.Element> {
  const eligibility = await getOnboardingEligibility();
  if (!eligibility.isEligible) {
    redirect("/");
  }

  return (
    <Box
      sx={{
        alignItems: "center",
        display: "flex",
        justifyContent: "center",
        minHeight: "100vh",
        padding: 3,
      }}
    >
      {children}
    </Box>
  );
};

export const dynamic = "force-dynamic";
export default OnboardingLayout;
